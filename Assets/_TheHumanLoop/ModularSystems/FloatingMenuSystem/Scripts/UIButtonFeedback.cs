using DG.Tweening;
using HumanLoop.AudioSystem;
using UnityEngine;

namespace TheHumanLoop.UI
{
    public class UIButtonFeedback : MonoBehaviour
    {
        [Header("GameSound Event")]
        [SerializeField] private SoundEventSO _buttonPressedSoundEvent;

        public void OnButtonClick()
        {
            // fast beat effect
            transform.DOScale(0.95f, 0.1f).OnComplete(() => {
                transform.DOScale(1f, 0.1f);
            });

            //Debug.Log("UIButtonFeedback: Button clicked! Playing sound effect...");
            // Play click sound effect
            if (_buttonPressedSoundEvent != null)
            {
                AudioManager.Instance.PlaySound(_buttonPressedSoundEvent);
            }
        }
    }
}
