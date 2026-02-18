using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

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

        bool isMasterMuted = false;
        bool isMusicMuted = false;
        bool isAmbientMuted = false;
        bool isSoundMuted = false;


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

        public void InitializeAudioSettings()
        {
            PlayerPrefs.SetInt(_masterexposedParamName + "_Toggle", 1);
            PlayerPrefs.SetInt(_musicexposedParamName + "_Toggle", 1);
            PlayerPrefs.SetInt(_ambientexposedParamName + "_Toggle", 1);
            PlayerPrefs.SetInt(_soundexposedParamName + "_Toggle", 1);

            //LoadPrefsMuteState();
        }

        public void ApplyAudioSettings()
        {
            ApplyMuteState(_masterexposedParamName, !isMasterMuted);
            ApplyMuteState(_musicexposedParamName, !isMusicMuted);
            ApplyMuteState(_ambientexposedParamName, !isAmbientMuted);
            ApplyMuteState(_soundexposedParamName, !isSoundMuted);
        }

        public void SaveAudioSettings()
        {
            PlayerPrefs.SetInt(_masterexposedParamName + "_Toggle", isMasterMuted ? 0 : 1);
            PlayerPrefs.SetInt(_musicexposedParamName + "_Toggle", isMusicMuted ? 0 : 1);
            PlayerPrefs.SetInt(_ambientexposedParamName + "_Toggle", isAmbientMuted ? 0 : 1);
            PlayerPrefs.SetInt(_soundexposedParamName + "_Toggle", isSoundMuted ? 0 : 1);
            PlayerPrefs.Save();
            Debug.Log("Audio settings saved.");
        }

        [ContextMenu("ResetAudioSettings")]
        public void ResetAudioSettings()
        {
            // Clear saved preferences
            PlayerPrefs.DeleteKey(_masterexposedParamName + "_Toggle");
            PlayerPrefs.DeleteKey(_musicexposedParamName + "_Toggle");
            PlayerPrefs.DeleteKey(_ambientexposedParamName + "_Toggle");
            PlayerPrefs.DeleteKey(_soundexposedParamName + "_Toggle");
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
            Debug.Log("Audio settings reset to default (ON).");
        }

        public void MasterMuteToggled()
        {
            isMasterMuted = !isMasterMuted;
            ApplyMuteState(_masterexposedParamName, !isMasterMuted);
            GetPrefsAudioValues();
        }

        public void MusicMuteToggled()
        {
            isMusicMuted = !isMusicMuted;
            ApplyMuteState(_musicexposedParamName, !isMusicMuted);
            GetPrefsAudioValues();
        }

        public void AmbientMuteToggled()
        {
            isAmbientMuted = !isAmbientMuted;
            ApplyMuteState(_ambientexposedParamName, !isAmbientMuted);
            GetPrefsAudioValues();
        }   

        public void SoundMuteToggled()
        {
            isSoundMuted = !isSoundMuted;
            ApplyMuteState(_soundexposedParamName, !isSoundMuted);
            GetPrefsAudioValues();
        }    
        
       

        private void ApplyMuteState(string exposedParamName, bool isOn)
        {
            float targetVolume = isOn ? _onVolumeDB : _offVolumeDB;
            _mixer.SetFloat(exposedParamName, targetVolume);

            // Save state as Int (PlayerPrefs doesn't support bool)
            PlayerPrefs.SetInt(exposedParamName + "_Toggle", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }


        [ContextMenu("GetPrefsAudioValues")]
        private void GetPrefsAudioValues()
        {
            prefMaster = PlayerPrefs.GetInt(_masterexposedParamName, 1);
            prefMusic = PlayerPrefs.GetInt(_musicexposedParamName, 1);
            prefAmbient = PlayerPrefs.GetInt(_ambientexposedParamName, 1);
            prefSound = PlayerPrefs.GetInt(_soundexposedParamName, 1);

            Debug.Log($"AudioManager: GetPrefsudioValues called. Master: {prefMaster}, Music: {prefMusic}, SFX: {prefSound}, Ambient: {prefAmbient}");
        }

        public void LoadPrefsMuteState()
        {
            // 1. Load saved state (1 for ON, 0 for OFF). Default to ON (1).
            bool masterSavedState = PlayerPrefs.GetInt(_masterexposedParamName + "_Toggle", 1) == 1;
            bool musicSavedState = PlayerPrefs.GetInt(_musicexposedParamName + "_Toggle", 1) == 1;
            bool ambientSavedState = PlayerPrefs.GetInt(_ambientexposedParamName + "_Toggle", 1) == 1;
            bool soundSavedState = PlayerPrefs.GetInt(_soundexposedParamName + "_Toggle", 1) == 1;

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
    }
}
