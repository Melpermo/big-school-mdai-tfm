using HumanLoop.Events;
using System;
using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Global manager for the 4 main game statistics.
    /// </summary>
    public class GameStatsManager : MonoBehaviour
    {
        public static GameStatsManager Instance { get; private set; }

        [Header("Initial Values")]
        [Range(0, 100)] public float budget = 50f;
        [Range(0, 100)] public float time = 50f;
        [Range(0, 100)] public float morale = 50f;
        [Range(0, 100)] public float quality = 50f;

        [Header("Events")]
        [SerializeField] private GameEventSO onStatsChangedEvent;
        [SerializeField] private GameEventSO onGameOverEvent;

        private void Awake() => Instance = this;

        public void UpdateStats(float b, float t, float m, float q)
        {
            budget = Mathf.Clamp(budget + b, 0, 100);
            time = Mathf.Clamp(time + t, 0, 100);
            morale = Mathf.Clamp(morale + m, 0, 100);
            quality = Mathf.Clamp(quality + q, 0, 100);

            // Notify listeners that stats have changed
            onStatsChangedEvent.Raise();

            if (budget <= 0 || time <= 0 || morale <= 0 || quality <= 0)
            {
                onGameOverEvent.Raise();
            }            
        }        
    }
}