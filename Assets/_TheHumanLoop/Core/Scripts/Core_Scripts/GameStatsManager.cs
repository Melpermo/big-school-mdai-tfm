using HumanLoop.Events;
using HumanLoop.Data;   
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace HumanLoop.Core
{
    /// <summary>
    /// Global manager for the 4 main game statistics (Budget, Time, Morale, Quality).
    /// Handles stat updates, validation, and checks end game conditions after each change.
    /// Implements Singleton pattern for global access.
    /// </summary>
    public class GameStatsManager : MonoBehaviour
    {
        // Singleton instance accessible from anywhere
        public static GameStatsManager Instance { get; private set; }

        [Header("Initial Values")]
        [Tooltip("Budget: Money for salaries, tools, licenses (0-100)")]
        [Range(0, 100)] public float budget = 50f;

        [Tooltip("Time: Deadlines, delays, project margin (0-100)")]
        [Range(0, 100)] public float time = 50f;

        [Tooltip("Morale: Motivation, mental health, trust (0-100)")]
        [Range(0, 100)] public float morale = 50f;

        [Tooltip("Quality: Code health, technical debt (0-100)")]
        [Range(0, 100)] public float quality = 50f;

        [Header("Events")]
        [Tooltip("Raised whenever any stat value changes")]
        [SerializeField] private GameEventSO _onStatsChangedEvent;

        [Header("UI References")]
        [Tooltip("Direct reference to EndGameUIHandler for displaying end game screens")]
        [SerializeField] private UI.EndGameUIHandler endGameUIHandler;

        [Tooltip("If true, also raises events (for backwards compatibility)")]
        [SerializeField] private bool alsoRaiseEvents = false;

        [Header("End Game Conditions Settings")]
        [Tooltip("If true, uses EndGameConditionLocalizationManager for localized conditions")]
        [SerializeField] private bool useLocalizedConditions = true;

        [Header("End Game Conditions (Only used if useLocalizedConditions is false)")]
        [Tooltip("**IMPORTANT: THE ORDER MATTERS.** Specific conditions first, global last.")]
        [SerializeField] private List<EndGameConditionSO> endGameConditionsList;

        // Store initial values for reset functionality
        private float _initialBudget;
        private float _initialTime;
        private float _initialMorale;
        private float _initialQuality;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;

            // Store initial values
            _initialBudget = budget;
            _initialTime = time;
            _initialMorale = morale;
            _initialQuality = quality;           
        }

        private void OnEnable()
        {
            if (useLocalizedConditions)
            {
                LocalizationSystem.LanguageManager.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            if (useLocalizedConditions)
            {
                LocalizationSystem.LanguageManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void Start()
        {
            if (useLocalizedConditions)
            {
                StartCoroutine(WaitAndLoadLocalizedConditions());
            }
        }

        /// <summary>
        /// Waits for EndGameConditionLocalizationManager to fully load conditions, then loads them.
        /// </summary>
        private System.Collections.IEnumerator WaitAndLoadLocalizedConditions()
        {
            // Wait for instance to exist
            int retries = 0;
            const int maxRetries = 20;

            while (LocalizationSystem.EndGameConditionLocalizationManager.Instance == null && retries < maxRetries)
            {
                retries++;
                yield return new WaitForSeconds(0.05f);
            }

            if (LocalizationSystem.EndGameConditionLocalizationManager.Instance == null)
            {
                Debug.LogError("[GameStatsManager] EndGameConditionLocalizationManager.Instance is null after waiting!");
                yield break;
            }

            // Wait for conditions to be loaded (check that list is not empty)
            retries = 0;
            List<EndGameConditionSO> conditions = null;

            while (retries < maxRetries)
            {
                conditions = LocalizationSystem.EndGameConditionLocalizationManager.Instance.GetLocalizedConditionsList();
                
                if (conditions != null && conditions.Count > 0)
                {
                    break; // Conditions are ready!
                }

                retries++;
                yield return new WaitForSeconds(0.05f);
            }

            // Now load conditions
            if (conditions != null && conditions.Count > 0)
            {
                endGameConditionsList = conditions;
                Debug.Log($"[GameStatsManager] ✓ Loaded {endGameConditionsList.Count} localized end game conditions");
            }
            else
            {
                Debug.LogError("[GameStatsManager] Failed to load localized conditions after waiting! " +
                              "EndGameConditionLocalizationManager returned empty list.");
            }
        }

        private void LoadLocalizedConditions()
        {
            if (LocalizationSystem.EndGameConditionLocalizationManager.Instance == null)
            {
                Debug.LogError("[GameStatsManager] EndGameConditionLocalizationManager.Instance is null!");
                return;
            }

            List<EndGameConditionSO> loadedList = LocalizationSystem.EndGameConditionLocalizationManager.Instance.GetLocalizedConditionsList();

            if (loadedList == null || loadedList.Count == 0)
            {
                Debug.LogWarning("[GameStatsManager] GetLocalizedConditionsList() returned empty list!");
                return;
            }

            endGameConditionsList = loadedList;
            Debug.Log($"[GameStatsManager] ✓ Reloaded {endGameConditionsList.Count} localized conditions (language changed)");
        }

        private void OnLanguageChanged(LocalizationSystem.LanguageManager.Language newLanguage)
        {
            Debug.Log($"[GameStatsManager] Language changed to {newLanguage}, reloading conditions...");
            StartCoroutine(ReloadConditionsAfterLanguageChange());
        }

        /// <summary>
        /// Reloads conditions after language change, waiting for new conditions to be loaded.
        /// </summary>
        private System.Collections.IEnumerator ReloadConditionsAfterLanguageChange()
        {
            // Give EndGameConditionLocalizationManager time to reload
            yield return new WaitForSeconds(0.1f);

            int retries = 0;
            const int maxRetries = 10;
            List<EndGameConditionSO> newConditions = null;

            // Wait until new conditions are loaded
            while (retries < maxRetries)
            {
                if (LocalizationSystem.EndGameConditionLocalizationManager.Instance != null)
                {
                    newConditions = LocalizationSystem.EndGameConditionLocalizationManager.Instance.GetLocalizedConditionsList();
                    
                    if (newConditions != null && newConditions.Count > 0)
                    {
                        // Verify it's actually new conditions (different reference)
                        if (newConditions != endGameConditionsList)
                        {
                            break;
                        }
                    }
                }

                retries++;
                yield return new WaitForSeconds(0.05f);
            }

            // Assign new conditions
            if (newConditions != null && newConditions.Count > 0)
            {
                endGameConditionsList = newConditions;
                Debug.Log($"[GameStatsManager] ✓ Reloaded {endGameConditionsList.Count} conditions in new language");
                
                // Log first condition name to verify language changed
                if (endGameConditionsList.Count > 0 && endGameConditionsList[0] != null)
                {
                    Debug.Log($"[GameStatsManager] First condition: {endGameConditionsList[0].name}");
                }
            }
            else
            {
                Debug.LogError("[GameStatsManager] Failed to reload conditions after language change!");
            }
        }

        /// <summary>
        /// Updates all four stats by adding the provided deltas and clamping to 0-100 range.
        /// Triggers stat changed event and checks for end game conditions.
        /// </summary>
        /// <param name="b">Budget delta (can be positive or negative)</param>
        /// <param name="t">Time delta (can be positive or negative)</param>
        /// <param name="m">Morale delta (can be positive or negative)</param>
        /// <param name="q">Quality delta (can be positive or negative)</param>
        public void UpdateStats(float b, float t, float m, float q)
        {
            // Apply deltas and clamp each stat to valid range (0-100)
            budget = Mathf.Clamp(budget + b, 0, 100);
            time = Mathf.Clamp(time + t, 0, 100);
            morale = Mathf.Clamp(morale + m, 0, 100);
            quality = Mathf.Clamp(quality + q, 0, 100);

            // Notify all listeners that stats have changed (updates UI, etc.)
            _onStatsChangedEvent?.Raise();

            // Check if any end game condition has been triggered
            CheckEndGameConditions();            
        }

        /// <summary>
        /// Evaluates all registered end game conditions.
        /// If any condition is met, shows UI directly and optionally raises event.
        /// </summary>
        private void CheckEndGameConditions()
        {
            if (endGameConditionsList == null || endGameConditionsList.Count == 0)
            {
                return;
            }

            foreach (var endGameCondition in endGameConditionsList)
            {
                if (endGameCondition == null)
                {
                    continue;
                }

                bool isMet = endGameCondition.CheckCondition();

                if (isMet)
                {
                    // Direct UI call (NEW - Primary method)
                    if (endGameUIHandler != null)
                    {
                        endGameUIHandler.SelectConditionTypeToShow(endGameCondition);
                    }
                    else
                    {
                        Debug.LogWarning("[GameStatsManager] endGameUIHandler not assigned! Falling back to events.");
                    }                    

                    return; // Stop checking after first match
                }
            }
        }

        /// <summary>
        /// Resets all stats to their initial values.
        /// PUBLIC - Available in builds.
        /// </summary>
        public void ResetToInitialValues()  // ← FUERA de #if UNITY_EDITOR
        {
            budget = _initialBudget;
            time = _initialTime;
            morale = _initialMorale;
            quality = _initialQuality;
            _onStatsChangedEvent?.Raise();
        }


#if (UNITY_EDITOR)
        #region Testing & Debug Methods

        /// <summary>
        /// Logs current state of all stats.
        /// </summary>

        [ContextMenu("Debug/Log Current Stats")]
        public void LogCurrentStats()
        {
            Debug.Log($"=== CURRENT STATS ===\n" +
                     $"Budget: {budget:F1}/100\n" +
                     $"Time: {time:F1}/100\n" +
                     $"Morale: {morale:F1}/100\n" +
                     $"Quality: {quality:F1}/100");
        }

        /// <summary>
        /// Logs all configured end game conditions and their status.
        /// </summary>
        [ContextMenu("Debug/Log All Conditions")]
        public void LogAllConditions()
        {
            if (endGameConditionsList == null || endGameConditionsList.Count == 0)
            {
                Debug.LogWarning("No end game conditions configured!");
                return;
            }

            Debug.Log($"=== END GAME CONDITIONS ({endGameConditionsList.Count} total) ===");
            for (int i = 0; i < endGameConditionsList.Count; i++)
            {
                var condition = endGameConditionsList[i];
                if (condition == null)
                {
                    Debug.LogWarning($"[{i}] NULL CONDITION");
                    continue;
                }

                bool isMet = condition.CheckCondition();
                string status = isMet ? "✓ MET" : "✗ NOT MET";                
                
                Debug.Log($"[{i}] {status} | {condition.conditionName}");
            }
        }       

        /// <summary>
        /// Tests positive stat change (+10 to all).
        /// </summary>
        [ContextMenu("Testing/Test Positive Change (+10 All)")]
        public void TestPositiveChange()
        {
            Debug.Log("Testing positive change: +10 to all stats");
            UpdateStats(10, 10, 10, 10);
        }

        /// <summary>
        /// Tests negative stat change (-10 to all).
        /// </summary>
        [ContextMenu("Testing/Test Negative Change (-10 All)")]
        public void TestNegativeChange()
        {
            Debug.Log("Testing negative change: -10 to all stats");
            UpdateStats(-10, -10, -10, -10);
        }

        /// <summary>
        /// Forces Budget to 0 to test Game Over condition.
        /// </summary>
        [ContextMenu("Testing/Force Budget to 0")]
        public void ForceBudgetGameOver()
        {
            Debug.Log("Forcing Budget to 0 - should trigger Game Over");
            budget = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Time to 0")]
        public void ForceTimeGameOver()
        {
            Debug.Log("Forcing Time to 0 - should trigger Game Over");
            time = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Morale to 0")]
        public void ForceMoraleGameOver()
        {
            Debug.Log("Forcing Morale to 0 - should trigger Game Over");
            morale = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Quality to 0")]
        public void ForceQualityGameOver()
        {
            Debug.Log("Forcing Quality to 0 - should trigger Game Over");
            quality = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        /// <summary>
        /// Forces all stats to 0 to test Game Over condition.
        /// </summary>
        [ContextMenu("Testing/Force All Stats to 0")]
        public void ForceGameOver()
        {
            Debug.Log("Forcing all stats to 0 - should trigger Victory");
            budget = time = morale = quality = 0f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }


        /// <summary>
        /// Forces all stats to 100 to test Victory condition.
        /// </summary>
        [ContextMenu("Testing/Force All Stats to 100")]
        public void ForceVictory()
        {
            Debug.Log("Forcing all stats to 100 - should trigger Victory");
            budget = time = morale = quality = 100f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        // Prueba esto con tus condiciones SpecialMet
        [ContextMenu("Testing/Test SpecialMet at 85")]
        public void TestSpecialMet()
        {
            Debug.Log("Testing SpecialMet: setting all stats to 85");
            budget = time = morale = quality = 85f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        /// <summary>
        /// Forces Budget to 96 to test Victory conditions.
        /// </summary>
        [ContextMenu("Testing/Force Budget to 96")]
        public void ForceBudgetVictory()
        {
            Debug.Log("Forcing Budget to 96 - should trigger Victory");
            budget = 96f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Time to 96")]
        public void ForceTimeVictory()
        {
            Debug.Log("Forcing Time to 96 - should trigger Victory");
            time = 96f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Morale to 96")]
        public void ForceMoraleVictory()
        {
            Debug.Log("Forcing Morale to 96 - should trigger Victory");
            morale = 96f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Quality to 96")]
        public void ForceQualityVictory()
        {
            Debug.Log("Forcing Quality to 96 - should trigger Victory");
            quality = 96f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        /// <summary>
        /// Forces Budget to 85 to test SpecialMet condition.
        /// </summary>
        [ContextMenu("Testing/Force Budget to 85 (SpecialMet)")]
        public void TestSpecialMetBudget()
        {
            Debug.Log("=== TESTING SPECIALMET: Budget to 85 ===");
            budget = 85f;
            Debug.Log($"Budget set to: {budget}");
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Debug/Force Reload Localized Conditions")]
        private void DebugForceReloadConditions()
        {
            if (!useLocalizedConditions)
            {
                Debug.LogWarning("useLocalizedConditions is false!");
                return;
            }

            LoadLocalizedConditions();
            Debug.Log($"Manual reload complete. Count: {endGameConditionsList?.Count ?? 0}");
        }

        [ContextMenu("Debug/Log EndGameConditionLocalizationManager Status")]
        private void DebugLogLocalizationStatus()
        {
            bool instanceExists = LocalizationSystem.EndGameConditionLocalizationManager.Instance != null;
            Debug.Log($"EndGameConditionLocalizationManager.Instance exists: {instanceExists}");
            
            if (instanceExists)
            {
                var list = LocalizationSystem.EndGameConditionLocalizationManager.Instance.GetLocalizedConditionsList();
                Debug.Log($"Available conditions: {list?.Count ?? 0}");
            }
        }
        #endregion
#endif
    }
}