using UnityEngine;
using TMPro;
using DG.Tweening;
using HumanLoop.Core;

namespace HumanLoop.UI
{
    public class TimeViewManager : MonoBehaviour
    {
        // References to UI elements and TimeManager, set in the Unity Inspector
        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI timeText;

        // Reference to the TimeManager to get the current week, set in the Unity Inspector
        [Header("Time Manager")]
        [SerializeField] private TimeManager timeManager;

        // METHOD TO UPDATE THE TIME UI, CALLED FROM TimeManager WHEN THE WEEK CHANGES
        public void UpdateTimeUI()
        {
            if (timeText == null || timeManager == null) return;

            // Update text            
            timeText.text = timeManager.CurrentWeek.ToString();

            // Visual feedback: small jump (Punch)
            timeText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f);
        }
    }
}