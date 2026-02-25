using DG.Tweening;
using HumanLoop.Core;
using HumanLoop.Data;
using HumanLoop.Events;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HumanLoop.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CardDisplay))]
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static event Action OnCardRemoved;
        public static event Action<CardDataSO, bool> OnCardRemovedWithDecision;

        [Header("Settings")]
        [SerializeField] private float fovAngle = 15f;
        [SerializeField] private float swipeThreshold = 150f;
        [SerializeField] private float returnDuration = 0.3f;
        [SerializeField] private float swipeExitX = 1000f;
        [SerializeField] private float swipeDuration = 0.4f;

        [Header("Dependencies")]
        [SerializeField] private CardFactory cardFactory;

        [Header("Mechanics GameEvents")]
        [SerializeField] private GameEventSO onCardSwipedEvent;
        [SerializeField] private GameEventSO onCardReturnedEvent;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private CardDisplay _display;

        private Vector2 _startPosition;

        private Tween _moveTween;
        private Tween _rotateTween;

        private bool _isBusy;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _display = GetComponent<CardDisplay>();

            // Start position should be "center" for your gameplay (usually 0,0 on anchoredPosition).
            // We still cache it, but we also refresh it on ResetCardController().
            _startPosition = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            KillTweens();
            _isBusy = false;
            SetRaycast(true);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public void SetFactory(CardFactory factory)
        {
            cardFactory = factory;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isBusy) return;
            KillTweens();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isBusy) return;

            _rectTransform.anchoredPosition += eventData.delta;

            float xOffset = _rectTransform.anchoredPosition.x - _startPosition.x;

            float normalizedOffset = Mathf.Clamp(xOffset / swipeThreshold, -1f, 1f);

            float rotationZ = -(normalizedOffset) * fovAngle;
            _rectTransform.localEulerAngles = new Vector3(0f, 0f, rotationZ);

            _display.UpdateChoiceVisuals(normalizedOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isBusy) return;

            float xOffset = _rectTransform.anchoredPosition.x - _startPosition.x;

            if (Mathf.Abs(xOffset) > swipeThreshold)
                SwipeCard(xOffset > 0);
            else
                ReturnToCenter();
        }

        private void ReturnToCenter()
        {
            _isBusy = true;
            SetRaycast(false);
            KillTweens();

            int completed = 0;

            _moveTween = _rectTransform
                .DOAnchorPos(_startPosition, returnDuration)
                .SetEase(Ease.OutBack)
                .SetTarget(this)
                .OnComplete(() =>
                {
                    completed++;
                    if (completed >= 2) FinishBusy();
                });

            _rotateTween = _rectTransform
                .DORotate(Vector3.zero, returnDuration)
                .SetEase(Ease.OutBack)
                .SetTarget(this)
                .OnComplete(() =>
                {
                    completed++;
                    if (completed >= 2) FinishBusy();
                });

            _display.HideChoices();
            onCardReturnedEvent?.Raise();
        }

        private void SwipeCard(bool isRight)
        {
            _isBusy = true;
            SetRaycast(false);
            KillTweens();

            float exitX = isRight ? swipeExitX : -swipeExitX;
            CardDataSO dataToApply = _display.Data;

            _moveTween = _rectTransform
                .DOAnchorPosX(exitX, swipeDuration)
                .SetEase(Ease.InBack)
                .SetTarget(this)
                .OnComplete(() =>
                {
                    dataToApply?.ApplyEffect(isRight);

                    OnCardRemovedWithDecision?.Invoke(dataToApply, isRight);
                    OnCardRemoved?.Invoke();

                    if (cardFactory != null)
                        cardFactory.ReturnToPool(_display);

                    FinishBusy();
                });

            onCardSwipedEvent?.Raise();
        }

        public void ResetCardController()
        {
            KillTweens();

            // In your gameplay the "center" is typically anchoredPosition = 0.
            // But if you want a different start offset, set it from outside before calling reset.
            _startPosition = Vector2.zero;

            _rectTransform.anchoredPosition = _startPosition;
            _rectTransform.localRotation = Quaternion.identity;

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false; // CardDisplay enables after flip completes

            _display.HideChoices();

            _isBusy = false;
            SetRaycast(true);
        }

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

        private void KillTweens()
        {
            DOTween.Kill(this, true);

            if (_rectTransform != null)
                _rectTransform.DOKill(true);

            _moveTween = null;
            _rotateTween = null;
        }
    }
}