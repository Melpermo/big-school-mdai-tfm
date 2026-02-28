using System;
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.UI;
using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Object pool for CardDisplay instances.
    /// Optimized for WebGL with bounded pool size and proper cleanup.
    /// </summary>
    public class CardFactory : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private CardDisplay cardPrefab;

        [Header("Parents")]
        [SerializeField] private Transform spawnParent;
        [SerializeField] private Transform poolParent;

        [Header("Pool Settings")]
        [Tooltip("Number of cards to create at startup")]
        [SerializeField, Min(0)] private int prewarmCount = 3;

        [Tooltip("Maximum cards that can exist (prevents memory issues in WebGL)")]
        [SerializeField, Min(1)] private int maxPoolSize = 12;

        [Header("Debug")]
        [SerializeField] private bool logPoolOperations = false;

        // Pool collections
        private readonly Stack<CardDisplay> _available = new Stack<CardDisplay>();
        private readonly HashSet<CardDisplay> _inUse = new HashSet<CardDisplay>();

        // Performance tracking
        private int _totalCreated = 0;
        private int _peakInUse = 0;

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateConfiguration();
            Prewarm();
        }

        private void OnDestroy()
        {
            CleanupPool();
        }

        #endregion

        #region Initialization

        private void ValidateConfiguration()
        {
            if (poolParent == null)
            {
                poolParent = transform;
                Debug.LogWarning("[CardFactory] poolParent not set, using self.");
            }

            if (spawnParent == null)
            {
                spawnParent = transform;
                Debug.LogWarning("[CardFactory] spawnParent not set, using self.");
            }

            if (cardPrefab == null)
            {
                Debug.LogError("[CardFactory] cardPrefab is null! Cannot create cards.");
            }

            if (prewarmCount > maxPoolSize)
            {
                Debug.LogWarning($"[CardFactory] prewarmCount ({prewarmCount}) > maxPoolSize ({maxPoolSize}). Adjusting to maxPoolSize.");
                prewarmCount = maxPoolSize;
            }
        }

        private void Prewarm()
        {
            if (cardPrefab == null) return;

            for (int i = 0; i < prewarmCount; i++)
            {
                CardDisplay card = CreateNewInstance();
                if (card != null)
                {
                    InternalReturnToPool(card);
                }
            }

            if (logPoolOperations)
            {
                Debug.Log($"[CardFactory] Prewarmed {prewarmCount} cards");
            }
        }

        #endregion

        #region Card Creation

        private CardDisplay CreateNewInstance()
        {
            if (cardPrefab == null)
            {
                Debug.LogError("[CardFactory] Cannot create card: prefab is null");
                return null;
            }

            // Instantiate and setup
            CardDisplay instance = Instantiate(cardPrefab, poolParent);
            instance.gameObject.SetActive(false);

            // Cache controller reference and set factory
            CardController controller = instance.GetComponent<CardController>();
            if (controller != null)
            {
                controller.SetFactory(this);
            }
            else
            {
                Debug.LogWarning("[CardFactory] CardController not found on prefab!");
            }

            _totalCreated++;

            if (logPoolOperations)
            {
                Debug.Log($"[CardFactory] Created new instance (Total: {_totalCreated})");
            }

            return instance;
        }

        #endregion

        #region Pool Operations

        /// <summary>
        /// Gets a card from the pool and initializes it with data.
        /// Returns null if pool is exhausted and maxPoolSize is reached.
        /// </summary>
        public CardDisplay GetCardFromPool(CardDataSO data)
        {
            if (data == null)
            {
                Debug.LogWarning("[CardFactory] Cannot get card: data is null");
                return null;
            }

            CardDisplay card = null;

            // Try to get from available pool
            if (_available.Count > 0)
            {
                card = _available.Pop();
            }
            else if (_inUse.Count < maxPoolSize)
            {
                // Pool exhausted but under limit: create new
                card = CreateNewInstance();
            }
            else
            {
                // Pool exhausted and at limit: cannot create more
                Debug.LogWarning($"[CardFactory] Pool exhausted! Max size reached ({maxPoolSize}). " +
                                "Consider increasing maxPoolSize or reducing card usage.");
                return null;
            }

            if (card == null)
            {
                Debug.LogError("[CardFactory] Failed to get card from pool");
                return null;
            }

            // Mark as in use
            _inUse.Add(card);
            UpdatePeakUsage();

            // Activate and setup
            ActivateCard(card);
            ResetCard(card);
            card.Setup(data);

            if (logPoolOperations)
            {
                Debug.Log($"[CardFactory] Card retrieved. InUse: {_inUse.Count}, Available: {_available.Count}");
            }

            return card;
        }

        /// <summary>
        /// Returns a card to the pool for reuse.
        /// </summary>
        public void ReturnToPool(CardDisplay display)
        {
            if (display == null)
            {
                Debug.LogWarning("[CardFactory] Attempted to return null card");
                return;
            }

            if (!_inUse.Contains(display))
            {
                Debug.LogWarning("[CardFactory] Attempted to return card that wasn't in use");
                return;
            }

            _inUse.Remove(display);
            InternalReturnToPool(display);

            if (logPoolOperations)
            {
                Debug.Log($"[CardFactory] Card returned. InUse: {_inUse.Count}, Available: {_available.Count}");
            }
        }

        private void InternalReturnToPool(CardDisplay display)
        {
            // Deactivate and move to pool parent
            display.gameObject.SetActive(false);
            display.transform.SetParent(poolParent, false);

            // Add to available stack if under max size
            if (_available.Count < maxPoolSize)
            {
                _available.Push(display);
            }
            else
            {
                // Pool is full: destroy excess
                if (logPoolOperations)
                {
                    Debug.Log($"[CardFactory] Pool full, destroying excess card");
                }
                Destroy(display.gameObject);
                _totalCreated--;
            }
        }

        #endregion

        #region Card Management

        private void ActivateCard(CardDisplay card)
        {
            Transform cardTransform = card.transform;
            cardTransform.SetParent(spawnParent, false);
            card.gameObject.SetActive(true);
        }

        private void ResetCard(CardDisplay card)
        {
            CardController controller = card.GetComponent<CardController>();
            if (controller != null)
            {
                controller.ResetCardController();
            }
        }

        #endregion

        #region Pool Cleanup

        /// <summary>
        /// Destroys all pooled cards and clears collections.
        /// Call this when changing scenes or shutting down.
        /// </summary>
        public void CleanupPool()
        {
            // Destroy all available cards
            while (_available.Count > 0)
            {
                CardDisplay card = _available.Pop();
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            // Destroy all in-use cards
            foreach (CardDisplay card in _inUse)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            _inUse.Clear();
            _totalCreated = 0;
            _peakInUse = 0;

            if (logPoolOperations)
            {
                Debug.Log("[CardFactory] Pool cleaned up");
            }
        }

        #endregion

        #region Statistics

        private void UpdatePeakUsage()
        {
            if (_inUse.Count > _peakInUse)
            {
                _peakInUse = _inUse.Count;
            }
        }

        public int GetAvailableCount() => _available.Count;
        public int GetInUseCount() => _inUse.Count;
        public int GetTotalCreated() => _totalCreated;
        public int GetPeakUsage() => _peakInUse;

        #endregion

        #region Debug & Testing

#if UNITY_EDITOR
        [ContextMenu("Debug/Print Pool Stats")]
        private void DebugPrintPoolStats()
        {
            Debug.Log($"=== CardFactory Stats ===\n" +
                     $"Available: {_available.Count}\n" +
                     $"In Use: {_inUse.Count}\n" +
                     $"Total Created: {_totalCreated}\n" +
                     $"Peak Usage: {_peakInUse}\n" +
                     $"Max Pool Size: {maxPoolSize}");
        }

        [ContextMenu("Debug/Force Cleanup Pool")]
        private void DebugForceCleanup()
        {
            CleanupPool();
            Debug.Log("[CardFactory] Pool forcefully cleaned");
        }

        [ContextMenu("Test/Create and Return 10 Cards")]
        private void DebugTestCreateReturn()
        {
            StartCoroutine(TestCreateReturnCoroutine());
        }

        private System.Collections.IEnumerator TestCreateReturnCoroutine()
        {
            // Necesitarías un CardDataSO de prueba
            CardDataSO testData = Resources.Load<CardDataSO>("TestCard");
            if (testData == null)
            {
                Debug.LogError("[CardFactory] No test card data found at Resources/TestCard");
                yield break;
            }

            List<CardDisplay> cards = new List<CardDisplay>();

            // Create 10 cards
            for (int i = 0; i < 10; i++)
            {
                CardDisplay card = GetCardFromPool(testData);
                if (card != null)
                {
                    cards.Add(card);
                }
                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log($"[CardFactory] Created {cards.Count} cards");
            DebugPrintPoolStats();

            yield return new WaitForSeconds(1f);

            // Return all cards
            foreach (CardDisplay card in cards)
            {
                ReturnToPool(card);
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[CardFactory] Returned all cards");
            DebugPrintPoolStats();
        }

        private void OnValidate()
        {
            // Ensure sensible values in editor
            prewarmCount = Mathf.Max(0, prewarmCount);
            maxPoolSize = Mathf.Max(1, maxPoolSize);

            if (prewarmCount > maxPoolSize)
            {
                prewarmCount = maxPoolSize;
            }
        }

        internal CardDisplay GetCardFromPool(object testCardData)
        {
            throw new NotImplementedException();
        }
#endif

        #endregion
    }
}