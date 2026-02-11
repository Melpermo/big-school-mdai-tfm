using UnityEngine;

namespace HumanLoop.AudioSystem
{
    public class PlayerSwipe : MonoBehaviour
    {
        public SoundEventSO playerSwipeEvent;

        void Swipe()
        {
            // Play the sound using the modular system
            AudioManager.Instance.PlaySound(playerSwipeEvent);
        }

        [ContextMenu("Player Swipe Event")]
        private void TestSwipe()
        {
            Swipe();
        }
    }
}
