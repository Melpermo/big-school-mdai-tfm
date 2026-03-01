using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheHumanLoop.LoadingScreenSystem
{
    /// <summary>
    /// Manages asynchronous scene loading with animated loading screen.
    /// Provides smooth transitions, progress tracking, and visual feedback during scene loads.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject backgroundScreen;
        [SerializeField] private GameObject[] gameObjectsToDeact;
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private CanvasGroup loadingCanvasGroup;

        [Header("Animation Elements")]
        [SerializeField] private RectTransform loadingIcon;
        [SerializeField] private Image[] pulsingImages;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Loading Settings")]
        [Tooltip("Minimum fake loading time to ensure smooth user experience")]
        [SerializeField] private float fakeLoadTime = 2f;
        
        [Tooltip("Minimum time loading screen stays visible")]
        [SerializeField] private float minimumLoadTime = 1f;
        
        [Tooltip("Duration of fade-in effect when showing loading screen")]
        [SerializeField] private float fadeInDuration = 0.5f;
        
        [Tooltip("Duration of fade-out effect when hiding loading screen")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Animation Settings")]
        [Tooltip("Speed of loading icon rotation (degrees per second)")]
        [SerializeField] private float iconRotationSpeed = 180f;
        
        [Tooltip("Speed of pulsing animation for images")]
        [SerializeField] private float pulseSpeed = 1f;
        
        [Tooltip("Speed of animated dots after loading text")]
        [SerializeField] private float dotsAnimationSpeed = 0.5f;

        [Header("Loading Tips Text")]
        [Tooltip("List of loading tips to randomly display")]
        [SerializeField] private List<TextMeshProUGUI> tipTextTMP_List;

        // Coroutine references for proper cleanup
        private Coroutine iconAnimationCoroutine;
        private Coroutine pulseAnimationCoroutine;
        private Coroutine dotsAnimationCoroutine;

        /// <summary>
        /// Loads a scene by its build index with animated loading screen.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene in Build Settings</param>
        public void LoadScenes(int sceneIndex)
        {       
            StartCoroutine(LoadAsynchronously(sceneIndex));
        }

        /// <summary>
        /// Loads a scene by its name with animated loading screen.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadSceneByName(string sceneName)
        {
            StartCoroutine(LoadAsynchronouslyByName(sceneName));
        }

        /// <summary>
        /// Loads a scene by its name without animations or loading screen.
        /// Cleans up unused assets and forces garbage collection before and after loading.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadAsynchronouslyByName(sceneName));
        }       

        /// <summary>
        /// Asynchronously loads a scene with progress tracking and animations.
        /// Blends fake and real loading progress for smooth user experience.
        /// </summary>
        private IEnumerator LoadAsynchronously(int sceneIndex)
        {
            // Fade in loading screen smoothly
            yield return StartCoroutine(FadeLoadingScreen(true));

            // Prepare loading screen UI
            backgroundScreen.SetActive(false);
            DeactivateArrayOf();
            loadingScreen.SetActive(true);
            loadingSlider.value = 0f;
            ShowRandomTipsTMP();

            // Start all visual animations
            StartLoadingAnimations();

            // Wait one frame to ensure UI is ready
            yield return null;

            // Begin async scene loading
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.allowSceneActivation = false; // Prevent automatic scene activation

            float timer = 0f;
            float targetProgress = 0f;

            // Loading loop: blend fake and real progress
            while (!operation.isDone)
            {
                // Unity's AsyncOperation.progress goes from 0 to 0.9 (90%)
                // The last 10% is reserved for scene activation
                float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

                // Calculate fake progress based on time
                timer += Time.deltaTime;
                float fakeProgress = Mathf.Clamp01(timer / fakeLoadTime);
                
                // Use the higher value between real and fake progress
                // This ensures smooth progress even if real loading is fast
                targetProgress = Mathf.Max(realProgress, fakeProgress);

                // Smooth progress bar update with lerp
                loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetProgress, Time.deltaTime * 3f);

                // Update percentage text display
                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(loadingSlider.value * 100)}%";
                }

                // Check if loading is complete and minimum time has passed
                if (operation.progress >= 0.9f && timer >= minimumLoadTime)
                {
                    // Set to 100% before activation
                    loadingSlider.value = 1f;
                    if (progressText != null)
                    {
                        progressText.text = "100%";
                    }

                    // Stop all animations before scene transition
                    StopLoadingAnimations();

                    // Fade out loading screen smoothly
                    yield return StartCoroutine(FadeLoadingScreen(false));

                    // Allow scene to activate
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Deactivates all GameObjects in the configured array.
        /// Used to hide menu elements during scene transition.
        /// </summary>
        private void DeactivateArrayOf()
        {
            foreach (GameObject go in gameObjectsToDeact)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Asynchronously loads a scene by name with progress tracking and animations.
        /// Same functionality as LoadAsynchronously but uses scene name instead of index.
        /// </summary>
        private IEnumerator LoadAsynchronouslyByName(string sceneName)
        {
            // Fade in loading screen smoothly
            yield return StartCoroutine(FadeLoadingScreen(true));

            // Prepare loading screen UI
            backgroundScreen.SetActive(false);
            DeactivateArrayOf();
            loadingScreen.SetActive(true);
            loadingSlider.value = 0f;
            ShowRandomTipsTMP();

            // Start all visual animations
            StartLoadingAnimations();

            // Wait one frame to ensure UI is ready
            yield return null;

            // Begin async scene loading by name
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            float timer = 0f;
            float targetProgress = 0f;

            // Loading loop with progress blending
            while (!operation.isDone)
            {
                // Calculate real loading progress (0 to 0.9 normalized to 0 to 1)
                float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

                // Calculate time-based fake progress
                timer += Time.deltaTime;
                float fakeProgress = Mathf.Clamp01(timer / fakeLoadTime);
                targetProgress = Mathf.Max(realProgress, fakeProgress);

                // Smooth progress update
                loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetProgress, Time.deltaTime * 3f);

                // Update percentage display
                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(loadingSlider.value * 100)}%";
                }

                // Check completion conditions
                if (operation.progress >= 0.9f && timer >= minimumLoadTime)
                {
                    loadingSlider.value = 1f;
                    if (progressText != null)
                    {
                        progressText.text = "100%";
                    }

                    // Clean up animations
                    StopLoadingAnimations();

                    // Fade out before activation
                    yield return StartCoroutine(FadeLoadingScreen(false));

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Smoothly fades the loading screen in or out using CanvasGroup alpha.
        /// </summary>
        /// <param name="fadeIn">True to fade in (0 to 1), false to fade out (1 to 0)</param>
        private IEnumerator FadeLoadingScreen(bool fadeIn)
        {
            if (loadingCanvasGroup == null) yield break;

            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            float elapsed = 0f;

            // Smooth lerp over duration
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            // Ensure final alpha is set precisely
            loadingCanvasGroup.alpha = endAlpha;
        }

        #region Animations

        /// <summary>
        /// Starts all loading screen animations (icon rotation, image pulsing, dots animation).
        /// </summary>
        private void StartLoadingAnimations()
        {
            // Start icon rotation if available
            if (loadingIcon != null)
            {
                iconAnimationCoroutine = StartCoroutine(RotateLoadingIcon());
            }

            // Start image pulsing if available
            if (pulsingImages != null && pulsingImages.Length > 0)
            {
                pulseAnimationCoroutine = StartCoroutine(PulseImages());
            }

            // Start dots animation if loading text exists
            if (loadingText != null)
            {
                dotsAnimationCoroutine = StartCoroutine(AnimateLoadingDots());
            }
        }

        /// <summary>
        /// Stops all running loading animations safely.
        /// </summary>
        private void StopLoadingAnimations()
        {
            if (iconAnimationCoroutine != null)
            {
                StopCoroutine(iconAnimationCoroutine);
            }

            if (pulseAnimationCoroutine != null)
            {
                StopCoroutine(pulseAnimationCoroutine);
            }

            if (dotsAnimationCoroutine != null)
            {
                StopCoroutine(dotsAnimationCoroutine);
            }
        }

        /// <summary>
        /// Continuously rotates the loading icon at configured speed.
        /// </summary>
        private IEnumerator RotateLoadingIcon()
        {
            while (true)
            {
                // Rotate counter-clockwise (negative Z rotation)
                loadingIcon.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Creates a pulsing alpha animation for configured images.
        /// Alpha oscillates smoothly between 0.3 and 1.0.
        /// </summary>
        private IEnumerator PulseImages()
        {
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                
                // Sine wave produces smooth oscillation between 0 and 1
                float alpha = (Mathf.Sin(time) + 1f) / 2f;

                // Apply alpha to all pulsing images
                foreach (Image img in pulsingImages)
                {
                    if (img != null)
                    {
                        Color color = img.color;
                        // Lerp between 0.3 (dim) and 1.0 (bright)
                        color.a = Mathf.Lerp(0.3f, 1f, alpha);
                        img.color = color;
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Creates a pulsing scale animation for the target RectTransform.
        /// Currently unused but kept for potential future use.
        /// </summary>
        private IEnumerator ScaleAnimation(RectTransform target)
        {
            Vector3 originalScale = target.localScale;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * 2f;
                // Oscillates between 0.9 and 1.1 (±10% of original scale)
                float scale = 1f + Mathf.Sin(time) * 0.1f;
                target.localScale = originalScale * scale;
                yield return null;
            }
        }

        /// <summary>
        /// Animates loading text with cycling dots (...).
        /// Cycles through 0, 1, 2, 3 dots continuously.
        /// </summary>
        private IEnumerator AnimateLoadingDots()
        {
            string baseText = " ";
            int dotCount = 0;

            while (true)
            {
                // Cycle through 0-3 dots
                dotCount = (dotCount + 1) % 4;
                loadingText.text = baseText + new string('.', dotCount);
                yield return new WaitForSeconds(dotsAnimationSpeed);
            }
        }

        /// <summary>
        /// Randomly selects and displays one loading tip from the configured list.
        /// All other tips are hidden (alpha = 0).
        /// </summary>
        private void ShowRandomTipsTMP()
        {
            if (tipTextTMP_List != null && tipTextTMP_List.Count > 0)
            {
                // Pick random tip index
                int tipToShowIndex = Random.Range(0, tipTextTMP_List.Count);

                // Show selected tip, hide all others
                for (int i = 0; i < tipTextTMP_List.Count; i++)
                {
                    if (i == tipToShowIndex)
                    {
                        tipTextTMP_List[i].alpha = 1; // Visible
                    }
                    else
                    {
                        tipTextTMP_List[i].alpha = 0; // Hidden
                    }
                }
            }
        }

        #endregion
    }
}