using UnityEngine;
using HumanLoop.Events;
using HumanLoop.UI;

namespace HumanLoop.Core
{
    public class TimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int startingWeek = 1;
        [SerializeField] private GameEvent onTimeAdvancedEvent;

        public int CurrentWeek { get; private set; }

        private void Awake()
        {
            CurrentWeek = startingWeek;
        }

        private void OnEnable()
        {
            // Cada vez que una carta se completa, avanza el tiempo
            CardController.OnCardRemoved += AdvanceTime;
        }

        private void OnDisable()
        {
            CardController.OnCardRemoved -= AdvanceTime;
        }

        private void AdvanceTime()
        {
            CurrentWeek++;
            Debug.Log($"Week: {CurrentWeek}");

            if (onTimeAdvancedEvent != null)
                onTimeAdvancedEvent.Raise();
        }
    }
}