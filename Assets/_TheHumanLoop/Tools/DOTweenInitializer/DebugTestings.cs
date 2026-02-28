using DG.Tweening;
using HumanLoop.Core;
using HumanLoop.Data;
using HumanLoop.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheHumanLoop.Tools
{
    /// <summary>
    /// Global debug utilities for DOTween health monitoring.
    /// Place this on a GameObject in the scene for testing.
    /// </summary>
    public class DebugTestings : MonoBehaviour
    {
        [Header("Monitoring")]
        [SerializeField] private bool continuousMonitoring = false;
        [SerializeField] private float monitoringInterval = 1f;

        [Header("Test Data")]
        [Tooltip("Card data used for pool stress testing")]
        [SerializeField] private CardDataSO testCardData; // ← AÑADIR ESTO

        private void Update()
        {
            if (continuousMonitoring && Time.frameCount % (int)(monitoringInterval * 60) == 0)
            {
                CheckDOTweenHealth();
            }
        }

        [ContextMenu("Test/Check DOTween Health")]
        private void CheckDOTweenHealth()
        {
            if (DOTweenInitializer.Instance == null)
            {
                Debug.LogWarning("⚠️ DOTweenInitializer no encontrado en la escena!");
                return;
            }

            int activeTweens = DOTween.TotalPlayingTweens();
            int totalTweens = DOTween.TweensById(null, false).Count;

            Debug.Log($"=== DOTween Health Check ===\n" +
                     $"Active Tweens: {activeTweens}\n" +
                     $"Total Tweens: {totalTweens}\n" +
                     $"Status: {GetHealthStatus(activeTweens)}");

            if (activeTweens > 150)
            {
                Debug.LogWarning("⚠️ Muchos tweens activos! Posible leak.");
                LogActiveTweenDetails();
            }
        }

        private string GetHealthStatus(int activeTweens)
        {
            if (activeTweens < 50) return "<color=green>✓ Saludable</color>";
            if (activeTweens < 100) return "<color=yellow>⚠ Moderado</color>";
            return "<color=red>✗ Crítico</color>";
        }

        private void LogActiveTweenDetails()
        {
            // Intenta obtener información de tweens activos
            var tweens = DOTween.PlayingTweens();
            if (tweens != null)
            {
                Debug.Log($"Detalles de tweens activos: {tweens.Count} encontrados");
                // Puedes añadir más detalles si necesitas
            }
        }

        [ContextMenu("Test/Force Clear All Tweens")]
        private void ForceClearAllTweens()
        {
            int before = DOTween.TotalPlayingTweens();
            DOTween.KillAll();
            DOTween.ClearCachedTweens();
            int after = DOTween.TotalPlayingTweens();

            Debug.Log($"Tweens limpiados: {before} → {after}");
        }

        [ContextMenu("Test/Log Memory Stats")]
        private void LogMemoryStats()
        {
            long totalMemory = System.GC.GetTotalMemory(false);
            Debug.Log($"=== Memory Stats ===\n" +
                     $"Total Memory: {totalMemory / 1024 / 1024} MB\n" +
                     $"Active Tweens: {DOTween.TotalPlayingTweens()}");
        }

        [ContextMenu("Test/Stress Test Card Pool")]
        private void TestCardPoolStress()
        {
            StartCoroutine(TestCardPoolStressCoroutine());
        }

        private IEnumerator TestCardPoolStressCoroutine()
        {
            var factory = FindObjectOfType<CardFactory>();
            if (factory == null)
            {
                Debug.LogError("CardFactory not found!");
                yield break;
            }

            // Validar que testCardData está asignado
            if (testCardData == null)
            {
                Debug.LogError("testCardData is not assigned! Assign a CardDataSO in the Inspector.");
                yield break;
            }

            // Simula uso intensivo
            for (int cycle = 0; cycle < 5; cycle++)
            {
                Debug.Log($"=== Cycle {cycle + 1}/5 ===");

                List<CardDisplay> cards = new List<CardDisplay>();

                // Crear máximo de cartas
                for (int i = 0; i < 15; i++)
                {
                    var card = factory.GetCardFromPool(testCardData); // Ahora funciona
                    if (card != null)
                    {
                        cards.Add(card);
                    }
                    yield return null;
                }

                Debug.Log($"Created: {cards.Count}, " +
                         $"InUse: {factory.GetInUseCount()}, " +
                         $"Available: {factory.GetAvailableCount()}");

                yield return new WaitForSeconds(0.5f);

                // Devolver todas
                foreach (var card in cards)
                {
                    factory.ReturnToPool(card);
                    yield return null;
                }

                Debug.Log($"After return - InUse: {factory.GetInUseCount()}, " +
                         $"Available: {factory.GetAvailableCount()}");

                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log($"<color=green>✓ Pool stress test complete</color>\n" +
                     $"Peak Usage: {factory.GetPeakUsage()}, " +
                     $"Total Created: {factory.GetTotalCreated()}");
        }

        [ContextMenu("Test/Check for Memory Leaks")]
        private void TestMemoryLeaks()
        {
            StartCoroutine(TestMemoryLeaksCoroutine());
        }

        private IEnumerator TestMemoryLeaksCoroutine()
        {
            var handler = FindObjectOfType<EndGameUIHandler>();
            if (handler == null)
            {
                Debug.LogError("EndGameUIHandler not found!");
                yield break;
            }

            // Test victory infinite loop cleanup
            Debug.Log("=== Testing Victory Sequence ===");

            EndGameConditionSO victoryCondition = ScriptableObject.CreateInstance<EndGameConditionSO>();
            victoryCondition.conditionName = "Test Victory";
            victoryCondition.conditionType = EndGameConditionSO.ConditionType.Victory;

            handler.SelectConditionTypeToShow(victoryCondition);

            yield return new WaitForSeconds(3f);

            int tweensBeforeHide = DOTween.TotalPlayingTweens();
            Debug.Log($"Tweens before hide: {tweensBeforeHide}");

            handler.HideUI();

            yield return new WaitForSeconds(0.5f);

            int tweensAfterHide = DOTween.TotalPlayingTweens();
            Debug.Log($"Tweens after hide: {tweensAfterHide}");

            if (tweensAfterHide < tweensBeforeHide)
            {
                Debug.Log("<color=green>✓ No memory leak detected</color>");
            }
            else
            {
                Debug.LogWarning("<color=red>⚠️ Possible memory leak!</color>");
            }
        }

        [ContextMenu("Test/Game Over Stress Test")]
        private void TestGameOverStress()
        {
            StartCoroutine(TestGameOverStressCoroutine());
        }

        private IEnumerator TestGameOverStressCoroutine()
        {
            var handler = FindObjectOfType<GameOverUIHandler>();
            if (handler == null)
            {
                Debug.LogError("GameOverUIHandler not found!");
                yield break;
            }

            // Show/Hide repeatedly
            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"=== Cycle {i + 1}/10 ===");

                // Show
                handler.HandleGameOver();
                yield return new WaitForSecondsRealtime(1f);

                int tweensAfterShow = DOTween.TotalPlayingTweens();
                Debug.Log($"Tweens after show: {tweensAfterShow}");

                // Hide (simulate)
                handler.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.2f);

                int tweensAfterHide = DOTween.TotalPlayingTweens();
                Debug.Log($"Tweens after hide: {tweensAfterHide}");

                handler.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f);
            }

            Debug.Log("<color=green>✓ Game Over stress test complete</color>");
        }

        [ContextMenu("Test/WebGL Memory Stress Test")]
        private void TestWebGLMemory()
        {
            StartCoroutine(WebGLMemoryStressTestCoroutine());
        }

        private IEnumerator WebGLMemoryStressTestCoroutine()
        {
            var factory = FindObjectOfType<CardFactory>();
            
            for (int cycle = 0; cycle < 50; cycle++)
            {
                Debug.Log($"=== Cycle {cycle + 1}/50 ===");
                
                // Simula juego
                yield return new WaitForSeconds(2f);
                
                // Stats
                long memory = System.GC.GetTotalMemory(false);
                int tweens = DOTween.TotalPlayingTweens();
                
                Debug.Log($"Memory: {memory / 1048576}MB, " +
                         $"Tweens: {tweens}, " +
                         $"InUse: {factory.GetInUseCount()}");
                
                if (memory > 500000000) // 500MB
                {
                    Debug.LogError($"⚠️ HIGH MEMORY at cycle {cycle}!");
                }
            }
        }
    }
}
