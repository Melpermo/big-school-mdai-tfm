using HumanLoop.Events;
using HumanLoop.Data;   
using System.Collections.Generic;
using UnityEngine;

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

        [Header("End Game Conditions")]
        [Tooltip("List of conditions to check. Each condition contains its own event to raise.")]
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
        /// If any condition is met, it raises its own event and stops checking (first match wins).
        /// </summary>
        private void CheckEndGameConditions()
        {
            // Early exit if no conditions configured
            if (endGameConditionsList == null || endGameConditionsList.Count == 0)
            {
                return;
            }

            // Iterate through all end game conditions
            foreach (var endGameCondition in endGameConditionsList)
            {
                // Skip null references
                if (endGameCondition == null)
                {
                    continue;
                }

                // Check if condition is met and let it raise its own event
                if (endGameCondition.CheckCondition())
                {
                    //Debug.Log($"End game condition met: {endGameCondition.conditionName} ({endGameCondition.conditionType})");
                    endGameCondition.RaiseEvent();
                    return; // Stop checking after first match to avoid multiple triggers
                }
            }
        }

        #region Testing & Debug Methods

        /// <summary>
        /// Logs current state of all stats.
        /// </summary>
        [ContextMenu("Debug/Log Current Stats")]
        private void LogCurrentStats()
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
        private void LogAllConditions()
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
                string eventName = condition.endGameConditionEvent != null 
                    ? condition.endGameConditionEvent.name 
                    : "NO EVENT";
                
                Debug.Log($"[{i}] {status} | {condition.conditionName} ({condition.conditionType}) → {eventName}");
            }
        }

        /// <summary>
        /// Resets all stats to their initial values.
        /// </summary>
        [ContextMenu("Testing/Reset to Initial Values")]
        private void ResetToInitialValues()
        {
            budget = _initialBudget;
            time = _initialTime;
            morale = _initialMorale;
            quality = _initialQuality;

            _onStatsChangedEvent?.Raise();
            Debug.Log($"Stats reset to initial values: B:{budget} T:{time} M:{morale} Q:{quality}");
        }

        /// <summary>
        /// Tests positive stat change (+10 to all).
        /// </summary>
        [ContextMenu("Testing/Test Positive Change (+10 All)")]
        private void TestPositiveChange()
        {
            Debug.Log("Testing positive change: +10 to all stats");
            UpdateStats(10, 10, 10, 10);
        }

        /// <summary>
        /// Tests negative stat change (-10 to all).
        /// </summary>
        [ContextMenu("Testing/Test Negative Change (-10 All)")]
        private void TestNegativeChange()
        {
            Debug.Log("Testing negative change: -10 to all stats");
            UpdateStats(-10, -10, -10, -10);
        }

        /// <summary>
        /// Forces Budget to 0 to test Game Over condition.
        /// </summary>
        [ContextMenu("Testing/Force Budget to 0")]
        private void ForceBudgetGameOver()
        {
            Debug.Log("Forcing Budget to 0 - should trigger Game Over");
            budget = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Time to 0")]
        private void ForceTimeGameOver()
        {
            Debug.Log("Forcing Time to 0 - should trigger Game Over");
            time = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Morale to 0")]
        private void ForceMoraleGameOver()
        {
            Debug.Log("Forcing Morale to 0 - should trigger Game Over");
            morale = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        [ContextMenu("Testing/Force Quality to 0")]
        private void ForceQualityGameOver()
        {
            Debug.Log("Forcing Quality to 0 - should trigger Game Over");
            quality = 0;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        /// <summary>
        /// Forces all stats to 100 to test Victory condition.
        /// </summary>
        [ContextMenu("Testing/Force All Stats to 100")]
        private void ForceVictory()
        {
            Debug.Log("Forcing all stats to 100 - should trigger Victory");
            budget = time = morale = quality = 100f;
            _onStatsChangedEvent?.Raise();
            CheckEndGameConditions();
        }

        #endregion
    }
}