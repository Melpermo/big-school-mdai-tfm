using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HumanLoop.Core;

namespace HumanLoop.UI
{
    /// <summary>
    /// Updates the UI Sliders to reflect the current game stats.
    /// Optimized for WebGL with proper tween cleanup to prevent memory leaks.
    /// </summary>
    public class StatsViewManager : MonoBehaviour
    {
        [Header("Slider References")]
        [SerializeField] private Slider budgetSlider;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Slider moraleSlider;
        [SerializeField] private Slider qualitySlider;
        
        [Header("Progress Texts")]
        [SerializeField] private TextMeshProUGUI budgetProgressText;
        [SerializeField] private TextMeshProUGUI timeProgressText;
        [SerializeField] private TextMeshProUGUI moraleProgressText;
        [SerializeField] private TextMeshProUGUI qualityProgressText;

        [Header("Animation Settings")]
        [SerializeField] private float lerpDuration = 0.5f;

        // Tween references for cleanup
        private Tween _budgetTween;
        private Tween _timeTween;
        private Tween _moraleTween;
        private Tween _qualityTween;

        #region Unity Lifecycle

        private void Start()
        {
            if (GameStatsManager.Instance != null)
            {
                UpdateUIImmediate();
            }
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

        #region UI Updates

        /// <summary>
        /// Animates sliders to their new values with proper tween management.
        /// </summary>
        public void UpdateUI()
        {
            var stats = GameStatsManager.Instance;

            // Kill previous tweens BEFORE creating new ones
            CleanupTweens();

            // Create new tweens with references
            _budgetTween = budgetSlider
                .DOValue(stats.budget, lerpDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(budgetSlider)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(() => _budgetTween = null);

            _timeTween = timeSlider
                .DOValue(stats.time, lerpDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(timeSlider)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(() => _timeTween = null);

            _moraleTween = moraleSlider
                .DOValue(stats.morale, lerpDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(moraleSlider)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(() => _moraleTween = null);

            _qualityTween = qualitySlider
                .DOValue(stats.quality, lerpDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(qualitySlider)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .OnComplete(() => _qualityTween = null);

            UpdateProgressText(stats);
        }

        private void UpdateUIImmediate()
        {
            var stats = GameStatsManager.Instance;
            budgetSlider.value = stats.budget;
            timeSlider.value = stats.time;
            moraleSlider.value = stats.morale;
            qualitySlider.value = stats.quality;

            UpdateProgressText(stats);
        }

        private void UpdateProgressText(GameStatsManager stats)
        {
            if (budgetProgressText != null) budgetProgressText.text = $"{(int)(stats.budget)}%";
            if (timeProgressText != null) timeProgressText.text = $"{(int)(stats.time)}%";
            if (moraleProgressText != null) moraleProgressText.text = $"{(int)(stats.morale)}%";
            if (qualityProgressText != null) qualityProgressText.text = $"{(int)(stats.quality)}%";
        }

        #endregion

        #region Cleanup

        private void CleanupTweens()
        {
            if (_budgetTween != null && _budgetTween.IsActive())
            {
                _budgetTween.Kill(complete: false);
                _budgetTween = null;
            }

            if (_timeTween != null && _timeTween.IsActive())
            {
                _timeTween.Kill(complete: false);
                _timeTween = null;
            }

            if (_moraleTween != null && _moraleTween.IsActive())
            {
                _moraleTween.Kill(complete: false);
                _moraleTween = null;
            }

            if (_qualityTween != null && _qualityTween.IsActive())
            {
                _qualityTween.Kill(complete: false);
                _qualityTween = null;
            }

            // Safety cleanup
            if (budgetSlider != null) budgetSlider.DOKill(complete: false);
            if (timeSlider != null) timeSlider.DOKill(complete: false);
            if (moraleSlider != null) moraleSlider.DOKill(complete: false);
            if (qualitySlider != null) qualitySlider.DOKill(complete: false);
        }

        #endregion
    }
}
