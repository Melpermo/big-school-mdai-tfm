using UnityEngine;
using HumanLoop.Data;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// Manages loading of localized deck assets based on current language.
    /// Dynamically loads decks from Resources folder with language suffix.
    /// </summary>
    public class DeckLocalizationManager : MonoBehaviour
    {
        public static DeckLocalizationManager Instance { get; private set; }

        [Header("Deck Base Names (Without Language Suffix)")]
        [SerializeField] private string completeDeckName = "CompleteGameDeck";
        [SerializeField] private string earlyPhaseDeckName = "EarlyPhaseDeck";
        [SerializeField] private string midPhaseDeckName = "MidPhaseDeck";
        [SerializeField] private string endPhaseDeckName = "EndPhaseDeck";

        [Header("Resources Path")]
        [SerializeField] private string decksResourcePath = "Decks";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Cached decks for current language
        private DeckSO _cachedCompleteDeck;
        private DeckSO _cachedEarlyPhaseDeck;
        private DeckSO _cachedMidPhaseDeck;
        private DeckSO _cachedEndPhaseDeck;

        [SerializeField] private string suffix;

        #region Singleton

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += OnLanguageChanged;            
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        }

        private void Start()
        {
            LoadDecksForCurrentLanguage();            

            /*
            if (LanguageManager.Instance == null)
            {
                Debug.LogError("[DeckLocalizationManager] LanguageManager.Instance is null on Start!");
                return;
            }
            suffix = LanguageManager.Instance.GetLanguageSuffix();*/
        }

        #endregion

        #region Public API

        public DeckSO CompleteDeck => _cachedCompleteDeck;
        public DeckSO EarlyPhaseDeck => _cachedEarlyPhaseDeck;
        public DeckSO MidPhaseDeck => _cachedMidPhaseDeck;
        public DeckSO EndPhaseDeck => _cachedEndPhaseDeck;

        /// <summary>
        /// Reloads all decks for current language.
        /// Called automatically when language changes.
        /// </summary>
        public void LoadDecksForCurrentLanguage()
        {
            if (LanguageManager.Instance == null)
            {
                Debug.LogError("[DeckLocalizationManager] LanguageManager.Instance is null!");
                return;
            }

            suffix = LanguageManager.Instance.GetLanguageSuffix();

            _cachedCompleteDeck = LoadDeck(completeDeckName, suffix);
            _cachedEarlyPhaseDeck = LoadDeck(earlyPhaseDeckName, suffix);
            _cachedMidPhaseDeck = LoadDeck(midPhaseDeckName, suffix);
            _cachedEndPhaseDeck = LoadDeck(endPhaseDeckName, suffix);

            if (showDebugLogs)
            {
                Debug.Log($"[DeckLocalizationManager] Loaded decks for language: {LanguageManager.Instance.CurrentLanguage}");
            }
        }

        #endregion

        #region Private Methods

        private DeckSO LoadDeck(string baseName, string languageSuffix)
        {
            string fullPath = $"{decksResourcePath}/{baseName}{languageSuffix}";
            DeckSO deck = Resources.Load<DeckSO>(fullPath);

            if (deck == null)
            {
                Debug.LogError($"[DeckLocalizationManager] Failed to load deck at: Resources/{fullPath}");
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[DeckLocalizationManager] Loaded: {fullPath}");
            }

            return deck;
        }

        private void OnLanguageChanged(LanguageManager.Language newLanguage)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[DeckLocalizationManager] Language changed to: {newLanguage}, reloading decks...");
            }

            LoadDecksForCurrentLanguage();
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        [ContextMenu("Debug/Reload Decks")]
        private void DebugReloadDecks()
        {
            LoadDecksForCurrentLanguage();
            Debug.Log("[DeckLocalizationManager] Decks reloaded manually");
        }

        [ContextMenu("Debug/Log Cached Decks")]
        private void DebugLogCachedDecks()
        {
            Debug.Log($"=== CACHED DECKS ===\n" +
                     $"Complete: {(_cachedCompleteDeck != null ? _cachedCompleteDeck.name : "NULL")}\n" +
                     $"Early: {(_cachedEarlyPhaseDeck != null ? _cachedEarlyPhaseDeck.name : "NULL")}\n" +
                     $"Mid: {(_cachedMidPhaseDeck != null ? _cachedMidPhaseDeck.name : "NULL")}\n" +
                     $"End: {(_cachedEndPhaseDeck != null ? _cachedEndPhaseDeck.name : "NULL")}");
        }
        #endif

        #endregion
    }
}