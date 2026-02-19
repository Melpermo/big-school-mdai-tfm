using UnityEngine;
using HumanLoop.Core;
using HumanLoop.AudioSystem;

namespace HumanLoop.Data
{
    [CreateAssetMenu(fileName = "EndGameConditionSO", menuName = "The Human Loop/EndGameCondition/EndGameConditionSO")]
    public class EndGameConditionSO : ScriptableObject
    {
        public string conditionName;

        public enum ConditionType { Victory, GameOver, SpecialMet, SpecialFailed }
        public ConditionType conditionType;

        [Header("Condition Parameters")]
        public float budgetThreshold;
        public float timeThreshold;
        public float moraleThreshold;
        public float qualityThreshold;

        [Header("UI Feedback")]
        [Multiline]
        public string endGameMessage;
        public Sprite endGameBG_image;        


        public bool CheckCondition()
        {
            var stats = GameStatsManager.Instance;
            bool conditionMet = false;
    
            switch (conditionType)
            {
                case ConditionType.Victory:
                    conditionMet = stats.budget >= budgetThreshold &&
                    stats.time >= timeThreshold &&
                    stats.morale >= moraleThreshold &&
                    stats.quality >= qualityThreshold;
                break;
    
                case ConditionType.GameOver:
                    conditionMet = stats.budget <= budgetThreshold ||
                    stats.time <= timeThreshold ||
                    stats.morale <= moraleThreshold ||
                    stats.quality <= qualityThreshold;
                break;
    
                case ConditionType.SpecialMet:
                    conditionMet = stats.budget >= budgetThreshold &&
                    stats.time >= timeThreshold &&
                    stats.morale >= moraleThreshold &&
                    stats.quality >= qualityThreshold;
                break;
    
                case ConditionType.SpecialFailed:
                    conditionMet = stats.budget <= budgetThreshold ||
                    stats.time <= timeThreshold ||
                    stats.morale <= moraleThreshold ||
                    stats.quality <= qualityThreshold;
                break;
            }
    
            return conditionMet;
        }

    }
}
