using UnityEngine;
using HumanLoop.Core;
using HumanLoop.Events;

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

        [Header("Condition Parameters")]
        [Tooltip("Threshold value for Budget stat")]
        public float budgetThreshold;
        
        [Tooltip("Threshold value for Time stat")]
        public float timeThreshold;
        
        [Tooltip("Threshold value for Morale stat")]
        public float moraleThreshold;
        
        [Tooltip("Threshold value for Quality stat")]
        public float qualityThreshold;

        [Header("Event to Raise")]
        [Tooltip("The GameEvent that will be raised when this condition is met")]
        public GameEventSO endGameConditionEvent;

        [Header("UI Feedback")]
        [Tooltip("Message to display when this condition is met")]
        [TextArea(3, 5)]
        public string endGameMessage;
        
        [Tooltip("Background image to show when this condition is met")]
        public Sprite endGameBG_image;

        /// <summary>
        /// Checks if this end game condition is currently met based on game stats.
        /// </summary>
        /// <returns>True if the condition is met, false otherwise</returns>
        public bool CheckCondition()
        {
            var stats = GameStatsManager.Instance;
            
            if (stats == null)
            {
                Debug.LogError("GameStatsManager.Instance is null! Cannot check end game condition.");
                return false;
            }

            bool conditionMet = false;

            switch (conditionType)
            {
                case ConditionType.Victory:
                    // All stats must meet or exceed thresholds
                    conditionMet = stats.budget >= budgetThreshold &&
                                  stats.time >= timeThreshold &&
                                  stats.morale >= moraleThreshold &&
                                  stats.quality >= qualityThreshold;                    
                    break;

                case ConditionType.GameOver:
                    // Any stat at or below threshold triggers game over
                    conditionMet = stats.budget <= budgetThreshold &&
                                  stats.time <= timeThreshold &&
                                  stats.morale <= moraleThreshold &&
                                  stats.quality <= qualityThreshold;                    
                    break;

                case ConditionType.SpecialMet:
                    // All stats must exceed thresholds (stricter than Victory)
                    conditionMet = stats.budget >= budgetThreshold &&
                                  stats.time >= timeThreshold &&
                                  stats.morale >= moraleThreshold &&
                                  stats.quality >= qualityThreshold;                    
                    break;

                case ConditionType.SpecialFailed:
                    // Any stat below threshold triggers special failure
                    conditionMet = stats.budget <= budgetThreshold ||
                                  stats.time <= timeThreshold ||
                                  stats.morale <= moraleThreshold ||
                                  stats.quality <= qualityThreshold;
                    break;

                default:
                    Debug.LogWarning($"Unknown condition type: {conditionType}");
                    break;
            }


            //DebugConditionType(stats, conditionMet);           

            return conditionMet;
        }

        private void DebugConditionType(GameStatsManager stats, bool conditionMet)
        {
            Debug.Log($"Checking Victory condition '{conditionName}': " +
                                          $"Budget={stats.budget} (threshold {budgetThreshold}), " +
                                          $"Time={stats.time} (threshold {timeThreshold}), " +
                                          $"Morale={stats.morale} (threshold {moraleThreshold}), " +
                                          $"Quality={stats.quality} (threshold {qualityThreshold}) => Condition Met: {conditionMet}");
        }

        /// <summary>
        /// Raises the associated game event for this condition.
        /// Should be called when CheckCondition() returns true.
        /// </summary>
        public void RaiseEvent()
        {
            if (endGameConditionEvent != null)
            {
                Debug.Log($"Raising event for condition: {conditionName} ({conditionType})");
                endGameConditionEvent.Raise();
            }
            else
            {
                Debug.LogWarning($"No event assigned for condition: {conditionName}");
            }
        }

        #region Validation

        /// <summary>
        /// Validates that all required fields are properly configured.
        /// Called from the Inspector via a custom editor.
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(conditionName))
            {
                Debug.LogWarning($"EndGameConditionSO: conditionName is empty", this);
                isValid = false;
            }

            if (endGameConditionEvent == null)
            {
                Debug.LogWarning($"EndGameConditionSO '{conditionName}': No event assigned!", this);
                isValid = false;
            }

            if (string.IsNullOrEmpty(endGameMessage))
            {
                Debug.LogWarning($"EndGameConditionSO '{conditionName}': endGameMessage is empty", this);
                isValid = false;
            }

            return isValid;
        }

        #endregion
    }
}
