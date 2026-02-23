using HumanLoop.LocalizationSystem;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    public class LocalizationBootstrap : MonoBehaviour
    {
        // Singleton pattern to ensure only one instance of LocalizationBootstrap exists.
        public static LocalizationBootstrap Instance { get; private set; }

        // These fields are set in the Unity Inspector. The LocalizationTableSO should contain all your localized strings,
        [SerializeField] private LocalizationTableSO _table;

        [Header("Default language")]
        [SerializeField] private LanguageId _defaultLanguage = LanguageId.Spanish;

        [Header("Current language")]
        [SerializeField] private LanguageId _currentLanguage = LanguageId.Spanish;

        private LocalizationService m_service;

        private void Awake()
        {
            lock (this)
            {
                if (Instance != null && Instance != this)
                {
                    //Destroy(gameObject);
                    return;
                }
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }

            m_service = new LocalizationService(_table, _defaultLanguage);
            LocalizedTextTMP.Service = m_service;
        }

        // This method can be used to get the current language as an integer (0 for English, 1 for Spanish, etc.)
        // for use in UI elements like dropdowns.
        public int GetCurrentLanguaje()
        { 
            _currentLanguage = m_service.CurrentLanguage;
            return (int)m_service.CurrentLanguage;
        }

        // This method can be called to set the language using an integer ID (0 for English, 1 for Spanish, etc.)
        public void SetLanguage(int languageID)
        {
            var language = (LanguageId)languageID;
            m_service.SetLanguage(language);
        }

        // 👉 For TMP_Dropdown (0 = EN, 1 = ES)
        public void SetLanguageFromDropdown(int index)
        {
            var language = index == 1
                ? LanguageId.Spanish
                : LanguageId.English;

            m_service.SetLanguage(language);

            // Opcional: save SettingsService here if you have one, to persist user preference
            // settings.Language = language;
            // settings.Save();
        }

        [ContextMenu("Test/Set English")]
        public void SetEnglish()
        {
            m_service.SetLanguage(LanguageId.English);
            GetCurrentLanguaje();
            Debug.Log("Language set to English");
            Debug.Log($"Current language: {GetCurrentLanguaje()}");
        }

        [ContextMenu("Test/Set Spanish")]
        public void SetSpanish()
        {
            m_service.SetLanguage(LanguageId.Spanish);
            GetCurrentLanguaje();
            Debug.Log("Language set to Spanish");
            Debug.Log($"Current language: {GetCurrentLanguaje()}");
        }
       
    }
}