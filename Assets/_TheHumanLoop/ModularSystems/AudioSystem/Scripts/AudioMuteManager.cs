using UnityEngine;
using UnityEngine.Audio;

namespace HumanLoop.AudioSystem
{
    public class AudioMuteManager : MonoBehaviour
    {
        public static AudioMuteManager Instance { get; private set; }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _masterexposedParamName;
        [SerializeField] private string _musicexposedParamName;
        [SerializeField] private string _ambientexposedParamName;
        [SerializeField] private string _soundexposedParamName;
       

        [Header("Settings")]
        [SerializeField] private float _onVolumeDB = 0f;    // Volume when active
        [SerializeField] private float _offVolumeDB = -80f; // Volume when muted

        // These are used to track the current mute state of each category
        private bool isMasterMuted = false;
        private bool isMusicMuted = false;
        private bool isAmbientMuted = false;
        private bool isSoundMuted = false;

        // These are used to store the current state of the toggles for saving/loading purposes
        private int prefMaster;
        private int prefMusic;
        private int prefSound;
        private int prefAmbient;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            InitializeAudioSettings();
        }

        // This method can be called to set all audio categories to their default state (ON) and save that state in PlayerPrefs.
        public void InitializeAudioSettings()
        {
            PlayerPrefs.SetInt(AudioPrefsConstants.MasterVolume_toggle_Key, 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.MusicVolume_toggle_Key, 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.AmbientVolume_toggle_Key, 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.SFXVolume_toggle_Key, 1);

            //LoadPrefsMuteState();
        }

        // This method applies the current mute states to the AudioMixer.
        // It can be called after loading settings or when toggles are changed.
        public void ApplyAudioSettings()
        {
            ApplyMuteState(_masterexposedParamName, !isMasterMuted);
            ApplyMuteState(_musicexposedParamName, !isMusicMuted);
            ApplyMuteState(_ambientexposedParamName, !isAmbientMuted);
            ApplyMuteState(_soundexposedParamName, !isSoundMuted);
        }

