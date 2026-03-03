using UnityEngine;
using HumanLoop.UI;
using System.Collections;
using DG.Tweening;

namespace HumanLoop.Core
{
    /// <summary>
    /// Manages game session lifecycle: Start, Reset, End.
    /// Replaces scene reloading with efficient state reset.
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Manager References")]
        [SerializeField] private GameStatsManager statsManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private ProgressionManager progressionManager;
        [SerializeField] private CardFactory cardFactory;

        [Header("UI References")]
        [SerializeField] private StatsViewManager statsViewManager;
        [SerializeField] private TimeViewManager timeViewManager;
        [SerializeField] private EndGameUIHandler endGameUIHandler;
        [SerializeField] private GameOverUIHandler gameOverUIHandler;

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private bool _isGameActive = false;

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

            ValidateReferences();
        }

        #endregion

        #region Validation

        private void ValidateReferences()
        {
            if (statsManager == null) Debug.LogError("[GameSessionManager] GameStatsManager not assigned!");
            if (timeManager == null) Debug.LogError("[GameSessionManager] TimeManager not assigned!");
            if (deckManager == null) Debug.LogError("[GameSessionManager] DeckManager not assigned!");
            if (cardFactory == null) Debug.LogError("[GameSessionManager] CardFactory not assigned!");
        }

        #endregion

        #region Session Lifecycle

        /// <summary>
        /// Starts a new game session without reloading the scene.
        /// </summary>
        public void StartNewGame()
        {
            if (_isGameActive)
            {
                Debug.LogWarning("[GameSessionManager] Game already active!");
                return;
            }

            StartCoroutine(StartNewGameCoroutine());
        }

        /// <summary>
        /// Restarts the current game session (after Game Over).
        /// Much faster than reloading the scene.
        /// </summary>
        public void RestartGame()
        {
            StartCoroutine(RestartGameCoroutine());
        }

        /// <summary>
        /// Ends the current game session and returns to main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            StartCoroutine(ReturnToMainMenuCoroutine());
        }

        #endregion

        #region Coroutines

        private IEnumerator StartNewGameCoroutine()
        {
            if (showDebugLogs)
                Debug.Log("[GameSessionManager] Starting new game...");

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // Initialize game state
            ResetAllManagers();
            ResetAllUI();

            // Fade from black
            yield return StartCoroutine(FadeOut());

            _isGameActive = true;

            if (showDebugLogs)
                Debug.Log("[GameSessionManager] Game started!");
        }

        private IEnumerator RestartGameCoroutine()
        {
            if (showDebugLogs)
            {
                long memBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[GameSessionManager] Restarting game... Memory: {memBefore / 1048576}MB");
            }

            _isGameActive = false;

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // CRITICAL: Clean up current game state
            CleanupGameSession();

            // Small delay for cleanup to complete
            yield return new WaitForSeconds(0.2f);

            // Reset everything
            ResetAllManagers();
            ResetAllUI();

            // Fade from black
            yield return StartCoroutine(FadeOut());

            _isGameActive = true;

            if (showDebugLogs)
            {
                long memAfter = System.GC.GetTotalMemory(false);
                Debug.Log($"[GameSessionManager] Game restarted! Memory: {memAfter / 1048576}MB");
            }
        }

        private IEnumerator ReturnToMainMenuCoroutine()
        {
            if (showDebugLogs)
                Debug.Log("[GameSessionManager] Returning to main menu...");

            _isGameActive = false;

            // Fade to black
            yield return StartCoroutine(FadeIn());

            // Cleanup
            CleanupGameSession();

            // Load main menu scene (only time we actually reload)
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region Reset Methods

        /// <summary>
        /// Resets all game managers to initial state.
        /// </summary>
        private void ResetAllManagers()
        {
            // Reset stats to initial values
            if (statsManager != null)
            {
                statsManager.ResetToInitialValues();
            }

            // Reset time counter
            // TimeManager doesn't need reset (it resets on Start automatically via CardController event)

            // Reset progression flags
            // ProgressionManager doesn't need manual reset (tracks via TimeManager)

            // Note: DeckManager doesn't need reset (it reinitializes on Start)

            if (showDebugLogs)
                Debug.Log("[GameSessionManager] All managers reset");
        }

        /// <summary>
        /// Resets all UI elements to initial state.
        /// </summary>
        private void ResetAllUI()
        {
            // Hide end game UIs
            if (endGameUIHandler != null)
            {
                endGameUIHandler.HideUI();
            }

            if (gameOverUIHandler != null)
            {
                // Assuming you add a HideUI method
                gameOverUIHandler.gameObject.SetActive(false);
            }

            // Update stats UI immediately
            if (statsViewManager != null)
            {
                statsViewManager.UpdateUI();
            }

            // Update time UI
            if (timeViewManager != null)
            {
                timeViewManager.UpdateTimeUI();
            }

            if (showDebugLogs)
                Debug.Log("[GameSessionManager] All UI reset");
        }

        /// <summary>
        /// Cleans up current game session before reset.
        /// CRITICAL for preventing memory leaks.
        /// </summary>
        private void CleanupGameSession()
        {
            // 1. Kill all active tweens
            DOTween.KillAll();
            DOTween.ClearCachedTweens();

            // 2. Return all cards to pool
            if (cardFactory != null)
            {
                // Cards should already be returned via CardController,
                // but this ensures cleanup
                // Note: You might need to add a ForceReturnAll method to CardFactory
            }

            // 3. Stop all coroutines (except this one)
            // Already handled by individual managers

            if (showDebugLogs)
                Debug.Log("[GameSessionManager] Session cleaned up");
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
        [ContextMenu("Test/Restart Game")]
        private void TestRestartGame()
        {
            RestartGame();
        }

        [ContextMenu("Test/Log Memory")]
        private void TestLogMemory()
        {
            long mem = System.GC.GetTotalMemory(false);
            Debug.Log($"[GameSessionManager] Current Memory: {mem / 1048576}MB");
        }
#endif

        #endregion
    }
}