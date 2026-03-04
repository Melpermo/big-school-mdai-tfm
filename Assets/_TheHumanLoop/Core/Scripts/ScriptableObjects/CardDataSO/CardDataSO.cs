using UnityEngine;
using System.Collections.Generic;

namespace HumanLoop.Data
{
    public abstract class CardDataSO : ScriptableObject
    {
        [Header("General Info")]
        public string cardName;
        public CardCategory category;

        [TextArea(3, 5)]
        public string description;

        [Header("Visuals")]
        public Sprite cardArt;

        [Header("Swipe Options")]
        public string leftChoiceText;
        public string rightChoiceText;

        [Header("Stats Impact")]
        public StatEffect leftSwipeImpact;
        public StatEffect rightSwipeImpact;

        [Header("Spawn Conditions")]
        public bool useConditions;
        public List<SpawnCondition> conditions;

        [Header("Forced Sequels")]
        public CardDataSO nextCardLeft;
        public CardDataSO nextCardRight;

        [System.Serializable]
        public struct StatEffect
        {
            public float budget;
            public float time;
            public float morale;
            public float quality;
        }

        [System.Serializable]
        public struct SpawnCondition
        {
            public enum StatType { Budget, Time, Morale, Quality }
            public enum Comparison { GreaterThan, LessThan }

            public StatType stat;
            public Comparison comparison;
            public float value;
        }        

        /// <summary>
        /// Changed from 'abstract' to 'virtual' to provide a base implementation.
        /// </summary>
        public virtual void ApplyEffect(bool isRightSwipe)
        {
            // Select the correct impact based on swipe direction
            StatEffect impact = isRightSwipe ? rightSwipeImpact : leftSwipeImpact;

            // Send the data to the Singleton Manager
            if (Core.GameStatsManager.Instance != null)
            {
                Core.GameStatsManager.Instance.UpdateStats(
                    impact.budget,
                    impact.time,
                    impact.morale,
                    impact.quality
                );
            }
            else
            {
                //Debug.LogError("ApplyEffect: GameStatsManager.Instance is missing in the scene!");
                return;
            }
        }

        /// <summary>
        /// Checks if the current game stats meet the requirements for this card to spawn.
        /// </summary>
        public bool CanSpawn(float b, float t, float m, float q)
        {
            if (!useConditions) return true;

            foreach (var cond in conditions)
            {
                float currentVal = cond.stat switch
                {
                    SpawnCondition.StatType.Budget => b,
                    SpawnCondition.StatType.Time => t,
                    SpawnCondition.StatType.Morale => m,
                    _ => q
                };

                if (cond.comparison == SpawnCondition.Comparison.GreaterThan && currentVal <= cond.value) return false;
                if (cond.comparison == SpawnCondition.Comparison.LessThan && currentVal >= cond.value) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the forced next card based on the decision. Can be null.
        /// </summary>
        public CardDataSO GetForcedNextCard(bool isRightSwipe)
        {
            return isRightSwipe ? nextCardRight : nextCardLeft;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Advertir si el texto es muy largo
            if (description != null && description.Length > 200)
            {
                Debug.LogWarning($"[{cardName}] Description muy larga ({description.Length} chars). Recomendado: <200");
            }
        }
#endif
    }
}
