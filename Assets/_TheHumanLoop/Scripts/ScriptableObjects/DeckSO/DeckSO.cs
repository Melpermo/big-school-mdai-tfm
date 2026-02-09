using UnityEngine;
using System.Collections.Generic;
using HumanLoop.Data;

namespace HumanLoop.Data
{
    [CreateAssetMenu(fileName = "NewDeck", menuName = "The Human Loop/Cards/DeckConfiguration")]
    public class DeckSO : ScriptableObject
    {
        public string deckName;
        [TextArea(2, 4)] public string description;

        [Header("Cards in this Deck")]
        public List<CardDataSO> cards;

        /// <summary>
        /// Returns a copy of the cards list to avoid modifying the SO asset during play.
        /// </summary>
        public List<CardDataSO> GetDeckCopy()
        {
            return new List<CardDataSO>(cards);
        }
    }
}