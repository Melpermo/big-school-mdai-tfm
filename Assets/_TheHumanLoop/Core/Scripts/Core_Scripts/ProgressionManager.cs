using HumanLoop.Data;
using HumanLoop.Events;
using UnityEngine;

namespace HumanLoop.Core
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager _deckManager;
        [SerializeField] private TimeManager _timeManager;

        // This toggle allows you to decide whether reaching progression milestones should automatically load new decks.
        // If false, the decks will not change, but the appropriate events will still be raised.
        [Tooltip("This toggle allows you to decide whether reaching progression milestones should automatically load new decks.")]
        [SerializeField] private bool _useDecksForProgression = false;

        [Header("Deck Progression")]
        [Tooltip("Initial deck loaded at game start and when resetting")]
        [SerializeField] private DeckSO _initialDeck;
        
        [Header("Progression Milestones")]
        [SerializeField] private int _midGameWeek = 10;
        [SerializeField] private DeckSO _midGameDeck;

        [SerializeField] private int _endGameWeek = 25;
        [SerializeField] private DeckSO _endGameDeck;

        [Header("Phase Victory Settings")]
        [SerializeField] private int _victoryWeek = 50; // Week to win
        [SerializeField] private GameEventSO _onVictoryEvent;
        [SerializeField] private GameEventSO _onMidGameReachedEvent;
        [SerializeField] private GameEventSO _onEndGameReachedEvent;

        // Internal state to track progression milestones
        [SerializeField] private bool midGameReached = false; // Serialized for debugging purposes, but should be managed through code logic
        [SerializeField] private bool endGameReached = false; // Serialized for debugging purposes, but should be managed through code logic
        [SerializeField] private bool victoryReached = false; // Serialized for debugging purposes, but should be managed through code logic

        [SerializeField] private int currentWeek; // Serialized for debugging purposes, but should be updated from TimeManager

        // This method should be called whenever the week changes to check if any progression milestones have been reached.
        public void CheckProgression()
        {
            if (victoryReached) return;

            currentWeek = _timeManager.CurrentWeek;

            // Victory condition
            if (currentWeek >= _victoryWeek && !victoryReached)
            {
                victoryReached = true;
                WinGame();
                return;
            }

            // Deck Progression
            if (currentWeek >= _endGameWeek && !endGameReached)
            {
                endGameReached = true;
                ChangeToDeck(_endGameDeck, "End Game Deck");
            }
            else if (currentWeek >= _midGameWeek && !midGameReached)
            {
                midGameReached = true;
                ChangeToDeck(_midGameDeck, "Mid Game Deck");
            }
        }

        private void ChangeToDeck(DeckSO newDeck, string phaseName)
        {
            //Debug.Log($"<color=cyan>Progression:</color> Entering {phaseName} phase!");
            
            // Only load new deck if progression system is enabled
            if (_useDecksForProgression && newDeck != null)
            {
                _deckManager.LoadNewDeck(newDeck);
            }

            // Raise the appropriate event based on the phase
            if (phaseName == "Mid Game Deck" && _onMidGameReachedEvent != null)
            {
                _onMidGameReachedEvent.Raise();
            }
            else if (phaseName == "End Game Deck" && _onEndGameReachedEvent != null)
            {
                _onEndGameReachedEvent.Raise();
            }
        }

        /// <summary>
        /// Resets all progression flags to initial state.
        /// Called when restarting the game.
        /// </summary>
        public void ResetProgression()
        {
            // Reset week counter
            currentWeek = 0;

            // Reset progression flags
            midGameReached = false;
            endGameReached = false;
            victoryReached = false;

            // Reset deck to initial if using deck progression
            if (_useDecksForProgression && _initialDeck != null && _deckManager != null)
            {
                _deckManager.LoadNewDeck(_initialDeck);
            }
        }

        private void WinGame()
        {
            //Debug.Log("<color=gold>VICTORY: The loop has been successfully completed.</color>");
            if (_onVictoryEvent != null)
            {
                _onVictoryEvent.Raise();
            }
        }
    }
}