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

        private Tween _spawnDelayTween;

        private void OnEnable()
        {
            CardController.OnCardRemovedWithDecision += HandleDecision;
            CardController.OnCardRemoved += SpawnNextCard;
        }

        private void OnDisable()
        {
            CardController.OnCardRemovedWithDecision -= HandleDecision;
            CardController.OnCardRemoved -= SpawnNextCard;

            _spawnDelayTween?.Kill(true);
            _spawnDelayTween = null;
            DOTween.Kill(this); // kill tweens targeted to the DeckManager
        }

        private void OnDestroy()
        {
            CardController.OnCardRemovedWithDecision -= HandleDecision;
            CardController.OnCardRemoved -= SpawnNextCard;
        }

        private void Start()
        {
            InitializeDeck();

            // 1. We prepare the first card (which will be the active one)
            CardDataSO firstData = GetEligibleCard();
            _currentActiveCard = cardFactory.GetCardFromPool(firstData);
            _currentActiveCard.FlipToFront(); // The first one appears already facing forwards

            // 2. We prepared the second card (which remains peeking out from behind)
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

            //Debug.Log($"DeckManager: Switched to {newDeck.deckName}");
        }

        private void InitializeDeck()
        {
            if (currentDeck == null) return;
            _activeDeck = currentDeck.GetDeckCopy();
            if (shuffleOnStart) ShuffleDeck(_activeDeck);
        }

        private void HandleDecision(CardDataSO previousCard, bool isRight)
        {
            // We capture if the card that just left has a forced consequence.
            _forcedNextCard = previousCard.GetForcedNextCard(isRight);
        }

        /// <summary>
        /// Central logic: Which card should come out now based on priority and conditions.
        /// </summary>
        private CardDataSO GetEligibleCard()
        {
            CardDataSO selected = null;

            // PRIORITY 1: Is there a letter forced by the previous decision?
            if (_forcedNextCard != null)
            {
                selected = _forcedNextCard;
                _forcedNextCard = null; // We consumed the priority.
            }
            else
            {
                // PRIORITY 2: Search by conditions in the active deck
                var stats = GameStatsManager.Instance;
                selected = _activeDeck.Find(card => card.CanSpawn(stats.budget, stats.time, stats.morale, stats.quality));
            }

            // Deck management (remove and reinsert if it's a loop)
            if (selected != null && _activeDeck.Contains(selected))
            {
                _activeDeck.Remove(selected);
                if (infiniteLoop) _activeDeck.Add(selected);
            }

            return selected;
        }

        public void SpawnNextCard()
        {
            _spawnDelayTween?.Kill(true);

            _currentActiveCard = _nextPendingCard;

            _spawnDelayTween = DOVirtual.DelayedCall(0.4f, () =>
            {
                if (_currentActiveCard != null)
                    _currentActiveCard.FlipToFront();

                var nextData = GetEligibleCard();
                if (nextData != null)
                {
                    _nextPendingCard = cardFactory.GetCardFromPool(nextData);
                    _nextPendingCard.SetAsBackground();
                }
            }).SetTarget(this);
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