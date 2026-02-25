using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using HumanLoop.Core;

namespace HumanLoop.UI
{
    /// <summary>
    /// Updates the UI Sliders to reflect the current game stats using DOTween.
    /// </summary>
    public class StatsViewManager : MonoBehaviour
    {
        [Header("Slider References")]
        [SerializeField] private Slider budgetSlider;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Slider moraleSlider;
        [SerializeField] private Slider qualitySlider;
        
        [Header("Progressins Texts")]
        [SerializeField] private TextMeshProUGUI budgetProgressText;
        [SerializeField] private TextMeshProUGUI timeProgressText;
        [SerializeField] private TextMeshProUGUI moraleProgressText;
        [SerializeField] private TextMeshProUGUI qualityProgressText;

        [Header("Animation Settings")]
        [SerializeField] private float lerpDuration = 0.5f;      


        private void Start()
        {
            // Ensure the Singleton exists before the first update
            if (GameStatsManager.Instance != null)
            {
                UpdateUIImmediate();
            }
        }

        /// <summary>
        /// Animates sliders to their new values.
        /// </summary>
        public void UpdateUI()
        {
            var stats = GameStatsManager.Instance;

            //Debug.Log("StatsViewManager: I received the signal! Animating bars...");

            budgetSlider.DOValue(stats.budget, lerpDuration).SetEase(Ease.OutCubic);
            timeSlider.DOValue(stats.time, lerpDuration).SetEase(Ease.OutCubic);
            moraleSlider.DOValue(stats.morale, lerpDuration).SetEase(Ease.OutCubic);
            qualitySlider.DOValue(stats.quality, lerpDuration).SetEase(Ease.OutCubic);

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
            budgetProgressText.text = $"{(int)(stats.budget)}%";
            timeProgressText.text = $"{(int)(stats.time)}%";
            moraleProgressText.text = $"{(int)(stats.morale)}%";
            qualityProgressText.text = $"{(int)(stats.quality)}%";
        }
    }
}
