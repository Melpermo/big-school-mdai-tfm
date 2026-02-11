using UnityEngine;
using HumanLoop.UI;
using HumanLoop.AudioSystem;
using HumanLoop.Events;

namespace HumanLoop.AudioSystem
{

    public class GameplayAudioManager : MonoBehaviour
    {
        [Header("Card SoundEventSO elements")]
        
        [SerializeField] private SoundEventSO cardAddedSoundEvent;
        [SerializeField] private SoundEventSO cardFlipSoundEvent;
        [SerializeField] private SoundEventSO cardReturnedSoundEvent;
        [SerializeField] private SoundEventSO cardSwipedSoundEvent;        
        

        // Subscribe to card-related events
        public void HandleCardReturnedSounds()
        {
            // Play the card returned sound effect
            AudioManager.Instance.PlaySound(cardReturnedSoundEvent);
        }

        public void HandleCardSwipedSounds()
        {
            // Play the card swiped sound effect
            AudioManager.Instance.PlaySound(cardSwipedSoundEvent);
        }        

        public void HandleCardFlipSounds()
        {
            // Play the card flip sound effect
            AudioManager.Instance.PlaySound(cardFlipSoundEvent);
        }

        public void HandleCardAddedSounds()
        {
            // Play the card added sound effect
            AudioManager.Instance.PlaySound(cardAddedSoundEvent);
            
        }        
    }
}
