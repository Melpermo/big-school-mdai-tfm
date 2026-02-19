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
            // Each time a card is completed, time advances.
            // This is a simple way to simulate time passing in the game, as each card represents a decision point that takes time to resolve.
            // By subscribing to the OnCardRemoved event, we ensure that time advances whenever a card is removed from the game, which typically happens when a player makes a decision on a card.
            CardController.OnCardRemoved += AdvanceTime;
        }

        private void OnDisable()
        {
            // Unsubscribe from the event to prevent memory leaks and unintended behavior when the object is disabled or destroyed.
            CardController.OnCardRemoved -= AdvanceTime;
        }

        // Advances time by one week and raises the onTimeAdvancedEvent.
        private void AdvanceTime()
        {
            CurrentWeek++;
            //Debug.Log($"Week: {CurrentWeek}");

            if (onTimeAdvancedEvent != null)
                onTimeAdvancedEvent.Raise();
        }
    }
}