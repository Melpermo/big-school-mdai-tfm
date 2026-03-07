using UnityEngine;

namespace HumanLoop.Core
{
    /// <summary>
    /// Configuration for build-time optimizations.
    /// Controls whether platform-specific optimizations are applied automatically.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildOptimizationSettings", menuName = "The Human Loop/Build/Optimization Settings")]
    public class BuildOptimizationSettings : ScriptableObject
    {
        [Header("Card Animation Optimization")]
        [Tooltip("If true, automatically disables flip animation for WebGL builds")]
        public bool autoOptimizeCardFlipForWebGL = false;

        [Space(10)]
        [Tooltip("Manual override: Force flip animation on/off regardless of platform")]
        public bool useManualOverride = false;

        [Tooltip("Used only if useManualOverride is true")]
        public bool manualFlipAnimationEnabled = true;

        [Header("Debug")]
        [Tooltip("Log optimization changes during build")]
        public bool logOptimizationChanges = true;
    }
}