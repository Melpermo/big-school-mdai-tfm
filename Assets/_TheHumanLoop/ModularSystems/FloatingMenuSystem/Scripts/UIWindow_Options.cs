using HumanLoop.AudioSystem;
using UnityEngine;

namespace TheHumanLoop.UI
{
    public class UIWindow_Options : MonoBehaviour
    {
        [Header("UI TOGGLE SWITCH")]
        [SerializeField] private UIToggleSwitch _masterSwitch;
        [SerializeField] private UIToggleSwitch _musicSwitch;
        [SerializeField] private UIToggleSwitch _ambientSwitch;
        [SerializeField] private UIToggleSwitch _sfxSwitch;

        [Header("GameSound Event")]
        [SerializeField] private SoundEventSO _buttonPressedSoundEvent;

        // These strings should match the keys used in PlayerPrefs and AudioMuteManager for consistency.
        private string masterExposedParamName = AudioPrefsConstants.MasterVolume_toggle_Key;
        private string musicExposedParamName = AudioPrefsConstants.MusicVolume_toggle_Key;
        private string ambientExposedParamName = AudioPrefsConstants.AmbientVolume_toggle_Key;
        private string sfxExposedParamName = AudioPrefsConstants.SFXVolume_toggle_Key;


        private void Start()
        {
            LoadPrefsValues();
            ListenChanges();
        }

        // This method listens to changes in the toggle switches
        // and updates the PlayerPrefs and calls the AudioMuteManager methods accordingly.
        public void ListenChanges()
        {
            //Debug.Log("Listening to changes in toggle switches...");

            // Listen to changes

            _masterSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                //Debug.Log("Master is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("MasterVol", isOn ? 1 : 0);
                AudioMuteManager.Instance.MasterMuteToggled();
                _masterSwitch.Setup(isOn); // Update the switch state to reflect the change
                PlayClickSound();
            };

            _musicSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                //Debug.Log("Music is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("Music", isOn ? 1 : 0);
                AudioMuteManager.Instance.MusicMuteToggled();
                _musicSwitch.Setup(isOn); // Update the switch state to reflect the change
                PlayClickSound();
            };

            _ambientSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                //Debug.Log("Ambient is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("Ambient", isOn ? 1 : 0);
                AudioMuteManager.Instance.AmbientMuteToggled();
                _ambientSwitch.Setup(isOn); // Update the switch state to reflect the change
                PlayClickSound();
            };

            _sfxSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                //Debug.Log("SFX is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("SFX", isOn ? 1 : 0);
                AudioMuteManager.Instance.SoundMuteToggled();
                _sfxSwitch.Setup(isOn); // Update the switch state to reflect the change
                PlayClickSound();
            };
        }

        // This method initializes the toggle switches based on the saved PlayerPrefs values.
        public void LoadPrefsValues()
        {
            // Initialize (Example: loading from PlayerPrefs)
            _masterSwitch.Setup(PlayerPrefs.GetInt(masterExposedParamName, 1) == 1);
            _musicSwitch.Setup(PlayerPrefs.GetInt(musicExposedParamName, 1) == 1);
            _ambientSwitch.Setup(PlayerPrefs.GetInt(ambientExposedParamName, 1) == 1);
            _sfxSwitch.Setup(PlayerPrefs.GetInt(sfxExposedParamName, 1) == 1);
        }

        private void PlayClickSound()
        {
            //Debug.Log("UIWindow_Options: Button clicked! Playing sound effect...");
            if (_buttonPressedSoundEvent != null)
            {
                AudioManager.Instance.PlaySound(_buttonPressedSoundEvent);
            }
        }
    }
}
