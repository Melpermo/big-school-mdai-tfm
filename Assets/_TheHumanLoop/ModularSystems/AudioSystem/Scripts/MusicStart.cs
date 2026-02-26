using UnityEngine;

namespace HumanLoop.AudioSystem
{
    public class MusicStart : MonoBehaviour
    {
        [SerializeField] private SoundEventSO _explorationMusicEvent;

        void Start()
        {
            PlayMusic();
        }


        //[ContextMenu("ExplorationMusicEvent")]
        public void PlayMusic()
        {
            // Play the sound using the modular system
            AudioManager.Instance.PlayMusic(_explorationMusicEvent, 3.0f); // 3 seconds fade
        }
    }  
}
