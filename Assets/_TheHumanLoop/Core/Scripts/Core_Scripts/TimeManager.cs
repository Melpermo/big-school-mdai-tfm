using UnityEngine;
using HumanLoop.Events;
using HumanLoop.UI;

namespace HumanLoop.Core
{
    public class TimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int startingWeek = 1;
        [SerializeField] private GameEventSO onTimeAdvancedEvent;

        public int CurrentWeek { get; private set; }

        private void Awake()
        {
            CurrentWeek = startingWeek;
        }

        private void OnEnable()
        {
            CardController.OnCardRemoved += AdvanceTime;
        }

        private void OnDisable()
        {
            CardController.OnCardRemoved -= AdvanceTime;
        }

        private void OnDestroy()
        {
            CardController.OnCardRemoved -= AdvanceTime;
        }

        private void AdvanceTime()
        {
            CurrentWeek++;
            if (onTimeAdvancedEvent != null)
                onTimeAdvancedEvent.Raise();
        }
    }
}