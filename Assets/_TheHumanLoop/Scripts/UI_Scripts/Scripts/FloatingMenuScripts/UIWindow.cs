using UnityEngine;
using DG.Tweening;

namespace TheHumanLoop.UI
{
    public class UIWindow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float punchStrength = 0.1f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            // Initial state: invisible and scaled down
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0;
        }

        public void Open()
        {
            gameObject.SetActive(true);

            transform.DOKill();
            canvasGroup.DOKill();

            // Reset state
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0;

            // 1. Principal Scale Animation
            transform.DOScale(1f, duration).SetEase(Ease.OutBack);

            // 2. Fade In
            canvasGroup.DOFade(1f, duration * 0.5f);

            // 3. Using punchStrength for a "Bounce" effect on arrival
            // This creates a subtle wobble that makes the window feel organic
            transform.DOPunchScale(Vector3.one * punchStrength, duration, 5, 1)
                     .SetDelay(duration * 0.5f); // Starts mid-animation for a smooth transition

            // 4. Personality rotation
            transform.DOPunchRotation(new Vector3(0, 0, 5f), duration, 2, 0.5f);           
        }

        public void Close()
        {
            transform.DOKill();
            canvasGroup.DOKill();

            // Symmetrical Closing Logic:
            // 1. A quick "Punch" as anticipation before shrinking
            transform.DOPunchScale(Vector3.one * (punchStrength * 0.5f), duration * 0.5f, 2, 0.5f);

            // 2. Rotate slightly in the opposite direction
            transform.DOPunchRotation(new Vector3(0, 0, -5f), duration * 0.5f, 2, 0.5f);

            // 3. Fade and Scale down with InBack (the inverse of OutBack)
            canvasGroup.DOFade(0f, duration * 0.6f).SetDelay(duration * 0.2f);

            transform.DOScale(0f, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    gameObject.SetActive(false);
                });           
        }
    }
}
