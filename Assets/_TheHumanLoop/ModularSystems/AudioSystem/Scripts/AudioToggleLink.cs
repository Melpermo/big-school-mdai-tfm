using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace HumanLoop.AudioSystem
{
    //[RequireComponent(typeof(Toggle))]
    public class AudioToggleLink : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _exposedParamName;

        [Header("Settings")]
        [SerializeField] private float _onVolumeDB = 0f;    // Volume when active
        [SerializeField] private float _offVolumeDB = -80f; // Volume when muted

        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void Start()
        {
            // 1. Load saved state (1 for ON, 0 for OFF). Default to ON (1).
            bool savedState = PlayerPrefs.GetInt(_exposedParamName + "_Toggle", 1) == 1;

            // 2. Sync UI without triggering the event yet
            _toggle.isOn = savedState;

            // 3. Apply the actual volume to the Mixer
            ApplyMuteState(savedState);

            // 4. Listen for future clicks
            _toggle.onValueChanged.AddListener(ApplyMuteState);
        }

        private void ApplyMuteState(bool isOn)
        {
            float targetVolume = isOn ? _onVolumeDB : _offVolumeDB;
            _mixer.SetFloat(_exposedParamName, targetVolume);

            // Save state as Int (PlayerPrefs doesn't support bool)
            PlayerPrefs.SetInt(_exposedParamName + "_Toggle", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(ApplyMuteState);
        }
    }
}