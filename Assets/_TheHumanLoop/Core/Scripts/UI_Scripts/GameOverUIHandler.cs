using DG.Tweening;
using TMPro;
using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Handles Game Over UI presentation.
    /// Optimized for performance: Uses Canvas.enabled for instant show/hide.
    /// DOTween fade animation maintained for visual polish (efficient).
    /// </summary>
    public class GameOverUIHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup gameOverPanel;
        [SerializeField] private TextMeshProUGUI statsSummaryText;

        [Header("Canvas Components (Performance)")]
        [Tooltip("Canvas component of Game Over Panel - disabling stops rendering")]
        [SerializeField] private Canvas gameOverPanelCanvas;

        [Header("Dependencies")]
        [SerializeField] private TimeManager timeManager;

        [Header("Messages")]
        [SerializeField, TextArea(2, 4)]
        private string fireMessage = "¡Ve ha hablar con recursos humanos! ¡No has conseguido pasar el corte! Semanas en la empresa:";

        [Header("Animation Settings")]
        [Tooltip("Duration of fade-in animation")]
        [SerializeField] private float fadeDuration = 1f;

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

            // Disable Canvas rendering (performance optimized)
            SetCanvasState(gameOverPanelCanvas, false);

            gameOverPanel.alpha = 0f;
            gameOverPanel.interactable = false;
            gameOverPanel.blocksRaycasts = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the Game Over panel with fade animation.
        /// Called by GameStatsManager when game over condition is met.
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
        /// Restarts the game through SceneStateManager.
        /// </summary>
        public void RestartGame()
        {
            CleanupTweens();

            if (SceneStateManager.Instance != null)
            {
                SceneStateManager.Instance.RestartGame();
            }
            else
            {
                Debug.LogError("[GameOverUIHandler] SceneStateManager.Instance is null! Cannot restart game.");
            }
        }

        /// <summary>
        /// Returns to main menu through SceneStateManager.
        /// </summary>
        public void ReturnToMainMenu()
        {
            CleanupTweens();

            if (SceneStateManager.Instance != null)
            {
                SceneStateManager.Instance.ShowMainMenu();
            }
            else
            {
                Debug.LogError("[GameOverUIHandler] SceneStateManager.Instance is null! Cannot return to menu.");
            }
        }

        #endregion

        #region Private Methods

        private void ShowGameOverPanel()
        {
            CleanupTweens();

            // Enable Canvas rendering (performance optimized)
            SetCanvasState(gameOverPanelCanvas, true);

            gameOverPanel.alpha = 0f;
            gameOverPanel.interactable = false;
            gameOverPanel.blocksRaycasts = false;

            // Update stats text
            UpdateStatsText();

            // Fade in animation (appropriate for visual effect)
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
            if (gameOverPanel != null)
            {
                gameOverPanel.interactable = true;
                gameOverPanel.blocksRaycasts = true;
            }

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

        /// <summary>
        /// Enables/disables Canvas rendering.
        /// CRITICAL for performance: Disabled canvas stops rendering but preserves geometry buffer.
        /// </summary>
        private void SetCanvasState(Canvas canvas, bool enabled)
        {
            if (canvas == null) return;
            canvas.enabled = enabled;
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
            Debug.Log("[GameOverUIHandler] Testing restart");
            RestartGame();
        }

        [ContextMenu("Test/Return to Menu")]
        private void TestReturnToMenu()
        {
            Debug.Log("[GameOverUIHandler] Testing return to menu");
            ReturnToMainMenu();
        }

        [ContextMenu("Debug/Check Tween Status")]
        private void DebugCheckTweenStatus()
        {
            bool fadeActive = _fadeInTween != null && _fadeInTween.IsActive();
            Debug.Log($"[GameOverUIHandler] Fade Tween Active: {fadeActive}");
        }

        [ContextMenu("Debug/Check Canvas State")]
        private void DebugCheckCanvasState()
        {
            Debug.Log($"=== GAMEOVER CANVAS STATE ===\n" +
                     $"Panel Canvas: {GetCanvasStateString(gameOverPanelCanvas)}\n" +
                     $"Panel Alpha: {gameOverPanel?.alpha:F2}\n" +
                     $"Panel Interactable: {gameOverPanel?.interactable}\n" +
                     $"Panel Blocks Raycasts: {gameOverPanel?.blocksRaycasts}");
        }

        private string GetCanvasStateString(Canvas canvas)
        {
            if (canvas == null) return "NULL";
            return canvas.enabled ? "ENABLED (Rendering)" : "DISABLED (Not Rendering)";
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