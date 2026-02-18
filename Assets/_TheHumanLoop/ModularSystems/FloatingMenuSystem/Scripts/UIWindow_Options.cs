using HumanLoop.AudioSystem;
using UnityEngine;

namespace TheHumanLoop.UI
{
    public class UIWindow_Options : MonoBehaviour
    {
        [Header("Exposed Param Names")]
        [SerializeField] private string _masterExposedParamName = "MasterVol";
        [SerializeField] private string _musicExposedParamName = "MusicVol";
        [SerializeField] private string _ambientExposedParamName = "AmbientVol";
        [SerializeField] private string _sfxExposedParamName = "SFXVol";

        [Header("UI TOGGLE SWITCH")]
        [SerializeField] private UIToggleSwitch _masterSwitch;
        [SerializeField] private UIToggleSwitch _musicSwitch;
        [SerializeField] private UIToggleSwitch _ambientSwitch;
        [SerializeField] private UIToggleSwitch _sfxSwitch;


        private void Start()
        {
            // Initialize (Example: loading from PlayerPrefs)
            _masterSwitch.Setup(PlayerPrefs.GetInt(_masterExposedParamName, 1) == 1);
            _musicSwitch.Setup(PlayerPrefs.GetInt(_musicExposedParamName, 1) == 1);
            _ambientSwitch.Setup(PlayerPrefs.GetInt(_ambientExposedParamName, 1) == 1);
            _sfxSwitch.Setup(PlayerPrefs.GetInt(_sfxExposedParamName, 1) == 1);
            
            // Listen to changes

            _masterSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                Debug.Log("Master is now: " + (isOn ? "ON" : "OFF"));
                //PlayerPrefs.SetInt("MasterVol", isOn ? 1 : 0);
                AudioMuteManager.Instance.MasterMuteToggled();
            };

            _musicSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                Debug.Log("Music is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("Music", isOn ? 1 : 0);
                AudioMuteManager.Instance.MusicMuteToggled();
            };

            _ambientSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                Debug.Log("Ambient is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("Ambient", isOn ? 1 : 0);
                AudioMuteManager.Instance.AmbientMuteToggled();
            };

            _sfxSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                Debug.Log("SFX is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("SFX", isOn ? 1 : 0);
                AudioMuteManager.Instance.SoundMuteToggled();
            };
        }
    }
}
