using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.UI;
using UnityEngine;

namespace HumanLoop.Core
{
    public class CardFactory : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private CardDisplay cardPrefab;

        [Header("Parents")]
        [SerializeField] private Transform spawnParent;
        [SerializeField] private Transform poolParent;

        [Header("Pool Settings")]
        [SerializeField, Min(0)] private int prewarmCount = 3;
        [SerializeField, Min(1)] private int maxPoolSize = 12;

        private readonly Stack<CardDisplay> _available = new();
        private readonly HashSet<CardDisplay> _inUse = new();

        private void Awake()
        {
            if (poolParent == null) poolParent = transform;
            if (spawnParent == null) spawnParent = transform;

            Prewarm();
        }

        private void Prewarm()
        {
            for (int i = 0; i < prewarmCount; i++)
            {
                var card = CreateInstance();
                InternalReturn(card);
            }
        }

        private CardDisplay CreateInstance()
        {
            var instance = Instantiate(cardPrefab, poolParent);
            instance.gameObject.SetActive(false);

            var controller = instance.GetComponent<CardController>();
            if (controller != null)
                controller.SetFactory(this);

            return instance;
        }



        /// <summary>
        /// Gets a card from pool, resets it, and applies its data.
        /// </summary>
        public CardDisplay GetCardFromPool(CardDataSO data)
        {
            CardDisplay card = null;

            if (_available.Count > 0)
            {
                card = _available.Pop();
            }
            else
            {
                // Empty pool: Do not grow uncontrollably (WebGL-safe)
                if (_inUse.Count < maxPoolSize)
                {
                    card = CreateInstance();
                }
                else
                {
                    // Reusing the first "in use" (hard fallback) is acceptable for MVP.
                    foreach (var inUseCard in _inUse)
                    {
                        card = inUseCard;
                        break;
                    }

                    if (card != null)
                    {
                        // We force a return before reusing it
                        InternalReturn(card);
                        _inUse.Remove(card);
                    }
                }
            }

            if (card == null) return null;

            _inUse.Add(card);

            // Activate and move to spawn
            var t = card.transform;
            t.SetParent(spawnParent, false);
            card.gameObject.SetActive(true);

            // Reset controller state (important for pooling)
            var controller = card.GetComponent<CardController>();
            if (controller != null)
                controller.ResetCardController();

            // Apply data
            card.Setup(data);

            return card;
        }

        /// <summary>
        /// Returns a card to the pool.
        /// </summary>
        public void ReturnToPool(CardDisplay display)
        {
            if (display == null) return;
            if (!_inUse.Contains(display))
            {
                // It was already out: avoid double returns
                return;
            }

            _inUse.Remove(display);
            InternalReturn(display);
        }

        private void InternalReturn(CardDisplay display)
        {
            // Deactivate and move to pool
            display.gameObject.SetActive(false);
            display.transform.SetParent(poolParent, false);

            // Mantén el pool acotado
            if (_available.Count < maxPoolSize)
                _available.Push(display);
            else
                Destroy(display.gameObject); // Keep the pool limited
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [ContextMenu("Debug/Print Pool Stats")]
        private void DebugPool()
        {
            Debug.Log($"[CardFactory] Available={_available.Count} InUse={_inUse.Count} Max={maxPoolSize}");
        }
#endif
    }
}