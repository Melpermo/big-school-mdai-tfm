using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace TheHumanLoop.LoadingScreenSystem
{
    /// <summary>
    /// Minimalist scene loader optimized for WebGL.
    /// Uses simple fade transitions with aggressive memory cleanup.
    /// </summary>
    public class SimpleSceneLoader : MonoBehaviour
    {
        [Header("UI Elements (Minimal)")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Image logoImage;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadTime = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private bool _isLoading = false;

        private void Start()
        {
            InitializeFadeCanvas();
        }        

        #region Initialization

        private void InitializeFadeCanvas()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }

            if (logoImage != null)
            {
                logoImage.gameObject.SetActive(false);
            }

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Loads a scene by name with fade transition and memory cleanup.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SimpleSceneLoader] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Loads a scene by build index with fade transition and memory cleanup.
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SimpleSceneLoader] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        #endregion

        #region Scene Loading

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _isLoading = true;

            if (showDebugLogs)
            {
                long memBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Starting load: {sceneName}, Memory: {memBefore / 1048576}MB");
            }

            // Step 1: Fade to black
            yield return StartCoroutine(FadeIn());

            // Step 2: Show minimal loading UI
            ShowLoadingUI();

            // Step 3: Aggressive pre-load cleanup
            yield return StartCoroutine(PreLoadCleanup());

            // Step 4: Load scene asynchronously
            float startTime = Time.realtimeSinceStartup;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Step 5: Ensure minimum load time (prevents jarring quick loads)
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minimumLoadTime)
            {
                yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);
            }

            // Step 6: Aggressive post-load cleanup
            yield return StartCoroutine(PostLoadCleanup());

            // Step 7: Hide loading UI
            HideLoadingUI();

            // Step 8: Fade from black
            yield return StartCoroutine(FadeOut());

            _isLoading = false;

            if (showDebugLogs)
            {
                long memAfter = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Load complete: {sceneName}, Memory: {memAfter / 1048576}MB");
            }
        }

        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            _isLoading = true;

            if (showDebugLogs)
            {
                long memBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Starting load: Scene {sceneIndex}, Memory: {memBefore / 1048576}MB");
            }

            // Step 1: Fade to black
            yield return StartCoroutine(FadeIn());

            // Step 2: Show minimal loading UI
            ShowLoadingUI();

            // Step 3: Aggressive pre-load cleanup
            yield return StartCoroutine(PreLoadCleanup());

            // Step 4: Load scene asynchronously
            float startTime = Time.realtimeSinceStartup;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Step 5: Ensure minimum load time
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minimumLoadTime)
            {
                yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);
            }

            // Step 6: Aggressive post-load cleanup
            yield return StartCoroutine(PostLoadCleanup());

            // Step 7: Hide loading UI
            HideLoadingUI();

            // Step 8: Fade from black
            yield return StartCoroutine(FadeOut());

            _isLoading = false;

            if (showDebugLogs)
            {
                long memAfter = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Load complete: Scene {sceneIndex}, Memory: {memAfter / 1048576}MB");
            }
        }

        #endregion

        #region Fade Effects

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Loading UI

        private void ShowLoadingUI()
        {
            if (logoImage != null)
            {
                logoImage.gameObject.SetActive(true);
            }

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(true);
                loadingText.text = "Cargando...";
            }
        }

        private void HideLoadingUI()
        {
            if (logoImage != null)
            {
                logoImage.gameObject.SetActive(false);
            }

            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Memory Cleanup

        /// <summary>
        /// Aggressive cleanup BEFORE loading new scene.
        /// </summary>
        private IEnumerator PreLoadCleanup()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SimpleSceneLoader] Pre-load cleanup starting...");
            }

            // Unload unused assets (textures, audio, etc.)
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            yield return unloadOp;

            // Force garbage collection (multiple passes)
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            if (showDebugLogs)
            {
                long mem = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Pre-load cleanup done. Memory: {mem / 1048576}MB");
            }
        }

        /// <summary>
        /// Aggressive cleanup AFTER loading new scene.
        /// </summary>
        private IEnumerator PostLoadCleanup()
        {
            // Wait for scene to fully initialize
            yield return new WaitForSecondsRealtime(0.5f);

            if (showDebugLogs)
            {
                Debug.Log("[SimpleSceneLoader] Post-load cleanup starting...");
            }

            // Unload unused assets again
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            yield return unloadOp;

            // Force garbage collection again
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            if (showDebugLogs)
            {
                long mem = System.GC.GetTotalMemory(false);
                Debug.Log($"[SimpleSceneLoader] Post-load cleanup done. Memory: {mem / 1048576}MB");
            }
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        [ContextMenu("Test/Load Main Menu")]
        private void TestLoadMainMenu()
        {
            LoadScene("MainMenu");
        }

        [ContextMenu("Test/Load Game Scene")]
        private void TestLoadGameScene()
        {
            LoadScene("GameScene");
        }
        #endif

        #endregion
    }
}
