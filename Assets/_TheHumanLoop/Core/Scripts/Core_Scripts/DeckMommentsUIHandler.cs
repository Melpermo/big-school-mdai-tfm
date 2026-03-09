using HumanLoop.AudioSystem;
using HumanLoop.Events;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    public class DeckMommentsUIHandler : MonoBehaviour
    {

        // This class is responsible for managing the deck moments in the game.
        // It listens for specific game events that indicate when a deck moment should be triggered and
        // handles the visual and gameplay changes associated with those moments.

        // Reference to the Image component that will display the background during deck moments.
        [Header("Gameplay UI Elements")]
        [SerializeField] private Image _gamePlayBackgroundImage;

        // References to the game events that trigger the deck moments and the corresponding sprites for visual feedback.
        [Header("References to Startdeck moment")]
        [SerializeField] private GameEventSO _start_deckMommentGameEventSO;
        [SerializeField] private Sprite _start_deckMommentSprite;

        [Header("References to Middeck moment")]        
        [SerializeField] private GameEventSO _mid_deckMommentGameEventSO;
        [SerializeField] private Sprite _mid_deckMommentSprite;        
        [SerializeField] private SoundEventSO _mid_deckMommentSoundEventSO;
        [Header("References to Middeck moment")]
        [SerializeField] private GameEventSO _end_deckMommentGameEventSO;
        [SerializeField] private Sprite _end_deckMommentSprite;
        [SerializeField] private SoundEventSO _end_deckMommentSoundEventSO;     

        // This method is called to trigger a deck moment based on the provided game event.
        public void TriggerDeckMomment(GameEventSO deckMommentGameEventSO)
        {
            // Here you can implement the logic to trigger the deck moment.
            // This could include displaying an animation, changing the background, or any other visual effect.
            //Debug.Log($"Deck {deckMommentGameEventSO.name} time activated!");

            if (deckMommentGameEventSO != null)
            {
                if (deckMommentGameEventSO == _mid_deckMommentGameEventSO)
                {
                    Debug.Log("Mid Deck Moment event detected!");
                    MidDeckMoment();
                }
                else if (deckMommentGameEventSO == _end_deckMommentGameEventSO)
                {
                    Debug.Log("End Deck Moment event detected!");
                    EndDeckMoment();
                }
                else if (deckMommentGameEventSO == _start_deckMommentGameEventSO)
                { 
                    Debug.Log("Start Deck Moment event detected!");
                    StartDeckMoment();
                }
            }
        }

        private void UpdateBackgroundImage(Sprite newSprite)
        {
            
            if (_gamePlayBackgroundImage != null)
            {
                _gamePlayBackgroundImage.sprite = newSprite;
            }

            if (_mid_deckMommentSoundEventSO != null)
            {
                AudioManager.Instance.PlaySound(_mid_deckMommentSoundEventSO);
            }
        }

        private void StartDeckMoment()
        {
            //Debug.Log("Start Deck Moment triggered!");
            UpdateBackgroundImage(_start_deckMommentSprite);
        }

        private void MidDeckMoment()
        {
            //Debug.Log("Mid Deck Moment triggered!");

            UpdateBackgroundImage(_mid_deckMommentSprite);
        }

        private void EndDeckMoment()
        {
            //Debug.Log("End Deck Moment triggered!");

            UpdateBackgroundImage(_end_deckMommentSprite);

            if (_end_deckMommentSoundEventSO != null)
            {
                AudioManager.Instance.PlayMusic(_end_deckMommentSoundEventSO);
            }

        }
    }
}
