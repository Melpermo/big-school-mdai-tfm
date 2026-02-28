using HumanLoop.AudioSystem;
using System.Collections;
using UnityEngine;

namespace TheHumanLoop.UI
{
    /// <summary>
    /// Animated window component with open/close transitions.
    /// Optimized for WebGL without DOTween dependency.
    /// </summary>
    public class UIWindow : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Duration for open/close animations")]
        [SerializeField] private float duration = 0.5f;

        [Tooltip("Strength of punch/bounce effect")]
        [SerializeField] private float punchStrength = 0.1f;

        [Tooltip("Rotation amplitude during animations")]
        [SerializeField] private float rotationAmplitude = 5f;

        [Header("Sound Clips")]
        [SerializeField] private SoundEventSO openWindowSound;
        [SerializeField] private SoundEventSO closeWindowSound;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Cached components
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;

        // Animation state
        private Coroutine _currentAnimation;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            InitializeState();
        }

        #endregion

        #region Initialization

        private void CacheComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void InitializeState()
        {
            // Initial state: invisible and scaled down
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Opens the window with animated transition.
        /// </summary>
        public void Open()
        {
            if (showDebugLogs)
            {
                Debug.Log($"[UIWindow] Opening: {gameObject.name}");
            }

            gameObject.SetActive(true);

            // Stop any running animation
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(OpenAnimationCoroutine());

            // Play open sound
            if (openWindowSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(openWindowSound);
            }
        }

        /// <summary>
        /// Closes the window with animated transition.
        /// </summary>
        public void Close()
        {
            if (showDebugLogs)
            {
                Debug.Log($"[UIWindow] Closing: {gameObject.name}");
            }

            // Stop any running animation
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(CloseAnimationCoroutine());

            // Play close sound
            if (closeWindowSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(closeWindowSound);
            }
        }

        #endregion

        #region Animation Coroutines

        private IEnumerator OpenAnimationCoroutine()
        {
            // Reset state
            _rectTransform.localScale = Vector3.zero;
            _rectTransform.localEulerAngles = Vector3.zero;
            _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;

            // Phase 1: Scale up with OutBack easing + Fade in (parallel)
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                // OutBack easing for scale
                float scaleT = EaseOutBack(t);
                _rectTransform.localScale = Vector3.one * scaleT;

                // Faster fade in (completes at 50% of duration)
                float fadeT = Mathf.Clamp01(t * 2f);
                _canvasGroup.alpha = fadeT;

                yield return null;
            }

            // Ensure final values
            _rectTransform.localScale = originalScale;
            _canvasGroup.alpha = 1f;

            // Phase 2: Punch effects (bounce + rotation)
            float punchDuration = duration * 0.5f;
            elapsed = 0f;

            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / punchDuration;

                // Punch scale (bounce effect)
                float punchScale = CalculatePunch(t, punchStrength, 5);
                _rectTransform.localScale = originalScale + Vector3.one * punchScale;

                // Punch rotation (wobble effect)
                float punchRotation = CalculatePunch(t, rotationAmplitude, 2) * 0.5f;
                _rectTransform.localEulerAngles = new Vector3(0f, 0f, punchRotation);

                yield return null;
            }

            // Reset to final state
            _rectTransform.localScale = originalScale;
            _rectTransform.localEulerAngles = Vector3.zero;

            // Enable interaction
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _currentAnimation = null;

            if (showDebugLogs)
            {
                Debug.Log($"[UIWindow] Open animation complete");
            }
        }

        private IEnumerator CloseAnimationCoroutine()
        {
            // Disable interaction immediately
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            Vector3 originalScale = Vector3.one;
            float punchDuration = duration * 0.5f;
            float elapsed = 0f;

            // Phase 1: Quick punch anticipation
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / punchDuration;

                // Small punch scale
                float punchScale = CalculatePunch(t, punchStrength * 0.5f, 2);
                _rectTransform.localScale = originalScale + Vector3.one * punchScale;

                // Opposite rotation
                float punchRotation = CalculatePunch(t, -rotationAmplitude, 2) * 0.5f;
                _rectTransform.localEulerAngles = new Vector3(0f, 0f, punchRotation);

                yield return null;
            }

            // Phase 2: Scale down with InBack + Fade out (parallel)
            elapsed = 0f;
            float fadeDelay = duration * 0.2f;
            float fadeDuration = duration * 0.6f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                // InBack easing for scale
                float scaleT = 1f - EaseInBack(t);
                _rectTransform.localScale = Vector3.one * scaleT;

                // Delayed fade out
                if (elapsed > fadeDelay)
                {
                    float fadeElapsed = elapsed - fadeDelay;
                    float fadeT = fadeElapsed / fadeDuration;
                    _canvasGroup.alpha = 1f - fadeT;
                }

                // Rotate back to 0
                float rotationT = Mathf.Lerp(_rectTransform.localEulerAngles.z, 0f, t);
                _rectTransform.localEulerAngles = new Vector3(0f, 0f, rotationT);

                yield return null;
            }

            // Final state
            _rectTransform.localScale = Vector3.zero;
            _rectTransform.localEulerAngles = Vector3.zero;
            _canvasGroup.alpha = 0f;

            gameObject.SetActive(false);

            _currentAnimation = null;

            if (showDebugLogs)
            {
                Debug.Log($"[UIWindow] Close animation complete");
            }
        }

        #endregion

        #region Easing Functions

        /// <summary>
        /// Ease Out Back: Overshoots target then settles.
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        /// <summary>
        /// Ease In Back: Pulls back before moving forward.
        /// </summary>
        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return c3 * t * t * t - c1 * t * t;
        }

        /// <summary>
        /// Calculates punch effect (oscillation with decay).
        /// </summary>
        private float CalculatePunch(float t, float strength, int vibrato)
        {
            float decay = 1f - t;
            float oscillation = Mathf.Sin(t * Mathf.PI * vibrato);
            return oscillation * strength * decay;
        }

        #endregion

        #region Cleanup

        private void OnDisable()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test/Open Window")]
        private void TestOpen()
        {
            Open();
        }

        [ContextMenu("Test/Close Window")]
        private void TestClose()
        {
            Close();
        }

        [ContextMenu("Test/Toggle Window")]
        private void TestToggle()
        {
            if (gameObject.activeSelf && _canvasGroup.alpha > 0.5f)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
#endif

        #endregion
    }
}