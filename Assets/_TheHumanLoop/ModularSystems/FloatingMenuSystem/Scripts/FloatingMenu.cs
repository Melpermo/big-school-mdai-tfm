using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TheHumanLoop.UI
{
    /// <summary>
    /// Floating menu system with staggered animations.
    /// Pure Unity implementation without DOTween dependency.
    /// </summary>
    public class FloatingMenu : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RectTransform container;
        [SerializeField] private List<RectTransform> menuButtons;
        
        [Header("Animation Settings")]
        [Tooltip("Delay between each button animation")]
        [SerializeField] private float staggerDelay = 0.05f;
        
        [Tooltip("Duration of each button animation")]
        [SerializeField] private float animationDuration = 0.4f;
        
        [Tooltip("Overshoot amount for bounce effect (OutBack easing)")]
        [SerializeField] private float overshoot = 1.70158f;

        [Header("Main Button")]
        [SerializeField] private RectTransform mainButton;
        
        [Tooltip("Punch scale intensity for main button feedback")]
        [SerializeField] private float punchIntensity = 0.2f;
        
        [Tooltip("Duration of punch animation")]
        [SerializeField] private float punchDuration = 0.2f;

        private bool _isOpen = false;
        private Coroutine _currentAnimation;
        private Coroutine _mainButtonAnimation;

        private void Start()
        {
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            container.gameObject.SetActive(false);
            
            foreach (var btn in menuButtons)
            {
                btn.localScale = Vector3.zero;
            }
        }

        /// <summary>
        /// Toggles the menu open/closed state.
        /// </summary>
        public void ToggleMenu()
        {
            // Stop any running animations to prevent conflicts
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _isOpen = !_isOpen;

            if (_isOpen)
            {
                _currentAnimation = StartCoroutine(OpenMenuCoroutine());
            }
            else
            {
                _currentAnimation = StartCoroutine(CloseMenuCoroutine());
            }

            // Main button feedback
            AnimateMainButton();
        }

        private void AnimateMainButton()
        {
            if (_mainButtonAnimation != null)
            {
                StopCoroutine(_mainButtonAnimation);
            }

            _mainButtonAnimation = StartCoroutine(MainButtonFeedbackCoroutine());
        }

        #region Open/Close Animations

        private IEnumerator OpenMenuCoroutine()
        {
            container.gameObject.SetActive(true);

            // Animate each button with stagger
            for (int i = 0; i < menuButtons.Count; i++)
            {
                StartCoroutine(ScaleButtonCoroutine(menuButtons[i], 0f, 1f, animationDuration, EaseType.OutBack));
                yield return new WaitForSeconds(staggerDelay);
            }
        }

        private IEnumerator CloseMenuCoroutine()
        {
            // Animate in reverse order
            for (int i = menuButtons.Count - 1; i >= 0; i--)
            {
                StartCoroutine(ScaleButtonCoroutine(menuButtons[i], 1f, 0f, animationDuration * 0.7f, EaseType.InBack));
                yield return new WaitForSeconds(staggerDelay);
            }

            // Wait for last animation to finish before hiding container
            yield return new WaitForSeconds(animationDuration * 0.7f);
            container.gameObject.SetActive(false);
        }

        #endregion

        #region Button Scale Animation

        private IEnumerator ScaleButtonCoroutine(RectTransform button, float startScale, float endScale, float duration, EaseType easeType)
        {
            float elapsed = 0f;
            Vector3 start = Vector3.one * startScale;
            Vector3 end = Vector3.one * endScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Apply easing
                float easedT = ApplyEasing(t, easeType);

                button.localScale = Vector3.LerpUnclamped(start, end, easedT);
                yield return null;
            }

            button.localScale = end;
        }

        #endregion

        #region Main Button Feedback

        private IEnumerator MainButtonFeedbackCoroutine()
        {
            // Punch scale animation
            StartCoroutine(PunchScaleCoroutine(mainButton, punchIntensity, punchDuration));

            // Rotation animation
            float startRotation = mainButton.localEulerAngles.z;
            float targetRotation = _isOpen ? 45f : 0f;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float easedT = EaseOutCubic(t);

                float currentRotation = Mathf.LerpAngle(startRotation, targetRotation, easedT);
                mainButton.localEulerAngles = new Vector3(0, 0, currentRotation);

                yield return null;
            }

            mainButton.localEulerAngles = new Vector3(0, 0, targetRotation);
        }

        private IEnumerator PunchScaleCoroutine(RectTransform target, float intensity, float duration)
        {
            Vector3 originalScale = Vector3.one;
            float elapsed = 0f;
            int vibrato = 10;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Decay factor
                float decay = 1f - t;

                // Oscillation
                float oscillation = Mathf.Sin(t * Mathf.PI * vibrato) * intensity * decay;

                target.localScale = originalScale + Vector3.one * oscillation;

                yield return null;
            }

            target.localScale = originalScale;
        }

        #endregion

        #region Easing Functions

        private enum EaseType
        {
            Linear,
            OutBack,
            InBack,
            OutCubic
        }

        private float ApplyEasing(float t, EaseType easeType)
        {
            return easeType switch
            {
                EaseType.OutBack => EaseOutBack(t),
                EaseType.InBack => EaseInBack(t),
                EaseType.OutCubic => EaseOutCubic(t),
                _ => t // Linear
            };
        }

        // Ease Out Back: Overshoots target then settles
        private float EaseOutBack(float t)
        {
            float c1 = overshoot;
            float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // Ease In Back: Pulls back before moving forward
        private float EaseInBack(float t)
        {
            float c1 = overshoot;
            float c3 = c1 + 1f;

            return c3 * t * t * t - c1 * t * t;
        }

        // Ease Out Cubic: Smooth deceleration
        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Programmatically opens the menu.
        /// </summary>
        public void OpenMenu()
        {
            if (!_isOpen)
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Programmatically closes the menu.
        /// </summary>
        public void CloseMenu()
        {
            if (_isOpen)
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Returns current menu state.
        /// </summary>
        public bool IsOpen => _isOpen;

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Stop all coroutines to prevent errors
            StopAllCoroutines();
        }

        #endregion
    }
}
