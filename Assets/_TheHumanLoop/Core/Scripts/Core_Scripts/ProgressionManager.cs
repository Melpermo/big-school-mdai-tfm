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



        private bool midGameReached = false;
        private bool endGameReached = false;
        private bool victoryReached = false;

        public void CheckProgression()
        {
            if (victoryReached) return;

            int currentWeek = _timeManager.CurrentWeek;

            // Condición de Victoria
            if (currentWeek >= _victoryWeek && !victoryReached)
            {
                victoryReached = true;
                WinGame();
                return;
            }

            // Progresión de mazos
            if (currentWeek >= _endGameWeek && !endGameReached)
            {
                endGameReached = true;
                _deckManager.LoadNewDeck(_endGameDeck);
                ChangeToDeck(_endGameDeck, "End Game Deck");

            }
            else if (currentWeek >= _midGameWeek && !midGameReached)
            {
                midGameReached = true;
                _deckManager.LoadNewDeck(_midGameDeck);
                ChangeToDeck(_midGameDeck, "Mid Game Deck");
            }
        }

        private void ChangeToDeck(DeckSO newDeck, string phaseName)
        {
            //Debug.Log($"<color=cyan>Progression:</color> Entering {phaseName} phase!");
            _deckManager.LoadNewDeck(newDeck);

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