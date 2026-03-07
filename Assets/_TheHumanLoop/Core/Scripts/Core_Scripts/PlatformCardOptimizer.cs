#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using HumanLoop.UI;

namespace HumanLoop.Core
{
    public class PlatformCardOptimizer : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // Path where the settings asset should be located
        private const string SETTINGS_PATH = "Assets/_TheHumanLoop/Core/Data/BuildOptimizationSettings.asset";

        public void OnPreprocessBuild(BuildReport report)
        {
            // Load settings
            BuildOptimizationSettings settings = AssetDatabase.LoadAssetAtPath<BuildOptimizationSettings>(SETTINGS_PATH);

            if (settings == null)
            {
                Debug.LogWarning($"[PlatformCardOptimizer] BuildOptimizationSettings not found at {SETTINGS_PATH}. Skipping optimization.");
                return;
            }

            // Check if auto-optimization is disabled
            if (!settings.autoOptimizeCardFlipForWebGL)
            {
                if (settings.logOptimizationChanges)
                {
                    Debug.Log($"[PlatformCardOptimizer] Auto-optimization DISABLED. Skipping card flip changes.");
                }
                return;
            }

            // Determine flip animation state
            bool enableFlip;

            if (settings.useManualOverride)
            {
                enableFlip = settings.manualFlipAnimationEnabled;
                if (settings.logOptimizationChanges)
                {
                    Debug.Log($"[PlatformCardOptimizer] Using MANUAL override: flip={enableFlip}");
                }
            }
            else
            {
                enableFlip = report.summary.platform != BuildTarget.WebGL;
                if (settings.logOptimizationChanges)
                {
                    Debug.Log($"[PlatformCardOptimizer] Auto-optimizing for {report.summary.platform}: flip={enableFlip}");
                }
            }

            // Find all CardController instances in scenes
            CardController[] controllers = Object.FindObjectsByType<CardController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (controllers.Length == 0)
            {
                Debug.LogWarning("[PlatformCardOptimizer] No CardController instances found in scenes.");
                return;
            }

            // Apply optimization
            int modifiedCount = 0;
            foreach (var controller in controllers)
            {
                SerializedObject so = new SerializedObject(controller);
                SerializedProperty prop = so.FindProperty("enableFlipAnimation");

                if (prop != null)
                {
                    bool currentValue = prop.boolValue;
                    prop.boolValue = enableFlip;
                    so.ApplyModifiedProperties();

                    if (currentValue != enableFlip)
                    {
                        modifiedCount++;
                    }
                }
            }

            if (settings.logOptimizationChanges)
            {
                Debug.Log($"[PlatformCardOptimizer] Modified {modifiedCount}/{controllers.Length} CardControllers (flip={enableFlip})");
            }
        }
    }
}
#endif