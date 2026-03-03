using UnityEngine;
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.UI;
using DG.Tweening;

namespace HumanLoop.Core
{
    /// <summary>
    /// Manages the card deck flow, spawning, and cycling.
    /// Optimized for WebGL with proper state management and refactored for new CardDisplay/CardController architecture.
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardFactory cardFactory;

        [Header("Deck Config")]
        [SerializeField] private DeckSO currentDeck;
        [SerializeField] private bool shuffleOnStart = true;
        [SerializeField] private bool infiniteLoop = true;

        [Header("Spawn Settings")]
        [Tooltip("Delay before flipping next card (seconds)")]
        [SerializeField] private float spawnDelay = 0.4f;

        [Header("Debug")]
        [SerializeField] private bool logSpawnEvents = false;

        // Deck state
        private List<CardDataSO> _activeDeck;
        private CardDataSO _forcedNextCard = null;

        // Card references
        private CardDisplay _currentActiveCard;
        private CardDisplay _nextPendingCard;

        // Tween management
        private Tween _spawnDelayTween;

        // State tracking
        private bool _isSpawning = false;

        // Card counter
        [SerializeField] private int _remainingCardsCount = 0;
        public int RemainingCardsCount => _remainingCardsCount;
        [SerializeField] private int _cardsPlayedCount = 0;
        public int CardsPlayedCount => _cardsPlayedCount;

