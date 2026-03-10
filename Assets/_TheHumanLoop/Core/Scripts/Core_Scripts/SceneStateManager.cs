using DG.Tweening;
using HumanLoop.UI;
using System.Collections;
using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Manages transitions between Main Menu and Game within a single scene.
    /// Optimized for performance: Uses Canvas.enabled to stop rendering without destroying hierarchy.
    /// Optional fade transition for visual polish.
    /// </summary>
    public class SceneStateManager : MonoBehaviour
    {
        public static SceneStateManager Instance { get; private set; }

        [Header("Scene Root GameObjects")]
        [SerializeField] private GameObject mainMenuRoot;
        [SerializeField] private GameObject gameSceneRoot;

        [Header("Scene Root CanvasGroups")]
        [SerializeField] private CanvasGroup mainMenuCanvasGroup;
        [SerializeField] private CanvasGroup gameSceneCanvasGroup;

        [Header("Canvas Components (Performance)")]
        [Tooltip("Canvas component of Main Menu - disabling stops rendering while preserving geometry")]
        [SerializeField] private Canvas mainMenuCanvas;
        
        [Tooltip("Canvas component of Game Scene - disabling stops rendering while preserving geometry")]
        [SerializeField] private Canvas gameSceneCanvas;

        [Header("Visual Transition (Optional)")]
        [Tooltip("If true, uses black fade overlay for transitions. If false, instant switch (better performance)")]
        [SerializeField] private bool useFadeTransition = true;
        
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Canvas fadeCanvas;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Game Managers")]
        [SerializeField] private GameStatsManager statsManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private ProgressionManager progressionManager;
        [SerializeField] private CardFactory cardFactory;

        [Header("Game UI")]
        [SerializeField] private StatsViewManager statsViewManager;
        [SerializeField] private TimeViewManager timeViewManager;
        [SerializeField] private EndGameUIHandler endGameUIHandler;        

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private bool _isTransitioning = false;

        #region Singleton

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion

        #region Initialization

        void Start()
        {
            ShowMainMenuImmediate();
        }

        private void ShowMainMenuImmediate()
        {
            // Main Menu: Visible
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.alpha = 1f;
                mainMenuCanvasGroup.interactable = true;
                mainMenuCanvasGroup.blocksRaycasts = true;
            }
            
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.enabled = true;
            }

            // Game Scene: Hidden
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.alpha = 1f; // Keep at 1, Canvas.enabled controls visibility
                gameSceneCanvasGroup.interactable = false;
                gameSceneCanvasGroup.blocksRaycasts = false;
            }
            
            if (gameSceneCanvas != null)
            {
                gameSceneCanvas.enabled = false; // Stop rendering
            }

            // Fade: Hidden
            if (fadeCanvas != null)
            {
                fadeCanvas.enabled = false;
            }
        }

        #endregion

        #region Public API - State Transitions

        public void ShowGameScene()
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionToGameCoroutine());
        }

        public void RestartGame()
        {
            if (_isTransitioning) return;
            StartCoroutine(RestartGameCoroutine());
        }

        public void ShowMainMenu()
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionToMenuCoroutine());
        }

        #endregion

        #region Coroutines

        private IEnumerator TransitionToGameCoroutine()
        {
            _isTransitioning = true;

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Transitioning to Game...");

            // Optional fade to black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }

            // Switch: Hide menu, show game
            HideMainMenu();
            ShowGameScene_Internal();

            // Reset game state
            ResetGame();
            yield return null;

            // Optional fade from black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }

            _isTransitioning = false;

            if (showDebugLogs)
            {
                long mem = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneStateManager] Game started. Memory: {mem / 1048576}MB");
            }
        }

        private IEnumerator RestartGameCoroutine()
        {
            _isTransitioning = true;

            if (showDebugLogs)
            {
                long memBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneStateManager] Restarting game... Memory: {memBefore / 1048576}MB");
            }

            // Optional fade to black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }

            // Cleanup and reset
            CleanupGame();
            yield return new WaitForSeconds(0.2f);
            ResetGame();

            // Optional fade from black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }

            _isTransitioning = false;

            if (showDebugLogs)
            {
                long memAfter = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneStateManager] Game restarted. Memory: {memAfter / 1048576}MB");
            }
        }

        private IEnumerator TransitionToMenuCoroutine()
        {
            _isTransitioning = true;

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Returning to Main Menu...");

            // Optional fade to black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeIn());
            }

            // Cleanup and switch
            CleanupGame();
            HideGameScene();
            ShowMainMenu_Internal();

            // Optional fade from black
            if (useFadeTransition)
            {
                yield return StartCoroutine(FadeOut());
            }

            _isTransitioning = false;

            if (showDebugLogs)
            {
                long mem = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneStateManager] Returned to menu. Memory: {mem / 1048576}MB");
            }
        }

        #endregion

        #region Canvas Control (Performance Optimized)  

        private void ShowMainMenu_Internal()
        {
            // Enable interactivity
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.interactable = true;
                mainMenuCanvasGroup.blocksRaycasts = true;
            }

            // CRITICAL: Enable Canvas to start rendering
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.enabled = true;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Main Menu shown (Canvas enabled)");
        }

        private void HideMainMenu()
        {
            // Disable interactivity
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.interactable = false;
                mainMenuCanvasGroup.blocksRaycasts = false;
            }

            // CRITICAL: Disable Canvas to stop rendering (preserves geometry buffer)
            if (mainMenuCanvas != null)
            {
                mainMenuCanvas.enabled = false;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Main Menu hidden (Canvas disabled)");
        }

        private void ShowGameScene_Internal()
        {
            // Enable interactivity
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.interactable = true;
                gameSceneCanvasGroup.blocksRaycasts = true;
            }

            // CRITICAL: Enable Canvas to start rendering
            if (gameSceneCanvas != null)
            {
                gameSceneCanvas.enabled = true;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game Scene shown (Canvas enabled)");
        }

        private void HideGameScene()
        {
            // Disable interactivity
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.interactable = false;
                gameSceneCanvasGroup.blocksRaycasts = false;
            }

            // CRITICAL: Disable Canvas to stop rendering (preserves geometry buffer)
            if (gameSceneCanvas != null)
            {
                gameSceneCanvas.enabled = false;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game Scene hidden (Canvas disabled)");
        }

        #endregion

        #region Game Management

        private void ResetGame()
        {
            if (statsManager != null)
                statsManager.ResetToInitialValues();

            if (timeManager != null)
                timeManager.ResetTime();

            if (progressionManager != null)
                progressionManager.ResetProgression();

            if (deckManager != null)
                deckManager.ResetDeck();

            if (endGameUIHandler != null)
                endGameUIHandler.HideUI();

            if (statsViewManager != null)
                statsViewManager.UpdateUI();

            if (timeViewManager != null)
                timeViewManager.UpdateTimeUI();

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game reset complete");
        }

        private void CleanupGame()
        {
            DOTween.KillAll();
            DOTween.ClearCachedTweens();

            if (endGameUIHandler != null)
                endGameUIHandler.HideUI();

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game cleaned up");
        }

        #endregion

        #region Fade Effects (Optional Transition)

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null || fadeCanvas == null) yield break;

            // Enable fade canvas
            fadeCanvas.enabled = true;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.alpha = 0f;

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
            if (fadeCanvasGroup == null || fadeCanvas == null) yield break;

            fadeCanvasGroup.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            
            // Disable fade canvas
            fadeCanvas.enabled = false;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test/Show Game")]
        private void TestShowGame()
        {
            ShowGameScene();
        }

        [ContextMenu("Test/Show Menu")]
        private void TestShowMenu()
        {
            ShowMainMenu();
        }

        [ContextMenu("Test/Restart Game")]
        private void TestRestart()
        {
            RestartGame();
        }

        [ContextMenu("Debug/Log Memory")]
        private void TestLogMemory()
        {
            long mem = System.GC.GetTotalMemory(false);
            Debug.Log($"[SceneStateManager] Current Memory: {mem / 1048576}MB");
        }

        [ContextMenu("Debug/Log Canvas State")]
        private void TestLogCanvasState()
        {
            Debug.Log($"=== CANVAS STATE ===\n" +
                     $"Main Menu Canvas: {(mainMenuCanvas != null ? (mainMenuCanvas.enabled ? "ENABLED" : "DISABLED") : "NULL")}\n" +
                     $"Game Scene Canvas: {(gameSceneCanvas != null ? (gameSceneCanvas.enabled ? "ENABLED" : "DISABLED") : "NULL")}\n" +
                     $"Fade Canvas: {(fadeCanvas != null ? (fadeCanvas.enabled ? "ENABLED" : "DISABLED") : "NULL")}");
        }
#endif

        #endregion
    }
}