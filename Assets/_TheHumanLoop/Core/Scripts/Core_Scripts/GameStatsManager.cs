using HumanLoop.Events;
using HumanLoop.Data;   
using System;
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

        [Header("End Game Conditions")]
        [Tooltip("List of conditions that trigger victory, defeat, or special outcomes")]
        [SerializeField] private List<EndGameConditionSO> endGameConditionsList;

        [Header("Events")]
        [Tooltip("Raised whenever any stat value changes")]
        [SerializeField] private GameEventSO _onStatsChangedEvent;
        
        [Tooltip("Raised when a Game Over condition is met (any stat reaches 0)")]
        [SerializeField] private GameEventSO _onGameOverEvent;
        
        [Tooltip("Raised when the player wins (typically 30 turns survived)")]
        [SerializeField] private GameEventSO _onVictoryEvent;
        
        [Tooltip("Raised when a special positive condition is met")]
        [SerializeField] private GameEventSO _onSpecialConditionMetEvent;
        
        [Tooltip("Raised when a special negative condition is met")]
        [SerializeField] private GameEventSO _onSpecialConditionFailedEvent;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
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
            _onStatsChangedEvent.Raise();

            // Check if any end game condition has been triggered
            CheckConditions();
        }

        /// <summary>
        /// Main entry point for condition checking. Called after every stat update.
        /// </summary>
        private void CheckConditions()
        {
            CheckConditionTypesOnList();
        }

        /// <summary>
        /// Iterates through all registered end game conditions and evaluates them.
        /// </summary>
        private void CheckConditionTypesOnList()
        { 
            foreach (var endGameCondition in endGameConditionsList)
            {
                // Use the ScriptableObject's internal CheckCondition method
                CheckIsConditionsFit_2();
            }
        }

        /// <summary>
        /// Evaluates each condition using the SO's CheckCondition() method.
        /// If a condition is met, raises the appropriate event based on condition type.
        /// </summary>
        private void CheckIsConditionsFit_2()
        { 
            foreach (var endGameCondition in endGameConditionsList)
            {
                // Let the ScriptableObject determine if its condition is met
                if (endGameCondition.CheckCondition() == true)
                { 
                    // Raise the appropriate event based on condition type
                    switch(endGameCondition.conditionType)
                    {
                        case EndGameConditionSO.ConditionType.Victory:
                            Debug.Log("Victory condition met! Using method on SO");
                            _onVictoryEvent.Raise();
                            break;

                        case EndGameConditionSO.ConditionType.GameOver:
                            Debug.Log("Game Over condition met! Using method on SO");
                            _onGameOverEvent.Raise();
                            break;

                        case EndGameConditionSO.ConditionType.SpecialMet:
                            Debug.Log("Special condition met! Using method on SO");
                            _onSpecialConditionMetEvent.Raise();
                            break;

                        case EndGameConditionSO.ConditionType.SpecialFailed:
                            Debug.Log("Special condition failed! Using method on SO");
                            _onSpecialConditionFailedEvent.Raise();
                            break;
                    }
                }
            }
        }

        #region Deprecated - Old Condition Checking Logic
        
        // NOTE: This method is deprecated. Condition logic is now encapsulated in EndGameConditionSO.CheckCondition()
        // Kept for reference in case rollback is needed during development.
        
        /// <summary>
        /// [DEPRECATED] Old method that checked conditions directly in the manager.
        /// Now replaced by delegating to EndGameConditionSO.CheckCondition().
        /// </summary>
        private void CheckIsConditionsFit(EndGameConditionSO endGameConditionSO)
        { 
            switch (endGameConditionSO.conditionType)
            {
                case EndGameConditionSO.ConditionType.Victory:
                    // Victory: All stats must meet or exceed thresholds
                    if (budget > endGameConditionSO.budgetThreshold && 
                        time >= endGameConditionSO.timeThreshold && 
                        morale >= endGameConditionSO.moraleThreshold && 
                        quality >= endGameConditionSO.qualityThreshold)
                    {
                        Debug.Log("Victory condition met!");
                        _onVictoryEvent.Raise();
                    }                    
                    break;

                case EndGameConditionSO.ConditionType.GameOver:
                    // Game Over: Any stat falls below threshold
                    if (budget <= endGameConditionSO.budgetThreshold || 
                        time <= endGameConditionSO.timeThreshold || 
                        morale <= endGameConditionSO.moraleThreshold || 
                        quality <= endGameConditionSO.qualityThreshold)
                    {
                        Debug.Log("Game Over condition met!");
                        _onGameOverEvent.Raise();
                    }
                    break;

                case EndGameConditionSO.ConditionType.SpecialMet:
                    // Special positive outcome: All stats exceed thresholds
                    if (budget > endGameConditionSO.budgetThreshold && 
                        time > endGameConditionSO.timeThreshold && 
                        morale > endGameConditionSO.moraleThreshold && 
                        quality > endGameConditionSO.qualityThreshold)
                    {
                        Debug.Log("Special condition met!");
                        _onSpecialConditionMetEvent.Raise();
                    }
                    break;

                case EndGameConditionSO.ConditionType.SpecialFailed:
                    // Special negative outcome: Any stat falls below threshold
                    if (budget < endGameConditionSO.budgetThreshold || 
                        time < endGameConditionSO.timeThreshold || 
                        morale < endGameConditionSO.moraleThreshold || 
                        quality < endGameConditionSO.qualityThreshold)
                    {
                        Debug.Log("Special condition failed!");
                        _onSpecialConditionFailedEvent.Raise();
                    }
                    break;
            }
        }
        
        #endregion
    }
}