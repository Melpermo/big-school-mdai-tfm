using UnityEngine;
using TMPro;
using DG.Tweening;
using HumanLoop.Core;

namespace HumanLoop.UI
{
    /// <summary>
    /// Updates the time/week UI text with animated feedback.
    /// Optimized for WebGL with proper tween cleanup.
    /// </summary>
    public class TimeViewManager : MonoBehaviour
    {
        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Time Manager")]
        [SerializeField] private TimeManager timeManager;

        [Header("Animation Settings")]
        [SerializeField] private float punchScale = 0.2f;
        [SerializeField] private float punchDuration = 0.3f;

        // Tween reference for cleanup
        private Tween _punchTween;

        #region Unity Lifecycle

        private void OnDisable()
        {
            CleanupTween();
        }

        private void OnDestroy()
        {
            CleanupTween();
        }

        #endregion

        #region UI Update

        /// <summary>
        /// Updates the time UI with animated punch effect.
        /// Called when week changes.
        /// </summary>
        public void UpdateTimeUI()
        {
            if (timeText == null || timeManager == null) return;

            // Update text
            timeText.text = timeManager.CurrentWeek.ToString();

            // Kill previous animation BEFORE creating new one
            CleanupTween();

            // Visual feedback: small jump (Punch)
            _punchTween = timeText.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f)
                .SetTarget(timeText.transform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true) // WebGL safe
                .OnComplete(() => _punchTween = null);
        }

        #endregion

        #region Cleanup

        private void CleanupTween()
        {
            if (_punchTween != null && _punchTween.IsActive())
            {
                _punchTween.Kill(complete: false);
                _punchTween = null;
            }

            // Safety cleanup
            if (timeText != null && timeText.transform != null)
            {
                timeText.transform.DOKill(complete: false);
            }
        }

        #endregion
    }
}