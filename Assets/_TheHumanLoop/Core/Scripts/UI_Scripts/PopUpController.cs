using System.Collections;
using UnityEngine;


namespace HumanLoop.UI
{
    [RequireComponent(typeof(CanvasGroup))] // Ensures the GameObject has a CanvasGroup for fading
    public class PopUpController : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        public float duration = 0.3f;
        public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve closeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private Coroutine currentAnimation;

        void Awake()
        {
            // Get the CanvasGroup component on Awake
            canvasGroup = GetComponent<CanvasGroup>();

            // Initial state: Invisible and scaled to zero
            if (panel != null)
            {
                canvasGroup.alpha = 0;
                panel.transform.localScale = Vector3.zero;
                panel.SetActive(false);
            }
        }

        // Public method to trigger the Open animation
        public void Open()
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);

            panel.SetActive(true);
            currentAnimation = StartCoroutine(AnimatePopUp(1f, Vector3.one, openCurve));
        }

        // Public method to trigger the Close animation
        public void Close()
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimatePopUp(0f, Vector3.zero, closeCurve, true));
        }

        private IEnumerator AnimatePopUp(float targetAlpha, Vector3 targetScale, AnimationCurve curve, bool deactivateAtEnd = false)
        {
            float time = 0;
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = panel.transform.localScale;

            while (time < duration)
            {
                time += Time.deltaTime;
                float lerpFactor = time / duration;
                float curveValue = curve.Evaluate(lerpFactor);

                // Handle Alpha Fade
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerpFactor);

                // Handle Scale with Curve (Supports Overshooting/Bounce)
                // We use LerpUnclamped to allow the curve to go above 1.0 or below 0.0
                panel.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

                yield return null;
            }

            // Ensure final values are exactly set
            canvasGroup.alpha = targetAlpha;
            panel.transform.localScale = targetScale;

            if (deactivateAtEnd)
                panel.SetActive(false);

            currentAnimation = null;
        }
    }
}
