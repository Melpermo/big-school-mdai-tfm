using UnityEngine;
using HumanLoop.Data;
using System.Collections.Generic;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// Manages loading of localized EndGameCondition assets based on current language.
    /// Dynamically loads conditions from Resources folder with language suffix.
    /// </summary>
    public class EndGameConditionLocalizationManager : MonoBehaviour
    {
        public static EndGameConditionLocalizationManager Instance { get; private set; }

        [Header("Condition Base Names (Without Language Suffix)")]
        [Header("Global Conditions")]
        [SerializeField] private string gameOverName = "GameOver";
        [SerializeField] private string victoryName = "Victory";

        [Header("Fail Conditions")]
        [SerializeField] private string failBudgetName = "FailBudget";
        [SerializeField] private string failMoraleName = "FailMorale";
        [SerializeField] private string failTimeName = "FailTime";
        [SerializeField] private string failQualityName = "FailQuality";

        [Header("Met Conditions")]
        [SerializeField] private string metBudgetName = "MetBudget";
        [SerializeField] private string metMoraleName = "MetMorale";
        [SerializeField] private string metTimeName = "MetTime";
        [SerializeField] private string metQualityName = "MetQuality";

        [Header("Resources Path")]
        [SerializeField] private string conditionsResourcePath = "EndGameConditions";

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Cached conditions for current language
        private EndGameConditionSO _cachedGameOver;
        private EndGameConditionSO _cachedVictory;
        private EndGameConditionSO _cachedFailBudget;
        private EndGameConditionSO _cachedFailMorale;
        private EndGameConditionSO _cachedFailTime;
        private EndGameConditionSO _cachedFailQuality;
        private EndGameConditionSO _cachedMetBudget;
        private EndGameConditionSO _cachedMetMorale;
        private EndGameConditionSO _cachedMetTime;
        private EndGameConditionSO _cachedMetQuality;

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
            LoadConditionsForCurrentLanguage();
        }

        #endregion

        #region Public API - Global Conditions

        public EndGameConditionSO GameOver => _cachedGameOver;
        public EndGameConditionSO Victory => _cachedVictory;

        #endregion

        #region Public API - Fail Conditions

        public EndGameConditionSO FailBudget => _cachedFailBudget;
        public EndGameConditionSO FailMorale => _cachedFailMorale;
        public EndGameConditionSO FailTime => _cachedFailTime;
        public EndGameConditionSO FailQuality => _cachedFailQuality;

        #endregion

        #region Public API - Met Conditions

        public EndGameConditionSO MetBudget => _cachedMetBudget;
        public EndGameConditionSO MetMorale => _cachedMetMorale;
        public EndGameConditionSO MetTime => _cachedMetTime;
        public EndGameConditionSO MetQuality => _cachedMetQuality;

        #endregion

        #region Public API - List Access

        /// <summary>
        /// Returns all localized conditions in the correct order for GameStatsManager.
        /// Order matters: Specific conditions first, global conditions last.
        /// </summary>
        public List<EndGameConditionSO> GetLocalizedConditionsList()
        {
            var list = new List<EndGameConditionSO>();

            // Add fail conditions first (most specific)
            if (_cachedFailBudget != null) list.Add(_cachedFailBudget);
            if (_cachedFailMorale != null) list.Add(_cachedFailMorale);
            if (_cachedFailTime != null) list.Add(_cachedFailTime);
            if (_cachedFailQuality != null) list.Add(_cachedFailQuality);

            // Add met conditions
            if (_cachedMetBudget != null) list.Add(_cachedMetBudget);
            if (_cachedMetMorale != null) list.Add(_cachedMetMorale);
            if (_cachedMetTime != null) list.Add(_cachedMetTime);
            if (_cachedMetQuality != null) list.Add(_cachedMetQuality);

            // Add global conditions last (least specific)
            if (_cachedVictory != null) list.Add(_cachedVictory);
            if (_cachedGameOver != null) list.Add(_cachedGameOver);

            return list;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Reloads all conditions for current language.
        /// Called automatically when language changes.
        /// </summary>
        public void LoadConditionsForCurrentLanguage()
        {
            if (LanguageManager.Instance == null)
            {
                Debug.LogError("[EndGameConditionLocalizationManager] LanguageManager.Instance is null!");
                return;
            }

            string suffix = LanguageManager.Instance.GetLanguageSuffix();

            // Load global conditions
            _cachedGameOver = LoadCondition(gameOverName, suffix);
            _cachedVictory = LoadCondition(victoryName, suffix);

            // Load fail conditions
            _cachedFailBudget = LoadCondition(failBudgetName, suffix);
            _cachedFailMorale = LoadCondition(failMoraleName, suffix);
            _cachedFailTime = LoadCondition(failTimeName, suffix);
            _cachedFailQuality = LoadCondition(failQualityName, suffix);

            // Load met conditions
            _cachedMetBudget = LoadCondition(metBudgetName, suffix);
            _cachedMetMorale = LoadCondition(metMoraleName, suffix);
            _cachedMetTime = LoadCondition(metTimeName, suffix);
            _cachedMetQuality = LoadCondition(metQualityName, suffix);

            if (showDebugLogs)
            {
                Debug.Log($"[EndGameConditionLocalizationManager] Loaded {GetLoadedCount()} conditions for language: {LanguageManager.Instance.CurrentLanguage}");
            }
        }

        private EndGameConditionSO LoadCondition(string baseName, string languageSuffix)
        {
            string fullPath = $"{conditionsResourcePath}/{baseName}{languageSuffix}";
            EndGameConditionSO condition = Resources.Load<EndGameConditionSO>(fullPath);

            if (condition == null)
            {
                Debug.LogError($"[EndGameConditionLocalizationManager] Failed to load condition at: Resources/{fullPath}");
            }
            else if (showDebugLogs)
            {
                Debug.Log($"[EndGameConditionLocalizationManager] Loaded: {fullPath}");
            }

            return condition;
        }

        private void OnLanguageChanged(LanguageManager.Language newLanguage)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[EndGameConditionLocalizationManager] Language changed to: {newLanguage}, reloading conditions...");
            }

            LoadConditionsForCurrentLanguage();
        }

        #endregion

        #region Helpers

        private int GetLoadedCount()
        {
            int count = 0;
            if (_cachedGameOver != null) count++;
            if (_cachedVictory != null) count++;
            if (_cachedFailBudget != null) count++;
            if (_cachedFailMorale != null) count++;
            if (_cachedFailTime != null) count++;
            if (_cachedFailQuality != null) count++;
            if (_cachedMetBudget != null) count++;
            if (_cachedMetMorale != null) count++;
            if (_cachedMetTime != null) count++;
            if (_cachedMetQuality != null) count++;
            return count;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug/Reload Conditions")]
        private void DebugReloadConditions()
        {
            LoadConditionsForCurrentLanguage();
            Debug.Log("[EndGameConditionLocalizationManager] Conditions reloaded manually");
        }

        [ContextMenu("Debug/Log Cached Conditions")]
        private void DebugLogCachedConditions()
        {
            Debug.Log($"=== CACHED END GAME CONDITIONS ===\n" +
                     $"GameOver: {(_cachedGameOver != null ? _cachedGameOver.name : "NULL")}\n" +
                     $"Victory: {(_cachedVictory != null ? _cachedVictory.name : "NULL")}\n" +
                     $"FailBudget: {(_cachedFailBudget != null ? _cachedFailBudget.name : "NULL")}\n" +
                     $"FailMorale: {(_cachedFailMorale != null ? _cachedFailMorale.name : "NULL")}\n" +
                     $"FailTime: {(_cachedFailTime != null ? _cachedFailTime.name : "NULL")}\n" +
                     $"FailQuality: {(_cachedFailQuality != null ? _cachedFailQuality.name : "NULL")}\n" +
                     $"MetBudget: {(_cachedMetBudget != null ? _cachedMetBudget.name : "NULL")}\n" +
                     $"MetMorale: {(_cachedMetMorale != null ? _cachedMetMorale.name : "NULL")}\n" +
                     $"MetTime: {(_cachedMetTime != null ? _cachedMetTime.name : "NULL")}\n" +
                     $"MetQuality: {(_cachedMetQuality != null ? _cachedMetQuality.name : "NULL")}\n" +
                     $"TOTAL LOADED: {GetLoadedCount()}/10");
        }

        [ContextMenu("Debug/Log Condition Order")]
        private void DebugLogConditionOrder()
        {
            var list = GetLocalizedConditionsList();
            Debug.Log($"=== CONDITION ORDER (Count: {list.Count}) ===");
            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log($"[{i}] {list[i].name}");
            }
        }
#endif

        #endregion
    }
}