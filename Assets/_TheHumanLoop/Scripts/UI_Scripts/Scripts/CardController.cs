using DG.Tweening;
using HumanLoop.Core;
using HumanLoop.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HumanLoop.UI
{
    /// <summary>
    /// Handles the physical drag, rotation, and swipe logic using DOTween.
    /// </summary>
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static event Action OnCardRemoved;
        public static event Action<CardDataSO, bool> OnCardRemovedWithDecision;

        [Header("Settings")]
        [SerializeField] private float fovAngle = 15f; // Rotation angle at max swipe
        [SerializeField] private float swipeThreshold = 150f; // Distance to trigger action
        [SerializeField] private float returnDuration = 0.3f; // DOTween duration        

        private Vector2 _startPosition;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private CardDisplay _display;



        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _display = GetComponent<CardDisplay>();
            _startPosition = _rectTransform.anchoredPosition;
        }

        void Start()
        {
            // Ensure starting position is recorded
            _startPosition = _rectTransform.anchoredPosition;
            _display.HideChoices();
            ResetCardController();
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            // Optional: Reset scale or trigger feedback when starting drag
            DOTween.Kill(_rectTransform);
        }

        // Inside OnDrag(PointerEventData eventData)
        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta;

            float xOffset = _rectTransform.anchoredPosition.x - _startPosition.x;

            // Calculate rotation
            float rotationZ = -(xOffset / swipeThreshold) * fovAngle;
            _rectTransform.localEulerAngles = new Vector3(0, 0, rotationZ);

            // Normalize offset: 0 is center, 1 (or -1) is threshold reached
            float normalizedOffset = Mathf.Clamp(xOffset / swipeThreshold, -1f, 1f);

            float rotationAmount = _rectTransform.anchoredPosition.x * -0.05f; // Ajusta el 0.05f a tu gusto
            _rectTransform.localRotation = Quaternion.Euler(0, 0, rotationAmount);

            // Delegate visual update to Display
            _display.UpdateChoiceVisuals(normalizedOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
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

        // Don't forget to reset alphas in ReturnToCenter()
        private void ReturnToCenter()
        {
            _rectTransform.DOAnchorPos(_startPosition, returnDuration).SetEase(Ease.OutBack);
            _rectTransform.DORotate(Vector3.zero, returnDuration).SetEase(Ease.OutBack);

            // Let Display handle clearing the UI
            _display.HideChoices();
        }

        
        private void SwipeCard(bool isRight)
        {
            float exitDirection = isRight ? 1000f : -1000f;
            CardDataSO dataToApply = _display.Data;

            _rectTransform.DOAnchorPosX(exitDirection, 0.4f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    dataToApply.ApplyEffect(isRight);
                    OnCardRemovedWithDecision?.Invoke(dataToApply, isRight);
                    OnCardRemoved?.Invoke();

                    // INSTEAD OF DESTROY: Return to pool via Factory
                    // Note: You might need a reference to the factory or use a Singleton
                    UnityEngine.Object.FindFirstObjectByType<CardFactory>().ReturnToPool(_display);
                });
        }

        // Este método debe ser llamado cuando la carta sale del pool
        public void ResetCardController()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = false; // Empezamos sin raycast hasta que el Flip termine
        }
    }
}