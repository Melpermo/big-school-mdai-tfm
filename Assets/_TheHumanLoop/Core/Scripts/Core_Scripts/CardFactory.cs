using HumanLoop.Data;
using HumanLoop.UI;
using System.Collections.Generic;
using UnityEngine;

namespace HumanLoop.Core
{
    public class CardFactory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardSpawnParent;
        [SerializeField] private Visuals.CardCategorySettings categorySettings;

        private Queue<CardDisplay> _pool = new Queue<CardDisplay>();

        private void Awake()
        {
            // We pre-instantiate the 2 cards needed for the loop
            for (int i = 0; i < 2; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardSpawnParent);
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                cardObj.SetActive(false);
                _pool.Enqueue(display);
            }
        }

        /// <summary>
        /// Gets a card from the pool instead of instantiating.
        /// </summary>
        public CardDisplay GetCardFromPool(CardDataSO data)
        {
            if (_pool.Count == 0) return null;

            // Take the card that is not in use
            CardDisplay display = _pool.Dequeue();

            // Prepare it
            display.gameObject.SetActive(true);
            display.Setup(data);

            // We start as background
            display.SetAsBackground();

            if (categorySettings != null)
            {
                var style = categorySettings.GetStyle(data.category);
                display.ApplyCategoryStyle(style);
            }

            display.AnimateIn();
            return display;
        }

        public void ReturnToPool(CardDisplay display)
        {
            display.gameObject.SetActive(false);
            // Reset position for next use
            display.transform.localPosition = Vector3.zero;
            display.transform.localRotation = Quaternion.identity;

            _pool.Enqueue(display);
        }
    }
}