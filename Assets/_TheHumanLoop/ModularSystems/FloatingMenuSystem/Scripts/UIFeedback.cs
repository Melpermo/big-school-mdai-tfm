using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheHumanLoop.UI
{
    public class UIFeedback : MonoBehaviour
    {
        [Header("Punch Settings")]
        [SerializeField] private float punchAmount = 0.1f;
        [SerializeField] private float duration = 0.2f;

        [Header("Hover Settings")]
        [SerializeField] private float hoverScale = 1.05f;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        // When the mouse/finger starts the click
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            // Slightly shrink to simulate "pressing"
            transform.DOScale(originalScale * (1f - punchAmount), duration * 0.5f).SetEase(Ease.OutQuad);
        }

        // When the mouse/finger is released
        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOKill();
            // Snap back with a small bounce
            transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        }

        // Subtle hover effect for PC/Mouse users
        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutCubic);
        }

        // Butotn click effect for PC/Mouse users
        public void OnButtonClick()
        {
            // Efecto de "latido" rápido
            transform.DOScale(0.95f, 0.1f).OnComplete(() => {
                transform.DOScale(1f, 0.1f);
            });
        }
    }
}