        // This method saves the current mute states to PlayerPrefs.
        // It should be called whenever a toggle is changed to ensure settings persist.
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetInt(AudioPrefsConstants.MasterVolume_toggle_Key, isMasterMuted ? 0 : 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.MusicVolume_toggle_Key, isMusicMuted ? 0 : 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.AmbientVolume_toggle_Key, isAmbientMuted ? 0 : 1);
            PlayerPrefs.SetInt(AudioPrefsConstants.SFXVolume_toggle_Key, isSoundMuted ? 0 : 1);
            PlayerPrefs.Save();
            //Debug.Log("Audio settings saved.");
        }

        // These methods are called by the UI toggles to update the mute state and apply it immediately.
        public void MasterMuteToggled()
        {
            isMasterMuted = !isMasterMuted;
            ApplyMuteState(_masterexposedParamName, !isMasterMuted);
            GetPrefsAudioValues();
        }

        // Similar methods for Music, Ambient, and Sound toggles
        public void MusicMuteToggled()
        {
            isMusicMuted = !isMusicMuted;
            ApplyMuteState(_musicexposedParamName, !isMusicMuted);
            GetPrefsAudioValues();
        }

        // Similar methods for Music, Ambient, and Sound toggles
        public void AmbientMuteToggled()
        {
            isAmbientMuted = !isAmbientMuted;
            ApplyMuteState(_ambientexposedParamName, !isAmbientMuted);
            GetPrefsAudioValues();
        }

        // Similar methods for Music, Ambient, and Sound toggles
        public void SoundMuteToggled()
        {
            isSoundMuted = !isSoundMuted;
            ApplyMuteState(_soundexposedParamName, !isSoundMuted);
            GetPrefsAudioValues();
        }



        // This helper method applies the mute state to the AudioMixer and saves it to PlayerPrefs.
        private void ApplyMuteState(string exposedParamName, bool isOn)
        {
            float targetVolume = isOn ? _onVolumeDB : _offVolumeDB;
            _mixer.SetFloat(exposedParamName, targetVolume);

            // Save state as Int (PlayerPrefs doesn't support bool)
            PlayerPrefs.SetInt(exposedParamName + "_Toggle", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }


        //[ContextMenu("GetPrefsAudioValues")]
        private void GetPrefsAudioValues()
        {
            prefMaster = PlayerPrefs.GetInt(_masterexposedParamName, 1);
            prefMusic = PlayerPrefs.GetInt(_musicexposedParamName, 1);
            prefAmbient = PlayerPrefs.GetInt(_ambientexposedParamName, 1);
            prefSound = PlayerPrefs.GetInt(_soundexposedParamName, 1);

            //Debug.Log($"AudioManager: GetPrefsudioValues called. Master: {prefMaster}, Music: {prefMusic}, SFX: {prefSound}, Ambient: {prefAmbient}");
        }

        public void LoadPrefsMuteState()
        {
            // 1. Load saved state (1 for ON, 0 for OFF). Default to ON (1).
            bool masterSavedState = PlayerPrefs.GetInt(AudioPrefsConstants.MasterVolume_toggle_Key, 1) == 1;
            bool musicSavedState = PlayerPrefs.GetInt(AudioPrefsConstants.MusicVolume_toggle_Key, 1) == 1;
            bool ambientSavedState = PlayerPrefs.GetInt(AudioPrefsConstants.AmbientVolume_toggle_Key, 1) == 1;
            bool soundSavedState = PlayerPrefs.GetInt(AudioPrefsConstants.SFXVolume_toggle_Key, 1) == 1;

            // 2. Sync UI without triggering the event yet
            isMasterMuted = masterSavedState;
            isMusicMuted = musicSavedState;
            isAmbientMuted = ambientSavedState;
            isSoundMuted = soundSavedState;

            // 3. Apply the actual volume to the Mixer
            ApplyMuteState(_masterexposedParamName, masterSavedState);
            ApplyMuteState(_musicexposedParamName, musicSavedState);
            ApplyMuteState(_ambientexposedParamName, ambientSavedState);
            ApplyMuteState(_soundexposedParamName, soundSavedState);
        }

        public void ToggleMasterMute(bool isOn)
        {
            isMasterMuted = !isOn; // Invert because isOn means "not muted"
            ApplyMuteState(_masterexposedParamName, isOn);
        }

        public void ToggleMusicMute(bool isOn)
        {
            isMusicMuted = !isOn;
            ApplyMuteState(_musicexposedParamName, isOn);
        }

        public void ToggleAmbientMute(bool isOn)
        {
            isAmbientMuted = !isOn;
            ApplyMuteState(_ambientexposedParamName, isOn);
        }

        public void ToggleSoundMute(bool isOn)
        {
            isSoundMuted = !isOn;
            ApplyMuteState(_soundexposedParamName, isOn);
        }


#if (UNITY_EDITOR)
        #region Testing Context Menu Methods
        [ContextMenu("ResetAudioSettings")]
        public void ResetAudioSettings()
        {
            // Clear saved preferences
            PlayerPrefs.DeleteKey(AudioPrefsConstants.MasterVolume_toggle_Key);
            PlayerPrefs.DeleteKey(AudioPrefsConstants.MusicVolume_toggle_Key);
            PlayerPrefs.DeleteKey(AudioPrefsConstants.AmbientVolume_toggle_Key);
            PlayerPrefs.DeleteKey(AudioPrefsConstants.SFXVolume_toggle_Key);
            PlayerPrefs.Save();
            // Reset UI and Mixer to default (ON)
            isMasterMuted = false;
            isMusicMuted = false;
            isAmbientMuted = false;
            isSoundMuted = false;
            ApplyMuteState(_masterexposedParamName, true);
            ApplyMuteState(_musicexposedParamName, true);
            ApplyMuteState(_ambientexposedParamName, true);
            ApplyMuteState(_soundexposedParamName, true);
            //Debug.Log("Audio settings reset to default (ON).");
        }
        #endregion
#endif
    }
}