        #region Unity Lifecycle

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            CleanupTweens();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            CleanupTweens();
        }

        private void Start()
        {
            InitializeDeck();
            SpawnInitialCards();
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            CardController.OnCardRemovedWithDecision += HandleDecision;
            CardController.OnCardRemoved += OnCardRemovedCallback;
        }

        private void UnsubscribeFromEvents()
        {
            CardController.OnCardRemovedWithDecision -= HandleDecision;
            CardController.OnCardRemoved -= OnCardRemovedCallback;
        }

        #endregion

        #region Initialization

        private void InitializeDeck()
        {
            if (currentDeck == null)
            {
                Debug.LogError("[DeckManager] currentDeck is null! Assign a DeckSO.");
                return;
            }

            _activeDeck = currentDeck.GetDeckCopy();

            if (shuffleOnStart)
            {
                ShuffleDeck(_activeDeck);
            }

            // NUEVO: Inicializar contador de cartas restantes
            _remainingCardsCount = _activeDeck.Count;
            _cardsPlayedCount = 0;

            if (logSpawnEvents)
            {
                Debug.Log($"[DeckManager] Initialized with {_activeDeck.Count} cards (Remaining: {_remainingCardsCount})");
            }
        }

        private void SpawnInitialCards()
        {
            // Spawn first card (active, facing forward)
            CardDataSO firstData = GetEligibleCard();
            if (firstData != null)
            {
                _currentActiveCard = cardFactory.GetCardFromPool(firstData);
                if (_currentActiveCard != null && _currentActiveCard.Controller != null)
                {
                    _currentActiveCard.Controller.FlipToFront(); // ← Ahora desde Controller
                    _currentActiveCard.Controller.AnimateIn();   // ← Ahora desde Controller

                    if (logSpawnEvents)
                    {
                        Debug.Log($"[DeckManager] Spawned first card: {firstData.cardName}");
                    }
                }
                else
                {
                    Debug.LogError($"[DeckManager] Failed to get controller for first card: {firstData.cardName}");
                }
            }
            else
            {
                Debug.LogError("[DeckManager] No eligible card for first spawn!");
            }

            // Spawn second card (background, waiting)
            CardDataSO secondData = GetEligibleCard();
            if (secondData != null)
            {
                _nextPendingCard = cardFactory.GetCardFromPool(secondData);
                if (_nextPendingCard != null && _nextPendingCard.Controller != null)
                {
                    _nextPendingCard.Controller.SetAsBackground(); // ← Ahora desde Controller

                    if (logSpawnEvents)
                    {
                        Debug.Log($"[DeckManager] Prepared second card: {secondData.cardName}");
                    }
                }
                else
                {
                    Debug.LogError($"[DeckManager] Failed to get controller for second card: {secondData.cardName}");
                }
            }
        }

        #endregion

        #region Deck Management

        /// <summary>
        /// Updates the current deck and resets the active list.
        /// Useful for difficulty spikes or phase changes.
        /// </summary>
        public void LoadNewDeck(DeckSO newDeck)
        {
            if (newDeck == null)
            {
                Debug.LogWarning("[DeckManager] Attempted to load null deck");
                return;
            }

            currentDeck = newDeck;
            _activeDeck = currentDeck.GetDeckCopy();

            if (shuffleOnStart)
            {
                ShuffleDeck(_activeDeck);
            }

            if (logSpawnEvents)
            {
                Debug.Log($"[DeckManager] Switched to deck: {newDeck.deckName}");
            }
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

        #endregion

        #region Card Selection

        private void HandleDecision(CardDataSO previousCard, bool isRight)
        {
            // Capture forced consequence from previous card
            _forcedNextCard = previousCard?.GetForcedNextCard(isRight);

            if (logSpawnEvents && _forcedNextCard != null)
            {
                Debug.Log($"[DeckManager] Forced next card: {_forcedNextCard.cardName}");
            }
        }

        /// <summary>
        /// Gets the next eligible card based on priority:
        /// 1. Forced card from previous decision
        /// 2. Card that meets spawn conditions
        /// 3. First available card (fallback)
        /// </summary>
        private CardDataSO GetEligibleCard()
        {
            CardDataSO selected = null;

            // PRIORITY 1: Forced card from previous decision
            if (_forcedNextCard != null)
            {
                selected = _forcedNextCard;
                _forcedNextCard = null;
            }
            else
            {
                // PRIORITY 2: Card that meets spawn conditions
                var stats = GameStatsManager.Instance;
                if (stats != null)
                {
                    selected = _activeDeck.Find(card =>
                        card.CanSpawn(stats.budget, stats.time, stats.morale, stats.quality)
                    );
                }

                // FALLBACK: First card if no conditions matched
                if (selected == null && _activeDeck.Count > 0)
                {
                    selected = _activeDeck[0];
                }
            }

            // Manage deck cycling
            if (selected != null && _activeDeck.Contains(selected))
            {
                _activeDeck.Remove(selected);

                if (infiniteLoop)
                {
                    _activeDeck.Add(selected);
                }
            }

            return selected;
        }

        #endregion

        #region Card Spawning

        private void OnCardRemovedCallback()
        {
            if (logSpawnEvents)
            {
                Debug.Log("[DeckManager] Card removed, spawning next...");
            }

            SpawnNextCard();
        }

        public void SpawnNextCard()
        {
            if (_isSpawning)
            {
                if (logSpawnEvents)
                {
                    Debug.LogWarning("[DeckManager] Already spawning, skipping");
                }
                return;
            }

            _isSpawning = true;

            // Kill previous delayed call
            CleanupTweens();

            // Move pending card to active (but don't flip yet)
            CardDisplay previousActive = _currentActiveCard;
            _currentActiveCard = _nextPendingCard;
            _nextPendingCard = null;

            if (logSpawnEvents)
            {
                Debug.Log($"[DeckManager] Moved to active: {_currentActiveCard?.Data?.cardName ?? "NULL"}");
            }

            // Delayed flip and spawn
            _spawnDelayTween = DOVirtual.DelayedCall(spawnDelay, () =>
            {
                OnSpawnDelayComplete(previousActive);
            })
            .SetTarget(this)
            .SetUpdate(true) // Works even if Time.timeScale = 0
            .SetAutoKill(true);
        }

        private void OnSpawnDelayComplete(CardDisplay previousCard)
        {
            // Flip current active card to front
            if (_currentActiveCard != null && _currentActiveCard.Controller != null)
            {
                _currentActiveCard.Controller.FlipToFront(); // ← Ahora desde Controller

                if (logSpawnEvents)
                {
                    Debug.Log($"[DeckManager] Flipped to front: {_currentActiveCard.Data?.cardName}");
                }
            }
            else
            {
                Debug.LogWarning($"[DeckManager] Cannot flip card - Controller is null!");
            }

            // Get next card data
            CardDataSO nextData = GetEligibleCard();
            if (nextData != null)
            {
                _nextPendingCard = cardFactory.GetCardFromPool(nextData);

                if (_nextPendingCard != null && _nextPendingCard.Controller != null)
                {
                    _nextPendingCard.Controller.SetAsBackground(); // ← Ahora desde Controller

                    if (logSpawnEvents)
                    {
                        Debug.Log($"[DeckManager] Prepared next card: {nextData.cardName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DeckManager] Failed to get card from pool for: {nextData.cardName}");
                }
            }
            else
            {
                Debug.LogWarning("[DeckManager] No eligible cards available!");
            }

            _spawnDelayTween = null;
            _isSpawning = false;
        }

        /// <summary>
        /// Called when a card is played. Decrements the remaining cards counter.
        /// This method should be invoked by a GameEventListener when a card is removed.
        /// </summary>
        public void OnCardPlayed()
        {
            if (!infiniteLoop)
            {
                _remainingCardsCount--;
                _cardsPlayedCount++;

                if (logSpawnEvents)
                {
                    Debug.Log($"[DeckManager] Card played. Remaining cards: {_remainingCardsCount}");
                    Debug.Log($"[DeckManager] Total cards played: {_cardsPlayedCount}");
                }

                // Optional: Check if deck is exhausted
                if (_remainingCardsCount <= 0)
                {
                    Debug.LogWarning("[DeckManager] Deck exhausted! No more cards available.");
                }


                // Optional: Check if deck is low (e.g., less than 3 cards remaining)
                if (_remainingCardsCount <= 3)
                {
                    Debug.LogWarning($"[DeckManager] Deck is low! Only {_remainingCardsCount} cards remaining.");
                }

                // Optional: Check if all cards have been played at least once (if not infinite loop)
                if (!infiniteLoop && _cardsPlayedCount >= currentDeck.cards.Count)
                {
                    Debug.Log($"[DeckManager] All cards have been played at least once.");
                }
            }
            // Note: If infiniteLoop is true, we don't decrement because cards are recycled
        }

        #endregion

        #region Reset & Cleanup

        /// <summary>
        /// Resets the deck to initial state (called when restarting game or starting from menu).
        /// Returns any active cards to pool and reinitializes deck.
        /// </summary>
        public void ResetDeck()
        {
            if (logSpawnEvents)
            {
                Debug.Log("[DeckManager] Resetting deck to initial state...");
            }

            // 1. Stop any pending spawn delay
            CleanupTweens();

            // 2. Return current cards to pool
            if (_currentActiveCard != null && cardFactory != null)
            {
                cardFactory.ReturnToPool(_currentActiveCard);
                _currentActiveCard = null;
            }

            if (_nextPendingCard != null && cardFactory != null)
            {
                cardFactory.ReturnToPool(_nextPendingCard);
                _nextPendingCard = null;
            }

            // 3. Clear forced next card
            _forcedNextCard = null;

            // 4. Reset spawning flag
            _isSpawning = false;

            // 5. Reinitialize deck (reload and reshuffle)
            InitializeDeck();

            // 6. Spawn initial cards again
            SpawnInitialCards();

            if (logSpawnEvents)
            {
                Debug.Log($"[DeckManager] Deck reset complete. Total cards: {_remainingCardsCount}");
            }
        }

        #endregion

        #region Cleanup

        private void CleanupTweens()
        {
            if (_spawnDelayTween != null && _spawnDelayTween.IsActive())
            {
                _spawnDelayTween.Kill(complete: false);
                _spawnDelayTween = null;
            }

            // Kill all tweens targeting this DeckManager
            DOTween.Kill(this, complete: false);
        }

        #endregion

        #region Debug & Testing

#if UNITY_EDITOR
        [ContextMenu("Debug/Log Deck State")]
        private void DebugLogDeckState()
        {
            Debug.Log($"=== DECK MANAGER STATE ===\n" +
                     $"Active Deck Cards: {_activeDeck?.Count ?? 0}\n" +
                     $"Current Active Card: {_currentActiveCard?.Data?.cardName ?? "NULL"}\n" +
                     $"Current Active Controller: {(_currentActiveCard?.Controller != null ? "OK" : "NULL")}\n" +
                     $"Next Pending Card: {_nextPendingCard?.Data?.cardName ?? "NULL"}\n" +
                     $"Next Pending Controller: {(_nextPendingCard?.Controller != null ? "OK" : "NULL")}\n" +
                     $"Is Spawning: {_isSpawning}\n" +
                     $"Forced Next: {_forcedNextCard?.cardName ?? "NONE"}");
        }

        [ContextMenu("Debug/Force Spawn Next")]
        private void DebugForceSpawnNext()
        {
            SpawnNextCard();
        }

        [ContextMenu("Debug/Check Controllers")]
        private void DebugCheckControllers()
        {
            if (_currentActiveCard != null)
            {
                bool hasController = _currentActiveCard.Controller != null;
                Debug.Log($"Current Active Card Controller: {(hasController ? "✓ EXISTS" : "✗ NULL")}");
            }
            else
            {
                Debug.Log("Current Active Card: NULL");
            }

            if (_nextPendingCard != null)
            {
                bool hasController = _nextPendingCard.Controller != null;
                Debug.Log($"Next Pending Card Controller: {(hasController ? "✓ EXISTS" : "✗ NULL")}");
            }
            else
            {
                Debug.Log("Next Pending Card: NULL");
            }
        }

        [ContextMenu("Test/Simulate 10 Card Swipes")]
        private void TestSimulateSwipes()
        {
            StartCoroutine(TestSimulateSwipesCoroutine());
        }

        private System.Collections.IEnumerator TestSimulateSwipesCoroutine()
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"=== Simulating swipe {i + 1}/10 ===");

                // Simulate card removal
                SpawnNextCard();

                yield return new UnityEngine.WaitForSeconds(1f);

                DebugLogDeckState();
            }

            Debug.Log("<color=green>✓ Swipe simulation complete</color>");
        }

        [ContextMenu("Debug/Log Remaining Cards")]
        private void DebugLogRemainingCards()
        {
            Debug.Log($"[DeckManager] Remaining Cards: {_remainingCardsCount} / {_activeDeck?.Count ?? 0}");
        }

        [ContextMenu("Test/Simulate Card Played")]
        private void DebugSimulateCardPlayed()
        {
            OnCardPlayed();
            DebugLogRemainingCards();
        }
#endif

        #endregion
    }
}