using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HumanLoop.Core
{
    /// <summary>
    /// Handles Game Over UI presentation and scene restart.
    /// Optimized for WebGL with async scene loading and proper tween cleanup.
    /// </summary>
    public class GameOverUIHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup gameOverPanel;
        [SerializeField] private TextMeshProUGUI statsSummaryText;

        [Header("Dependencies")]
        [SerializeField] private TimeManager timeManager;

        [Header("Messages")]
        [SerializeField, TextArea(2, 4)]
        private string fireMessage = "¡Ve ha hablar con recursos humanos! ¡No has conseguido pasar el corte! Semanas en la empresa:";

        [Header("Animation Settings")]
        [Tooltip("Duration of fade-in animation")]
        [SerializeField] private float fadeDuration = 1f;

        [Header("Scene Loading")]
        [Tooltip("Use async scene loading (recommended for WebGL)")]
        [SerializeField] private bool useAsyncLoading = true;        

        // Tween management
        private Tween _fadeInTween;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            CleanupTweens();
        }

        private void OnDestroy()
        {
            CleanupTweens();
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            if (gameOverPanel == null)
            {
                Debug.LogError("[GameOverUIHandler] gameOverPanel is not assigned!");
                return;
            }

            gameOverPanel.gameObject.SetActive(false);
            gameOverPanel.alpha = 0f;
            gameOverPanel.interactable = false;
            gameOverPanel.blocksRaycasts = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the Game Over panel with fade animation.
        /// Called by GameEventListener in response to Game Over event.
        /// </summary>
        public void HandleGameOver()
        {
            if (gameOverPanel == null)
            {
                Debug.LogError("[GameOverUIHandler] Cannot show Game Over: panel is null");
                return;
            }

            ShowGameOverPanel();
        }

        /// <summary>
        /// Restarts the current scene.
        /// Uses async loading if enabled for better WebGL performance.
        /// </summary>
        public void RestartGame()
        {
            CleanupTweens();

            // Ensure time is running
            Time.timeScale = 1f;

            if (useAsyncLoading)
            {
                StartCoroutine(RestartGameAsync());
            }
            else
            {
                RestartGameSync();
            }
        }

        public void RestartGameOnSceneStateManager()
        { 
            //SceneStateManager.Instance.RestartScene();
        }

        #endregion

        #region Private Methods

        private void ShowGameOverPanel()
        {
            // Cleanup any previous animation
            CleanupTweens();

            // Activate panel
            gameOverPanel.gameObject.SetActive(true);
            gameOverPanel.alpha = 0f;

            // Update stats text
            UpdateStatsText();

            // Fade in animation
            _fadeInTween = gameOverPanel
                .DOFade(1f, fadeDuration)
                .SetUpdate(true) // Works even if Time.timeScale = 0
                .SetTarget(gameOverPanel)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(OnFadeInComplete);
        }

        private void OnFadeInComplete()
        {
            // Enable interaction after fade completes
            gameOverPanel.interactable = true;
            gameOverPanel.blocksRaycasts = true;

            _fadeInTween = null;
        }

        private void UpdateStatsText()
        {
            if (statsSummaryText == null)
            {
                Debug.LogWarning("[GameOverUIHandler] statsSummaryText is not assigned");
                return;
            }

            if (timeManager == null)
            {
                Debug.LogWarning("[GameOverUIHandler] timeManager is not assigned");
                statsSummaryText.text = fireMessage;
                return;
            }

            statsSummaryText.text = $"{fireMessage} {timeManager.CurrentWeek}";
        }

        #endregion

        #region Scene Loading

        private void RestartGameSync()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }

        private IEnumerator RestartGameAsync()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // Optional: Show loading indicator here
            // loadingIndicator?.SetActive(true);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentSceneIndex);

            // Wait until scene is loaded
            while (!asyncLoad.isDone)
            {
                // Optional: Update loading progress bar
                // float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                // loadingProgressBar?.SetProgress(progress);

                yield return null;
            }
        }

        #endregion

        #region Tween Cleanup

        private void CleanupTweens()
        {
            // Kill fade tween if active
            if (_fadeInTween != null && _fadeInTween.IsActive())
            {
                _fadeInTween.Kill(complete: false);
                _fadeInTween = null;
            }

            // Safety: Kill all tweens on panel
            if (gameOverPanel != null)
            {
                gameOverPanel.DOKill(complete: false);
            }
        }

        #endregion

        #region Debug & Testing

#if UNITY_EDITOR
        [ContextMenu("Test/Show Game Over")]
        private void TestShowGameOver()
        {
            HandleGameOver();
        }

        [ContextMenu("Test/Restart Game")]
        private void TestRestartGame()
        {
            Debug.Log("[GameOverUIHandler] Testing restart (will reload scene)");
            RestartGame();
        }

        [ContextMenu("Debug/Check Tween Status")]
        private void DebugCheckTweenStatus()
        {
            bool fadeActive = _fadeInTween != null && _fadeInTween.IsActive();
            Debug.Log($"[GameOverUIHandler] Fade Tween Active: {fadeActive}");
        }

        private void OnValidate()
        {
            // Ensure sensible values
            fadeDuration = Mathf.Max(0.1f, fadeDuration);
        }
#endif

        #endregion
    }
}