using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// Manages current language selection and persistence.
    /// Single source of truth for game language.
    /// </summary>
    public class LanguageManager : MonoBehaviour
    {
        public static LanguageManager Instance { get; private set; }

        public enum Language
        {
            Spanish,
            English
        }

        [Header("Current Language")]
        [SerializeField] private Language _currentLanguage = Language.Spanish;

        // Events
        public static event System.Action<Language> OnLanguageChanged;

        // PlayerPrefs key
        private const string LANGUAGE_PREF_KEY = "GameLanguage";

        #region Singleton

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadLanguagePreference();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API

        public Language CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Changes the current language and saves preference.
        /// </summary>
        public void SetLanguage(Language newLanguage)
        {
            if (_currentLanguage == newLanguage) return;

            _currentLanguage = newLanguage;
            SaveLanguagePreference();
            OnLanguageChanged?.Invoke(_currentLanguage);

            Debug.Log($"[LanguageManager] Language changed to: {_currentLanguage}");
        }

        /// <summary>
        /// Returns the language suffix for asset naming ("_ES" or "_EN").
        /// </summary>
        public string GetLanguageSuffix()
        {
            return _currentLanguage switch
            {
                Language.Spanish => "_ES",
                Language.English => "_EN",
                _ => "_ES"
            };
        }

        #endregion

        #region Persistence

        private void LoadLanguagePreference()
        {
            if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
            {
                int savedLang = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
                _currentLanguage = (Language)savedLang;
                //Debug.Log($"[LanguageManager] Loaded language: {_currentLanguage}");
            }
            else
            {
                // Default to Spanish
                _currentLanguage = Language.Spanish;
                SaveLanguagePreference();
            }
        }

        private void SaveLanguagePreference()
        {
            PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)_currentLanguage);
            PlayerPrefs.Save();
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug/Switch to Spanish")]
        private void DebugSwitchToSpanish()
        {
            SetLanguage(Language.Spanish);
        }

        [ContextMenu("Debug/Switch to English")]
        private void DebugSwitchToEnglish()
        {
            SetLanguage(Language.English);
        }

        [ContextMenu("Debug/Log Current Language")]
        private void DebugLogLanguage()
        {
            Debug.Log($"Current Language: {_currentLanguage}, Suffix: {GetLanguageSuffix()}");
        }
#endif

        #endregion
    }
}
