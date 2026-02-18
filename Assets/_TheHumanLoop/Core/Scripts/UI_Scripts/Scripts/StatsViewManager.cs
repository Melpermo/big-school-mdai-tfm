using UnityEngine;
using UnityEngine.UI;
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

        [Header("Animation Settings")]
        [SerializeField] private float lerpDuration = 0.5f;

        /*
        private void OnEnable()
        {
            // Subscribe to the global stats event
            GameStatsManager.OnStatsChanged += UpdateUI;
        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            GameStatsManager.OnStatsChanged -= UpdateUI;
        }*/


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

            Debug.Log("StatsViewManager: I received the signal! Animating bars...");

            budgetSlider.DOValue(stats.budget, lerpDuration).SetEase(Ease.OutCubic);
            timeSlider.DOValue(stats.time, lerpDuration).SetEase(Ease.OutCubic);
            moraleSlider.DOValue(stats.morale, lerpDuration).SetEase(Ease.OutCubic);
            qualitySlider.DOValue(stats.quality, lerpDuration).SetEase(Ease.OutCubic);           
           
        }

        private void UpdateUIImmediate()
        {
            var stats = GameStatsManager.Instance;
            budgetSlider.value = stats.budget;
            timeSlider.value = stats.time;
            moraleSlider.value = stats.morale;
            qualitySlider.value = stats.quality;
        }
    }
}
