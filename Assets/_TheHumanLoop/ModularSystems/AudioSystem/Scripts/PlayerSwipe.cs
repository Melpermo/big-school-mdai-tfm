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

#if(UNITY_EDITOR)
        #region Context Menu Testing
        [ContextMenu("Player Swipe Event")]
        private void TestSwipe()
        {
            Swipe();
        }
        #endregion
    }
#endif
}
