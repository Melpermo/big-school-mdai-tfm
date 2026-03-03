using DG.Tweening;
using HumanLoop.UI;
using System.Collections;
using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Manages transitions between Main Menu and Game within a single scene.
    /// Eliminates scene loading entirely for maximum performance and memory stability.
    /// Uses CanvasGroup for smooth transitions without destroying GameObjects.
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
        [SerializeField] private GameOverUIHandler gameOverUIHandler;

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

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
            // Ensure we start with the main menu visible and game scene hidden
            ShowMainMenuImmediate();           
        }

        private void ShowMainMenuImmediate()
        {
            // Show main menu
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.alpha = 1f;
                mainMenuCanvasGroup.interactable = true;
                mainMenuCanvasGroup.blocksRaycasts = true;
            }

            // Hide game scene
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.alpha = 0f;
                gameSceneCanvasGroup.interactable = false;
                gameSceneCanvasGroup.blocksRaycasts = false;
            }
        }

        #endregion

        #region Public API - State Transitions

        /// <summary>
        /// Transitions from Main Menu to Game Scene with fade.
        /// </summary>
        public void ShowGameScene()
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionToGameCoroutine());
        }

        /// <summary>
        /// Restarts the game (after Game Over) without changing scenes.
        /// </summary>
        public void RestartGame()
        {
            if (_isTransitioning) return;
            StartCoroutine(RestartGameCoroutine());
        }

        /// <summary>
        /// Returns to Main Menu from Game Scene with fade.
        /// </summary>
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

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // Hide main menu, show game scene
            HideMainMenu();
            ShowGameScene_Internal();

            // NUEVO: Reset game state (igual que Restart)
            ResetGame();

            // Wait one frame for reset to complete
            yield return null;

            // Fade from black
            yield return StartCoroutine(FadeOut());

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

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // Game scene remains visible, just cleanup
            CleanupGame();

            // Small delay for cleanup
            yield return new WaitForSeconds(0.2f);

            // Reset game state
            ResetGame();

            // Fade from black
            yield return StartCoroutine(FadeOut());

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

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // Cleanup game
            CleanupGame();

            // Hide game scene, show main menu
            HideGameScene();
            ShowMainMenu_Internal();

            // Fade from black
            yield return StartCoroutine(FadeOut());

            _isTransitioning = false;

            if (showDebugLogs)
            {
                long mem = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneStateManager] Returned to menu. Memory: {mem / 1048576}MB");
            }
        }

        #endregion

        #region Canvas Group Control

        private void ShowMainMenu_Internal()
        {
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.alpha = 1f;
                mainMenuCanvasGroup.interactable = true;
                mainMenuCanvasGroup.blocksRaycasts = true;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Main Menu shown");
        }

        private void HideMainMenu()
        {
            if (mainMenuCanvasGroup != null)
            {
                mainMenuCanvasGroup.alpha = 0f;
                mainMenuCanvasGroup.interactable = false;
                mainMenuCanvasGroup.blocksRaycasts = false;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Main Menu hidden");
        }

        private void ShowGameScene_Internal()
        {
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.alpha = 1f;
                gameSceneCanvasGroup.interactable = true;
                gameSceneCanvasGroup.blocksRaycasts = true;
            }            

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game Scene shown");
        }

        private void HideGameScene()
        {
            if (gameSceneCanvasGroup != null)
            {
                gameSceneCanvasGroup.alpha = 0f;
                gameSceneCanvasGroup.interactable = false;
                gameSceneCanvasGroup.blocksRaycasts = false;
            }

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game Scene hidden");
        }

        #endregion

        #region Game Management

        private void ResetGame()
        {
            // 1. Reset Stats to initial values
            if (statsManager != null)
            {
                statsManager.ResetToInitialValues();
            }

            // 2. Reset Time to starting week
            if (timeManager != null)
            {
                timeManager.ResetTime();
            }

            // 3. Reset Progression flags and deck
            if (progressionManager != null)
            {
                progressionManager.ResetProgression();
            }

            // 4. Reset Deck to initial state (spawns first cards)
            if (deckManager != null)
            {
                deckManager.ResetDeck();
            }

            // 5. Hide end game UIs
            if (endGameUIHandler != null)
            {
                endGameUIHandler.HideUI();
            }

            if (gameOverUIHandler != null)
            {
                gameOverUIHandler.gameObject.SetActive(false);
            }

            // 6. Update UI to reflect reset state
            if (statsViewManager != null)
            {
                statsViewManager.UpdateUI();
            }

            if (timeViewManager != null)
            {
                timeViewManager.UpdateTimeUI();
            }

            if (showDebugLogs)
            {
                Debug.Log("[SceneStateManager] Game reset complete - All managers reset to initial state");
            }
        }

        private void CleanupGame()
        {
            // Kill all active tweens
            DOTween.KillAll();
            DOTween.ClearCachedTweens();

            // Hide UIs
            if (endGameUIHandler != null)
                endGameUIHandler.HideUI();

            if (gameOverUIHandler != null)
                gameOverUIHandler.gameObject.SetActive(false);

            if (showDebugLogs)
                Debug.Log("[SceneStateManager] Game cleaned up");
        }

        #endregion

        #region Fade Effects

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;

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
            fadeCanvasGroup.gameObject.SetActive(false);
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
            Debug.Log($"=== CANVAS GROUP STATE ===\n" +
                     $"Main Menu - Alpha: {mainMenuCanvasGroup?.alpha}, Interactable: {mainMenuCanvasGroup?.interactable}\n" +
                     $"Game Scene - Alpha: {gameSceneCanvasGroup?.alpha}, Interactable: {gameSceneCanvasGroup?.interactable}");
        }
#endif

        #endregion
    }
}