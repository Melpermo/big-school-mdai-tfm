using DG.Tweening;
using HumanLoop.Core;
using HumanLoop.Data;
using HumanLoop.Events;
using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HumanLoop.UI
{
    /// <summary>
    /// Handles all card animations, drag mechanics, and lifecycle.
    /// Calls CardDisplay for visual state changes.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CardDisplay))]
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static event Action OnCardRemoved;
        public static event Action<CardDataSO, bool> OnCardRemovedWithDecision;

        [Header("Swipe Settings")]
        [SerializeField] private float fovAngle = 15f;
        [SerializeField] private float swipeThreshold = 150f;
        [SerializeField] private float swipeExitX = 1000f;

        [Header("Animation Timing")]
        [SerializeField] private float returnDuration = 0.3f;
        [SerializeField] private float swipeDuration = 0.4f;
        [SerializeField] private float flipDuration = 0.5f;
        [SerializeField] private float appearDuration = 0.4f;

        [Header("Platform Optimization")]
        [Tooltip("If false, cards appear instantly without flip animation (recommended for WebGL if OOM issues persist)")]
        [SerializeField] private bool enableFlipAnimation = true;

        [Tooltip("Duration for instant appear fade when flip is disabled")]
        [SerializeField] private float instantAppearFadeDuration = 0.2f;

        [Header("Dependencies")]
        [SerializeField] private CardFactory cardFactory;

        [Header("Events")]
        [SerializeField] private GameEventSO onCardSwipedEvent;
        [SerializeField] private GameEventSO onCardReturnedEvent;
        [SerializeField] private GameEventSO onCardFlipEvent;
        [SerializeField] private GameEventSO onCardAddedEvent;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Cached components
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private CardDisplay _display;

        // State
        private Vector2 _startPosition;
        private float _lockedYPosition; // ← Y axis lock
        private bool _isBusy;
        private bool _isInBackground;

        // Tween management
        private Sequence _activeSequence;
        private Tween _currentTween;
        private Tween _delayedFaceSwitch; // ← NUEVO: Para sincronizar cara

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            _startPosition = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            CleanupTweens();
            _isBusy = false;
            SetRaycast(true);
        }

        private void OnDisable()
        {
            CleanupTweens();
        }

        private void OnDestroy()
        {
            CleanupTweens();
        }

        #endregion

        #region Component Caching

        private void CacheComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _display = GetComponent<CardDisplay>();
        }

        #endregion

        #region Public API - Setup

        public void SetFactory(CardFactory factory)
        {
            cardFactory = factory;
        }

        public void ResetCardController()
        {
            CleanupTweens();

            _startPosition = Vector2.zero;
            _rectTransform.anchoredPosition = _startPosition;
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.localScale = Vector3.one;

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false; // Enabled after flip

            _display.HideChoices();

            _isBusy = false;
            _isInBackground = false;
            SetRaycast(false);
        }

        #endregion

        #region Public API - Animations

        /// <summary>
        /// Animates card appearing with scale + fade.
        /// </summary>
        public void AnimateIn()
        {
            if (showDebugLogs)
                Debug.Log($"[CardController] AnimateIn: {_display.Data?.cardName}");

            CleanupTweens();

            _rectTransform.localScale = Vector3.one * 0.5f;
            _canvasGroup.alpha = 0f;

            _activeSequence = DOTween.Sequence()
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true);

            _activeSequence.Append(_rectTransform.DOScale(1f, appearDuration).SetEase(Ease.OutBack));
            _activeSequence.Join(_canvasGroup.DOFade(1f, appearDuration * 0.75f));

            _activeSequence.OnComplete(() => {
                _activeSequence = null;
                if (showDebugLogs)
                    Debug.Log($"[CardController] AnimateIn complete");
            });

            onCardAddedEvent?.Raise();
        }

        /// <summary>
        /// Sets card as background (scaled down, back face, no interaction).
        /// </summary>
        public void SetAsBackground()
        {
            if (showDebugLogs)
                Debug.Log($"[CardController] SetAsBackground: {_display.Data?.cardName}");

            _display.ShowBackFace();

            _rectTransform.localEulerAngles = new Vector3(0, 180f, 0);
            _rectTransform.localScale = Vector3.one * 0.9f;
            _rectTransform.SetAsFirstSibling();

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            _isInBackground = true;
        }

        /// <summary>
        /// Animates flip from background to front, or appears instantly if flip disabled.
        /// </summary>
        public void FlipToFront()
        {
            if (showDebugLogs)
                Debug.Log($"[CardController] FlipToFront: {_display.Data?.cardName}, IsBackground: {_isInBackground}, EnableFlip: {enableFlipAnimation}");

            _rectTransform.SetAsLastSibling();

            // Check if flip animation is enabled
            if (enableFlipAnimation)
            {
                // NORMAL: Full flip animation
                if (_isInBackground)
                {
                    AnimateFlipFromBackground();
                }
                else
                {
                    AnimateFlipFresh();
                }
            }
            else
            {
                // WEBGL FALLBACK: Instant appear without flip
                InstantAppearFromBackground();
            }

            _isInBackground = false;
            onCardFlipEvent?.Raise();
        }

        /// <summary>
        /// Instantly shows card without flip animation.
        /// WebGL-optimized: minimal memory usage, no rotation tweens.
        /// </summary>
        private void InstantAppearFromBackground()
        {
            CleanupTweens();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Set to front orientation immediately
            _rectTransform.localEulerAngles = Vector3.zero;
            _rectTransform.localScale = Vector3.one;

            // Show front face immediately (triggers fade in CardDisplay)
            _display.ShowFrontFace();

            // Simple fade-in sequence (much lighter than full flip)
            _activeSequence = DOTween.Sequence()
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true);

            // Just fade in the canvas group
            _activeSequence.Append(
                _canvasGroup.DOFade(1f, instantAppearFadeDuration)
                    .SetEase(Ease.OutQuad)
            );

            _activeSequence.OnComplete(OnFlipComplete);

            if (showDebugLogs)
                Debug.Log($"[CardController] Instant appear (no flip) complete");
        }

        #endregion

        #region Drag Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isBusy) return;
            CleanupTweens();

            // Capture the Y position at the start of the drag
            _lockedYPosition = _rectTransform.anchoredPosition.y;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isBusy) return;

            // Only apply horizontal movement (X-axis)
            Vector2 currentPos = _rectTransform.anchoredPosition;
            currentPos.x += eventData.delta.x; // Just modify X
            currentPos.y = _lockedYPosition;   // Hold Y constant
            _rectTransform.anchoredPosition = currentPos;
            
            float xOffset = _rectTransform.anchoredPosition.x - _startPosition.x;
            float normalizedOffset = Mathf.Clamp(xOffset / swipeThreshold, -1f, 1f);

            float rotationZ = -normalizedOffset * fovAngle;
            _rectTransform.localEulerAngles = new Vector3(0f, 0f, rotationZ);

            _display.UpdateChoiceVisuals(normalizedOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isBusy) return;

            float xOffset = _rectTransform.anchoredPosition.x - _startPosition.x;

            if (Mathf.Abs(xOffset) > swipeThreshold)
            {
                SwipeCard(xOffset > 0);
            }
            else
            {
                ReturnToCenter();
            }
        }

        #endregion

        #region Animation Methods

        private void AnimateFlipFromBackground()
        {
            CleanupTweens();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Force start from 180°
            _rectTransform.localEulerAngles = new Vector3(0, 180f, 0);

            // NUEVO: Programar cambio de cara a 90° (mitad del flip)
            float halfFlipDuration = flipDuration * 0.5f;
            _delayedFaceSwitch = DOVirtual.DelayedCall(halfFlipDuration, () => {
                _display.ShowFrontFace();
                _delayedFaceSwitch = null;
            })
            .SetTarget(this)
            .SetUpdate(true)
            .SetAutoKill(true);

            // Animación principal (sin callback)
            _activeSequence = DOTween.Sequence()
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true);

            // Rotate 180° → 0° (continuo, sin paradas)
            _activeSequence.Append(
                _rectTransform.DOLocalRotate(Vector3.zero, flipDuration, RotateMode.Fast)
                    .SetEase(Ease.OutQuad)
            );

            // Scale 0.9 → 1.0 (paralelo)
            _activeSequence.Join(
                _rectTransform.DOScale(1f, flipDuration)
                    .SetEase(Ease.OutBack)
            );

            _activeSequence.OnComplete(OnFlipComplete);
        }

        private void AnimateFlipFresh()
        {
            CleanupTweens();

            _display.ShowFrontFace();
            _rectTransform.localEulerAngles = Vector3.zero;
            _rectTransform.localScale = Vector3.one * 0.8f;

            _activeSequence = DOTween.Sequence()
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true);

            _activeSequence.Append(
                _rectTransform.DOScale(1f, flipDuration)
                    .SetEase(Ease.OutBack)
            );

            _activeSequence.OnComplete(OnFlipComplete);
        }

        private void OnFlipComplete()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;

                if (showDebugLogs)
                    Debug.Log($"[CardController] Flip complete, raycasts enabled");
            }

            _activeSequence = null;
        }

        private void ReturnToCenter()
        {
            _isBusy = true;
            SetRaycast(false);
            CleanupTweens();

            _activeSequence = DOTween.Sequence()
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true);

            _activeSequence.Append(_rectTransform.DOAnchorPos(_startPosition, returnDuration).SetEase(Ease.OutBack));
            _activeSequence.Join(_rectTransform.DORotate(Vector3.zero, returnDuration).SetEase(Ease.OutBack));

            _activeSequence.OnComplete(OnReturnComplete);

            _display.HideChoices();
            onCardReturnedEvent?.Raise();
        }

        private void OnReturnComplete()
        {
            _activeSequence = null;
            FinishBusy();
        }

        private void SwipeCard(bool isRight)
        {
            _isBusy = true;
            SetRaycast(false);
            CleanupTweens();

            float exitX = isRight ? swipeExitX : -swipeExitX;
            CardDataSO dataToApply = _display.Data;

            _currentTween = _rectTransform
                .DOAnchorPosX(exitX, swipeDuration)
                .SetEase(Ease.InBack)
                .SetTarget(_rectTransform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(() => OnSwipeComplete(dataToApply, isRight));

            onCardSwipedEvent?.Raise();
        }

        private void OnSwipeComplete(CardDataSO dataToApply, bool isRight)
        {
            dataToApply?.ApplyEffect(isRight);
            OnCardRemovedWithDecision?.Invoke(dataToApply, isRight);
            OnCardRemoved?.Invoke();

            // ← AÑADE ESTE LOG
            Debug.Log($"[CardController] Returning card to pool: {_display.Data?.cardName}");
            
            if (cardFactory != null)
            {
                cardFactory.ReturnToPool(_display);
                Debug.Log($"[CardController] Card returned. Pool stats - InUse: {cardFactory.GetInUseCount()}");
            }
            else
            {
                Debug.LogError("[CardController] cardFactory is NULL! Card NOT returned to pool!");
            }

            _currentTween = null;
            FinishBusy();
        }

        #endregion

        #region Helper Methods

        private void FinishBusy()
        {
            _isBusy = false;
            SetRaycast(true);
        }

        private void SetRaycast(bool enabled)
        {
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = enabled;
        }

        #endregion

        #region Cleanup

        private void CleanupTweens()
        {
            if (_activeSequence != null && _activeSequence.IsActive())
            {
                _activeSequence.Kill(complete: false);
                _activeSequence = null;
            }

            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill(complete: false);
                _currentTween = null;
            }

            // NUEVO: Limpiar delayed face switch
            if (_delayedFaceSwitch != null && _delayedFaceSwitch.IsActive())
            {
                _delayedFaceSwitch.Kill(complete: false);
                _delayedFaceSwitch = null;
            }

            if (_rectTransform != null)
                _rectTransform.DOKill(complete: false);
        }

        #endregion
    }
}