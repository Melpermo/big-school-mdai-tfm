using UnityEngine;
using UnityEngine.Audio;

namespace HumanLoop.AudioSystem
{
    public class AudioSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mainMixer;

        // Names must match the Exposed Parameters in your Audio Mixer exactly
        private const string MASTER_KEY = "MasterVol";
        private const string MUSIC_KEY = "MusicVol";
        private const string SFX_KEY = "SFXVol";
        private const string AMBIENT_KEY = "AmbientVol";

        private void Start()
        {
            // Load saved volumes on start, default to 0.75f if no key exists
            LoadAndApplyVolume(MASTER_KEY);
            LoadAndApplyVolume(MUSIC_KEY);
            LoadAndApplyVolume(SFX_KEY);
            LoadAndApplyVolume(AMBIENT_KEY);            
        }        

        private void LoadAndApplyVolume(string key)
        {
            float savedVolume = PlayerPrefs.GetFloat(key, 0.75f);
            SetMixerVolume(key, savedVolume);
        }

        // Methods for Sliders to call
        public void SetMasterVolume(float value) => SetMixerVolume(MASTER_KEY, value);
        public void SetMusicVolume(float value) => SetMixerVolume(MUSIC_KEY, value);
        public void SetSFXVolume(float value) => SetMixerVolume(SFX_KEY, value);
        public void SetAmbientVolume(float value) => SetMixerVolume(AMBIENT_KEY, value);

        private void SetMixerVolume(string parameterName, float sliderValue)
        {
            // Convert 0-1 linear to -80 to 20 dB log scale
            // Using 0.0001 to avoid Log10(0) error
            float dBValue = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;

            _mainMixer.SetFloat(parameterName, dBValue);

            // Save the raw slider value (0 to 1) for later use
            PlayerPrefs.SetFloat(parameterName, sliderValue);
            PlayerPrefs.Save();
        }


        // Context menu to reset all volumes to default (0.75f) for testing purposes
        [ContextMenu("ResetDefaultKeyValues")]
        private void ResetDefaultKeyValues()
        {
            SetMasterVolume(0.75f);
            SetMusicVolume(0.75f);
            SetAmbientVolume(0.75f);
            SetMasterVolume(0.75f);
            SetSFXVolume(0.75f);
            SetNewPlayerprefsKey(MASTER_KEY, 0.75f);
            SetNewPlayerprefsKey(MUSIC_KEY, 0.75f);
            SetNewPlayerprefsKey(SFX_KEY, 0.75f);
            SetNewPlayerprefsKey(AMBIENT_KEY, 0.75f);
        }

        private void SetNewPlayerprefsKey(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
    }
}