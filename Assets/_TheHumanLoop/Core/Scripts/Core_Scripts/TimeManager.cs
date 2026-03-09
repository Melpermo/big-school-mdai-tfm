using UnityEngine;
using HumanLoop.Events;
using HumanLoop.UI;

namespace HumanLoop.Core
{
    public class TimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int startingWeek = 1;

        [SerializeField] private TimeViewManager timeViewManager; // Reference to the TimeViewManager to update the UI when time advances

        [Header("Raise Event Optional")]
        [Tooltip("Optional event to raise when time advances. if it's null it doesn't happened")]
        [SerializeField] private GameEventSO onTimeAdvancedEvent;        

        public int CurrentWeek { get; private set; }

        private void Awake()
        {
            SetInitValues();
        }      

        private void OnEnable()
        {
            CardController.OnCardRemoved += AdvanceTime;
        }

        private void OnDisable()
        {
            CardController.OnCardRemoved -= AdvanceTime;
        }

        private void SetInitValues()
        {
            CurrentWeek = startingWeek;
            if (timeViewManager != null)
            {
                timeViewManager.UpdateTimeUI();
            }
        }


        private void AdvanceTime()
        {
            CurrentWeek++;
            timeViewManager.UpdateTimeUI();
            
            if (onTimeAdvancedEvent != null)
                onTimeAdvancedEvent.Raise();
        }

        /// <summary>
        /// Resets the current week to starting value.
        /// Called when restarting the game.
        /// </summary>
        public void ResetTime()
        {
            CurrentWeek = startingWeek;

            // Optionally raise event to update UI
            if (onTimeAdvancedEvent != null)
                onTimeAdvancedEvent.Raise();
        }
    }
}