using UnityEngine;
using TMPro;
using DG.Tweening;

namespace HumanLoop.UI
{
    public class TimeViewManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI prefixText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Core.TimeManager timeManager;
        [SerializeField] private string prefix = "WEEK ";

        public void UpdateTimeUI()
        {
            if (timeText == null || timeManager == null) return;

            // Update text
            prefixText.text = prefix;
            timeText.text = timeManager.CurrentWeek.ToString();

            // Visual feedback: small jump (Punch)
            timeText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f);
        }
    }
}