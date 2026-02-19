using UnityEngine;
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.UI;
using DG.Tweening;

namespace HumanLoop.Core
{
    public class DeckManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardFactory cardFactory;

        [Header("Deck Config")]
        [SerializeField] private DeckSO currentDeck;
        [SerializeField] private bool shuffleOnStart = true;
        [SerializeField] private bool infiniteLoop = true;

        private List<CardDataSO> _activeDeck;
        private CardDataSO _forcedNextCard = null;

        private CardDisplay _currentActiveCard;
        private CardDisplay _nextPendingCard;

        private void OnEnable()
        {
            CardController.OnCardRemovedWithDecision += HandleDecision;
            CardController.OnCardRemoved += SpawnNextCard;
        }

        private void OnDisable()
        {
            CardController.OnCardRemovedWithDecision -= HandleDecision;
            CardController.OnCardRemoved -= SpawnNextCard;
        }

        private void Start()
        {
            InitializeDeck();

            // 1. Preparamos la primera carta (que será la activa)
            CardDataSO firstData = GetEligibleCard();
            _currentActiveCard = cardFactory.GetCardFromPool(firstData);
            _currentActiveCard.FlipToFront(); // La primera aparece ya de frente

            // 2. Preparamos la segunda carta (que se queda asomando detrás)
            CardDataSO secondData = GetEligibleCard();
            if (secondData != null)
            {
                _nextPendingCard = cardFactory.GetCardFromPool(secondData);
                _nextPendingCard.SetAsBackground();
            }
        }

        /// <summary>
        /// Updates the current deck and clears the active list.
        /// Useful for difficulty spikes or phase changes.
        /// </summary>
        public void LoadNewDeck(DeckSO newDeck)
        {
            currentDeck = newDeck;

            // We refresh the active list with the new deck's cards
            _activeDeck = currentDeck.GetDeckCopy();

            if (shuffleOnStart) ShuffleDeck(_activeDeck);

            Debug.Log($"DeckManager: Switched to {newDeck.deckName}");
        }

        private void InitializeDeck()
        {
            if (currentDeck == null) return;
            _activeDeck = currentDeck.GetDeckCopy();
            if (shuffleOnStart) ShuffleDeck(_activeDeck);
        }

        private void HandleDecision(CardDataSO previousCard, bool isRight)
        {
            // Capturamos si la carta que acaba de irse tiene una consecuencia forzada
            _forcedNextCard = previousCard.GetForcedNextCard(isRight);
        }

        /// <summary>
        /// Lógica central: Qué carta debe salir ahora basándose en prioridad y condiciones.
        /// </summary>
        private CardDataSO GetEligibleCard()
        {
            CardDataSO selected = null;

            // PRIORIDAD 1: ¿Hay una carta forzada por la decisión anterior?
            if (_forcedNextCard != null)
            {
                selected = _forcedNextCard;
                _forcedNextCard = null; // Consumimos la prioridad
            }
            else
            {
                // PRIORIDAD 2: Buscar por condiciones en el mazo activo
                var stats = GameStatsManager.Instance;
                selected = _activeDeck.Find(card => card.CanSpawn(stats.budget, stats.time, stats.morale, stats.quality));
            }

            // Gestión del mazo (remover y reinsertar si es loop)
            if (selected != null && _activeDeck.Contains(selected))
            {
                _activeDeck.Remove(selected);
                if (infiniteLoop) _activeDeck.Add(selected);
            }

            return selected;
        }

        public void SpawnNextCard()
        {
            // La que estaba "asomando" pasa a ser la activa
            _currentActiveCard = _nextPendingCard;

            DOVirtual.DelayedCall(0.4f, () =>
            {
                // 1. Giramos la carta que ya estaba en escena
                if (_currentActiveCard != null)
                {
                    _currentActiveCard.FlipToFront();
                }

                // 2. Pedimos al pool la NUEVA carta que se quedará asomando detrás
                CardDataSO nextData = GetEligibleCard();
                if (nextData != null)
                {
                    _nextPendingCard = cardFactory.GetCardFromPool(nextData);
                    _nextPendingCard.SetAsBackground();
                }
            });
        }

        private void ShuffleDeck(List<CardDataSO> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                CardDataSO temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}