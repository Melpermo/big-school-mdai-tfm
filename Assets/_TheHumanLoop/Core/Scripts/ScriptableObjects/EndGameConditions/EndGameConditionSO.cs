using HumanLoop.Core;
using HumanLoop.Events;
using UnityEngine;


namespace HumanLoop.Data
{
    /// <summary>
    /// Defines an end game condition and the event to raise when it's met.
    /// </summary>
    [CreateAssetMenu(fileName = "EndGameConditionSO", menuName = "The Human Loop/EndGameCondition/EndGameConditionSO")]
    public class EndGameConditionSO : ScriptableObject
    {
        [Header("Condition Info")]
        [Tooltip("Display name for this end game condition")]
        public string conditionName;

        [Tooltip("Type of end game condition")]
        public ConditionType conditionType;

        public enum ConditionType { Victory, GameOver, SpecialMet, SpecialFailed }

        [Header("Stat to Check")]
        [Tooltip("Which stat(s) should this condition evaluate? Use 'All' for Victory conditions")]
        public StatToCheck statToCheck = StatToCheck.All;

        public enum StatToCheck { All, Budget, Time, Morale, Quality }

        [Header("Condition Parameters")]
        [Tooltip("Threshold for comparison (meaning depends on condition type)")]
        public float threshold;

        // Campos legacy para condiciones que evalúan múltiples stats
        [HideInInspector] public float budgetThreshold;
        [HideInInspector] public float timeThreshold;
        [HideInInspector] public float moraleThreshold;
        [HideInInspector] public float qualityThreshold;        
        

        [Header("Event to Raise")]
        [Tooltip("The GameEvent that will be raised when this condition is met")]
        public GameEventSO endGameConditionEvent;

        [Header("UI Feedback")]
        [Tooltip("Message to display when this condition is met")]
        [TextArea(3, 5)]
        public string endGameMessage;
        
        [Tooltip("Background image to show when this condition is met")]
        public Sprite endGameBG_image;

        [Header("Localizations IDs")]
        [SerializeField] private string conditionNameID;
        [SerializeField] private string endGameMessageID;        

        public string ConditionNameID => conditionNameID;
        public string EndGameMessageID => endGameMessageID;        

        /// <summary>
        /// Checks if this end game condition is currently met based on game stats.
        /// </summary>
        /// <returns>True if the condition is met, false otherwise</returns>
        public bool CheckCondition()
        {
            var stats = GameStatsManager.Instance;          


            if (stats == null)
            {
                Debug.LogError("GameStatsManager.Instance is null!");
                return false;
            }

            bool conditionMet = false;

            switch (conditionType)
            {
                case ConditionType.Victory:
                    conditionMet = CheckVictoryCondition(stats);                    
                    break;

                case ConditionType.GameOver:
                    conditionMet = CheckGameOverCondition(stats);
                    break;

                case ConditionType.SpecialMet:
                    conditionMet = CheckVictoryCondition(stats);
                    break;

                case ConditionType.SpecialFailed:
                    conditionMet = CheckGameOverCondition(stats);
                    break;
            }

            return conditionMet;
        }

        /// <summary>
        /// Checks Victory condition based on which stat(s) to monitor.
        /// </summary>
        private bool CheckVictoryCondition(GameStatsManager stats)
        {
                      
            /*
            // Log detallado ANTES de evaluar
            Debug.Log($"<color=magenta>[CheckVictoryCondition] Evaluating: {conditionName}</color>");
            Debug.Log($"  ConditionType: {conditionType}");
            Debug.Log($"  StatToCheck: {statToCheck}");
            Debug.Log($"  Threshold: {threshold}");*/

            switch (statToCheck)
            {
                case StatToCheck.Budget:
                    return stats.budget >= threshold;

                case StatToCheck.Time:
                    return stats.time >= threshold;

                case StatToCheck.Morale:
                    return stats.morale >= threshold;

                case StatToCheck.Quality:
                    return stats.quality >= threshold;

                case StatToCheck.All:
                    bool b = stats.budget >= threshold;
                    bool t = stats.time >= threshold;
                    bool m = stats.morale >= threshold;
                    bool q = stats.quality >= threshold;
                    return b && t && m && q;

                default:
                    //Debug.LogWarning($"Unknown StatToCheck: {statToCheck}");
                    return false;
            }
        }

        /// <summary>
        /// Checks Game Over condition based on which stat is being monitored.
        /// </summary>
        private bool CheckGameOverCondition(GameStatsManager stats)
        {
            switch (statToCheck)
            {
                case StatToCheck.Budget:
                    return stats.budget <= threshold;

                case StatToCheck.Time:
                    return stats.time <= threshold;

                case StatToCheck.Morale:
                    return stats.morale <= threshold;

                case StatToCheck.Quality:
                    return stats.quality <= threshold;

                case StatToCheck.All:
                    // Cualquier stat por debajo del threshold
                    return stats.budget <= threshold ||
                           stats.time <= threshold ||
                           stats.morale <= threshold ||
                           stats.quality <= threshold;

                default:
                    //Debug.LogWarning($"Unknown StatToCheck: {statToCheck}");
                    return false;
            }
        }

        /// <summary>
        /// Raises the associated game event for this condition.
        /// Should be called when CheckCondition() returns true.
        /// </summary>
        public void RaiseEvent()
        {
            if (endGameConditionEvent != null)
            {
                //Debug.Log($"Raising event for condition: {conditionName} ({conditionType})");
                endGameConditionEvent.Raise();
            }
            else
            {
                //Debug.LogWarning($"No event assigned for condition: {conditionName}");
                return;
            }
        }

#if (UNITY_EDITOR)
        #region Validation

        /// <summary>
        /// Debug method to log all threshold values for this condition.
        /// </summary>
        [ContextMenu("Debug/Log Threshold Values")]
        public void LogThresholdValues()
        {
            Debug.Log($"=== CONDITION: {conditionName} ({conditionType}) ===\n" +
                     $"Stat To Check: {statToCheck}\n" +
                     $"Threshold (new): {threshold}\n" +
                     $"--- Legacy Thresholds (hidden) ---\n" +
                     $"budgetThreshold: {budgetThreshold}\n" +
                     $"timeThreshold: {timeThreshold}\n" +
                     $"moraleThreshold: {moraleThreshold}\n" +
                     $"qualityThreshold: {qualityThreshold}");
        }

        /// <summary>
        /// Validates that all required fields are properly configured.
        /// Called from the Inspector via a custom editor.
        /// </summary>
        [ContextMenu("Validate Configuration")]
        public void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(conditionName))
            {
                Debug.LogWarning("conditionName is empty", this);
            }

            if (endGameConditionEvent == null)
            {
                Debug.LogWarning($"No event assigned for '{conditionName}'", this);
            }

            // Validación específica para cada tipo
            if (conditionType == ConditionType.Victory || conditionType == ConditionType.SpecialMet)
            {
                if (threshold == 0)
                {
                    Debug.LogWarning($"'{conditionName}': Victory/SpecialMet condition has threshold = 0. Is this intended?", this);
                }
            }

            if (conditionType == ConditionType.GameOver || conditionType == ConditionType.SpecialFailed)
            {
                if (statToCheck == StatToCheck.All)
                {
                    Debug.LogWarning($"'{conditionName}': GameOver/SpecialFailed with 'All' stats may cause conflicts. Consider specifying a single stat.", this);
                }
            }

            Debug.Log($"✓ Condition '{conditionName}' validated", this);
        }

        #endregion
#endif
    }
}
