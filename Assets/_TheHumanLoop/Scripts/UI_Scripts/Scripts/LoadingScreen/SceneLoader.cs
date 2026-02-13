using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace TheHumanLoop.LoadingScreenSystem
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject backgroundScreen;
        [SerializeField] private GameObject MainMenuGO;
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI loadingTipText;
        [SerializeField] private CanvasGroup loadingCanvasGroup;

        [Header("Animation Elements")]
        [SerializeField] private RectTransform loadingIcon;
        [SerializeField] private Image[] pulsingImages;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Loading Settings")]
        [SerializeField] private float fakeLoadTime = 2f;
        [SerializeField] private float minimumLoadTime = 1f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Animation Settings")]
        [SerializeField] private float iconRotationSpeed = 180f;
        [SerializeField] private float pulseSpeed = 1f;
        //[SerializeField] private float scalePulseSpeed = 1f;
        [SerializeField] private float dotsAnimationSpeed = 0.5f;

        [Header("Loading Tips")]
        [SerializeField]
        private string[] loadingTips = new string[]
        {
            "Tip: Stay hydrated!",
            "Tip: Take breaks regularly",
            "Tip: Explore every corner",
            "Loading assets...",
            "Preparing your experience..."
        };

        private Coroutine iconAnimationCoroutine;
        private Coroutine pulseAnimationCoroutine;
        private Coroutine dotsAnimationCoroutine;

        public void LoadScenes(int sceneIndex)
        {
            StartCoroutine(LoadAsynchronously(sceneIndex));
        }

        public void LoadSceneByName(string sceneName)
        {
            StartCoroutine(LoadAsynchronouslyByName(sceneName));
        }

        private IEnumerator LoadAsynchronously(int sceneIndex)
        {
            // Fade in loading screen
            yield return StartCoroutine(FadeLoadingScreen(true));

            // Setup UI
            backgroundScreen.SetActive(false);
            MainMenuGO.SetActive(false);
            loadingScreen.SetActive(true);
            loadingSlider.value = 0f;

            if (loadingTipText != null && loadingTips.Length > 0)
            {
                loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            // Start animations
            StartLoadingAnimations();

            yield return null;

            // Start async loading
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.allowSceneActivation = false;

            float timer = 0f;
            float targetProgress = 0f;

            while (!operation.isDone)
            {
                // Calculate real progress (0.0 to 0.9)
                float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

                // Blend fake and real progress
                timer += Time.deltaTime;
                float fakeProgress = Mathf.Clamp01(timer / fakeLoadTime);
                targetProgress = Mathf.Max(realProgress, fakeProgress);

                // Smooth progress update
                loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetProgress, Time.deltaTime * 3f);

                // Update percentage text
                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(loadingSlider.value * 100)}%";
                }

                // Check if loading is complete
                if (operation.progress >= 0.9f && timer >= minimumLoadTime)
                {
                    loadingSlider.value = 1f;
                    if (progressText != null)
                    {
                        progressText.text = "100%";
                    }

                    // Stop animations
                    StopLoadingAnimations();

                    // Fade out loading screen
                    yield return StartCoroutine(FadeLoadingScreen(false));

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        private IEnumerator LoadAsynchronouslyByName(string sceneName)
        {
            // Fade in loading screen
            yield return StartCoroutine(FadeLoadingScreen(true));

            backgroundScreen.SetActive(false);
            MainMenuGO.SetActive(false);
            loadingScreen.SetActive(true);
            loadingSlider.value = 0f;

            if (loadingTipText != null && loadingTips.Length > 0)
            {
                loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            // Start animations
            StartLoadingAnimations();

            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            float timer = 0f;
            float targetProgress = 0f;

            while (!operation.isDone)
            {
                float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

                timer += Time.deltaTime;
                float fakeProgress = Mathf.Clamp01(timer / fakeLoadTime);
                targetProgress = Mathf.Max(realProgress, fakeProgress);

                loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetProgress, Time.deltaTime * 3f);

                if (progressText != null)
                {
                    progressText.text = $"{Mathf.RoundToInt(loadingSlider.value * 100)}%";
                }

                if (operation.progress >= 0.9f && timer >= minimumLoadTime)
                {
                    loadingSlider.value = 1f;
                    if (progressText != null)
                    {
                        progressText.text = "100%";
                    }

                    // Stop animations
                    StopLoadingAnimations();

                    yield return StartCoroutine(FadeLoadingScreen(false));

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        private IEnumerator FadeLoadingScreen(bool fadeIn)
        {
            if (loadingCanvasGroup == null) yield break;

            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            loadingCanvasGroup.alpha = endAlpha;
        }

        #region Animations

        private void StartLoadingAnimations()
        {
            if (loadingIcon != null)
            {
                iconAnimationCoroutine = StartCoroutine(RotateLoadingIcon());
            }

            if (pulsingImages != null && pulsingImages.Length > 0)
            {
                pulseAnimationCoroutine = StartCoroutine(PulseImages());
            }

            if (loadingText != null)
            {
                dotsAnimationCoroutine = StartCoroutine(AnimateLoadingDots());
            }
        }

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

        private IEnumerator RotateLoadingIcon()
        {
            while (true)
            {
                loadingIcon.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator PulseImages()
        {
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                float alpha = (Mathf.Sin(time) + 1f) / 2f; // Oscillates between 0 and 1

                foreach (Image img in pulsingImages)
                {
                    if (img != null)
                    {
                        Color color = img.color;
                        color.a = Mathf.Lerp(0.3f, 1f, alpha);
                        img.color = color;
                    }
                }

                yield return null;
            }
        }

        private IEnumerator ScaleAnimation(RectTransform target)
        {
            Vector3 originalScale = target.localScale;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * 2f;
                float scale = 1f + Mathf.Sin(time) * 0.1f; // Oscila entre 0.9 y 1.1
                target.localScale = originalScale * scale;
                yield return null;
            }
        }

        private IEnumerator AnimateLoadingDots()
        {
            string baseText = "Loading";
            int dotCount = 0;

            while (true)
            {
                dotCount = (dotCount + 1) % 4;
                loadingText.text = baseText + new string('.', dotCount);
                yield return new WaitForSeconds(dotsAnimationSpeed);
            }
        }

        #endregion
    }
}