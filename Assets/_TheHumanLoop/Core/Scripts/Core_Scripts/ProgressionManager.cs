using HumanLoop.Data;
using HumanLoop.Events;
using UnityEngine;

namespace HumanLoop.Core
{
    public class ProgressionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private TimeManager timeManager;

        [Header("Progression Milestones")]
        [SerializeField] private int midGameWeek = 10;
        [SerializeField] private DeckSO midGameDeck;

        [SerializeField] private int endGameWeek = 25;
        [SerializeField] private DeckSO endGameDeck;

        [Header("Phase Victory Settings")]
        [SerializeField] private int victoryWeek = 50; // Semana para ganar
        [SerializeField] private GameEventSO onVictoryEvent;



        private bool _midGameReached = false;
        private bool _endGameReached = false;
        private bool _victoryReached = false;

        public void CheckProgression()
        {
            if (_victoryReached) return;

            int currentWeek = timeManager.CurrentWeek;

            // Condición de Victoria
            if (currentWeek >= victoryWeek && !_victoryReached)
            {
                _victoryReached = true;
                WinGame();
                return;
            }

            // Progresión de mazos
            if (currentWeek >= endGameWeek && !_endGameReached)
            {
                _endGameReached = true;
                deckManager.LoadNewDeck(endGameDeck);
                ChangeToDeck(endGameDeck, "End Game Deck");
            }
            else if (currentWeek >= midGameWeek && !_midGameReached)
            {
                _midGameReached = true;
                deckManager.LoadNewDeck(midGameDeck);
                ChangeToDeck(midGameDeck, "Mid Game Deck");
            }
        }

        private void ChangeToDeck(DeckSO newDeck, string phaseName)
        {
            Debug.Log($"<color=cyan>Progression:</color> Entering {phaseName} phase!");
            deckManager.LoadNewDeck(newDeck);
        }

        private void WinGame()
        {
            Debug.Log("<color=gold>VICTORIA: El bucle se ha completado con éxito.</color>");
            if (onVictoryEvent != null)
            {
                onVictoryEvent.Raise();
            }
        }        
    }
}