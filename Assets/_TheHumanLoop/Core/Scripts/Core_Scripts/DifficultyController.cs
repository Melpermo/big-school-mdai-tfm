using HumanLoop.Data;
using UnityEngine;

namespace HumanLoop.Core
{
    public class DifficultyController : MonoBehaviour
    {
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private DeckSO easyDeck;
        [SerializeField] private DeckSO hardDeck;

        public void SetEasyMode() => deckManager.LoadNewDeck(easyDeck);
        public void SetHardMode() => deckManager.LoadNewDeck(hardDeck);
    }
}
