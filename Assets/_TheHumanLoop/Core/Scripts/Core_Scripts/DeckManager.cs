using UnityEngine;
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.UI;
using DG.Tweening;

namespace HumanLoop.Core
{
    /// <summary>
    /// Manages the card deck flow, spawning, and cycling.
    /// Optimized for WebGL with proper state management and localization support.
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardFactory cardFactory;

        [Header("Deck Config")]
        [Tooltip("If true, loads deck from DeckLocalizationManager. If false, uses assigned deck.")]
        [SerializeField] private bool useLocalizedDecks = true;

        [Tooltip("Only used if useLocalizedDecks is false")]
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

        private void Start()
        {
            // Subscribe to language changes
            if (useLocalizedDecks)
            {
                LocalizationSystem.LanguageManager.OnLanguageChanged += OnLanguageChanged;
            }

            InitializeDeck();
            SpawnInitialCards();
        }

        private void OnDestroy()
        {
            if (useLocalizedDecks)
            {
                LocalizationSystem.LanguageManager.OnLanguageChanged -= OnLanguageChanged;
            }

            UnsubscribeFromEvents();
            CleanupTweens();
        }

        #endregion

        #region Localization Integration

        private void OnLanguageChanged(LocalizationSystem.LanguageManager.Language newLanguage)
        {
            if (logSpawnEvents)
            {
                Debug.Log($"[DeckManager] Language changed to {newLanguage}, reloading deck...");
            }

            // Get the new localized deck
            DeckSO newDeck = GetCurrentLocalizedDeck();

            if (newDeck != null)
            {
                LoadNewDeck(newDeck);
            }
        }

        private DeckSO GetCurrentLocalizedDeck()
        {
            if (!useLocalizedDecks)
                return currentDeck;

            if (LocalizationSystem.DeckLocalizationManager.Instance == null)
            {
                Debug.LogError("[DeckManager] DeckLocalizationManager.Instance is null!");
                return null;
            }

            // Return EarlyPhase by default (ProgressionManager will update this)
            return LocalizationSystem.DeckLocalizationManager.Instance.EarlyPhaseDeck;
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
            // Get deck based on localization setting
            if (useLocalizedDecks)
            {
                currentDeck = GetCurrentLocalizedDeck();
            }

            if (currentDeck == null)
            {
                Debug.LogError("[DeckManager] currentDeck is null! Assign a DeckSO or enable useLocalizedDecks.");
                return;
            }

            _activeDeck = currentDeck.GetDeckCopy();

            if (shuffleOnStart)
            {
                ShuffleDeck(_activeDeck);
            }

            // Initialize card counters
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
                    _currentActiveCard.Controller.FlipToFront();
                    _currentActiveCard.Controller.AnimateIn();

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
                    _nextPendingCard.Controller.SetAsBackground();

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
        /// Useful for difficulty spikes, phase changes, or language changes.
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
            .SetUpdate(true)
            .SetAutoKill(true);
        }

        private void OnSpawnDelayComplete(CardDisplay previousCard)
        {
            // Flip current active card to front
            if (_currentActiveCard != null && _currentActiveCard.Controller != null)
            {
                _currentActiveCard.Controller.FlipToFront();

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
                    _nextPendingCard.Controller.SetAsBackground();

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
        /// </summary>
        public void OnCardPlayed()
        {
            if (!infiniteLoop)
            {
                _remainingCardsCount--;
                _cardsPlayedCount++;

                if (logSpawnEvents)
                {
                    Debug.Log($"[DeckManager] Card played. Remaining: {_remainingCardsCount}, Total played: {_cardsPlayedCount}");
                }

                if (_remainingCardsCount <= 0)
                {
                    Debug.LogWarning("[DeckManager] Deck exhausted! No more cards available.");
                }
            }
        }

        #endregion

        #region Reset & Cleanup

        /// <summary>
        /// Resets the deck to initial state.
        /// </summary>
        public void ResetDeck()
        {
            if (logSpawnEvents)
            {
                Debug.Log("[DeckManager] Resetting deck to initial state...");
            }

            CleanupTweens();

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

            _forcedNextCard = null;
            _isSpawning = false;

            InitializeDeck();
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

            DOTween.Kill(this, complete: false);
        }

        #endregion

        #region Debug & Testing

#if UNITY_EDITOR
        [ContextMenu("Debug/Log Deck State")]
        private void DebugLogDeckState()
        {
            string currentLanguage = useLocalizedDecks && LocalizationSystem.LanguageManager.Instance != null
                ? LocalizationSystem.LanguageManager.Instance.CurrentLanguage.ToString()
                : "N/A";

            Debug.Log($"=== DECK MANAGER STATE ===\n" +
                     $"Use Localized Decks: {useLocalizedDecks}\n" +
                     $"Current Language: {currentLanguage}\n" +
                     $"Current Deck: {currentDeck?.deckName ?? "NULL"}\n" +
                     $"Active Deck Cards: {_activeDeck?.Count ?? 0}\n" +
                     $"Current Active Card: {_currentActiveCard?.Data?.cardName ?? "NULL"}\n" +
                     $"Next Pending Card: {_nextPendingCard?.Data?.cardName ?? "NULL"}\n" +
                     $"Is Spawning: {_isSpawning}");
        }

        [ContextMenu("Debug/Force Reload Localized Deck")]
        private void DebugForceReloadLocalizedDeck()
        {
            if (!useLocalizedDecks)
            {
                Debug.LogWarning("[DeckManager] useLocalizedDecks is false!");
                return;
            }

            DeckSO newDeck = GetCurrentLocalizedDeck();
            if (newDeck != null)
            {
                LoadNewDeck(newDeck);
                Debug.Log($"[DeckManager] Force reloaded: {newDeck.deckName}");
            }
        }
#endif

        #endregion
    }
}