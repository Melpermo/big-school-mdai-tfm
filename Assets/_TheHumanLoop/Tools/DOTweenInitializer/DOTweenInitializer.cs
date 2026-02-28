using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace TheHumanLoop.Tools
{
    /// <summary>
    /// Initializes and configures DOTween for the entire project.
    /// Optimized for WebGL with appropriate capacity settings.
    /// </summary>
    public class DOTweenInitializer : MonoBehaviour
    {
        public static DOTweenInitializer Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Enable to show DOTween initialization logs")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("Capacity Settings")]
        [Tooltip("Maximum number of individual tweens (animations)")]
        [SerializeField] private int maxTweens = 200;
        
        [Tooltip("Maximum number of sequences (grouped animations)")]
        [SerializeField] private int maxSequences = 50;

        [Header("WebGL Optimization")]
        [Tooltip("Clear tweens when loading new scenes")]
        [SerializeField] private bool clearOnSceneLoad = true;

        private void Awake()
        {
            InitializeSingleton();
            InitializeDOTween();
        }

        private void OnEnable()
        {
            if (clearOnSceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            if (clearOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        // Añade en la clase DOTweenInitializer
        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
    // Force cleanup cada 5 segundos en WebGL
    if (Time.frameCount % 300 == 0)
    {
        int before = DOTween.TotalPlayingTweens();
        if (before > 50)
        {
            DOTween.KillAll();
            DOTween.ClearCachedTweens();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.LogWarning($"[WebGL] Force cleanup: {before} tweens killed");
        }
    }
#endif
        }

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDOTween()
        {
            // Initialize DOTween with safe defaults
            DOTween.Init(
                recycleAllByDefault: true,    // Recycle tweens to reduce GC
                useSafeMode: true,             // Safe mode prevents errors
                logBehaviour: LogBehaviour.ErrorsOnly // Only log errors
            );

            #if UNITY_WEBGL
                // WebGL-specific optimizations
                ConfigureForWebGL();
            #else
                // Standard configuration for other platforms
                ConfigureStandard();
            #endif

            // Global settings
            DOTween.defaultAutoKill = true;      // Auto-destroy completed tweens
            DOTween.defaultRecyclable = true;    // Enable recycling by default
            DOTween.defaultEaseType = Ease.OutQuad; // Smoother default easing

            if (showDebugLogs)
            {
                LogConfiguration();
            }
        }

        private void ConfigureForWebGL()
        {
            // Reduced capacity for WebGL to save memory
            DOTween.SetTweensCapacity(maxTweens, maxSequences);
            
            // Additional WebGL optimizations
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.useSmoothDeltaTime = true; // Better for WebGL frame consistency

            if (showDebugLogs)
            {
                Debug.Log($"[DOTween] WebGL Mode - Capacity: {maxTweens} tweens, {maxSequences} sequences");
            }
        }

        private void ConfigureStandard()
        {
            // Higher capacity for standalone builds
            int tweenCapacity = maxTweens * 2;
            int sequenceCapacity = maxSequences * 2;
            
            DOTween.SetTweensCapacity(tweenCapacity, sequenceCapacity);

            if (showDebugLogs)
            {
                Debug.Log($"[DOTween] Standard Mode - Capacity: {tweenCapacity} tweens, {sequenceCapacity} sequences");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Clear tweens to prevent carryover between scenes
            ClearAllTweens();

            if (showDebugLogs)
            {
                Debug.Log($"[DOTween] Cleared tweens on scene load: {scene.name}");
            }
        }

        /// <summary>
        /// Clears all active tweens and cached data.
        /// Call this when transitioning between major game states.
        /// </summary>
        public void ClearAllTweens()
        {
            DOTween.KillAll();
            DOTween.ClearCachedTweens();
        }

        /// <summary>
        /// Complete reset of DOTween (use sparingly, e.g., before returning to main menu).
        /// </summary>
        public void FullReset()
        {
            DOTween.KillAll();
            DOTween.Clear(destroy: true);
            DOTween.ClearCachedTweens();

            if (showDebugLogs)
            {
                Debug.Log("[DOTween] Full reset completed");
            }
        }

        private void LogConfiguration()
        {
            Debug.Log("=== DOTween Configuration ===\n" +
                     $"Platform: {Application.platform}\n" +
                     $"Max Tweens: {maxTweens}\n" +
                     $"Max Sequences: {maxSequences}\n" +
                     $"Auto Kill: {DOTween.defaultAutoKill}\n" +
                     $"Recyclable: {DOTween.defaultRecyclable}\n" +
                     $"Default Ease: {DOTween.defaultEaseType}");
        }

        #region Context Menu Testing

        [ContextMenu("Debug/Log Active Tweens Count")]
        private void LogActiveTweensCount()
        {
            Debug.Log($"[DOTween] Active Tweens: {DOTween.TotalPlayingTweens()} / {maxTweens}");
        }

        [ContextMenu("Debug/Clear All Tweens")]
        private void DebugClearAllTweens()
        {
            ClearAllTweens();
            Debug.Log("[DOTween] All tweens cleared manually");
        }

        [ContextMenu("Debug/Full Reset")]
        private void DebugFullReset()
        {
            FullReset();
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                // Clean up when singleton is destroyed
                ClearAllTweens();
                Instance = null;
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure sensible values in editor
            maxTweens = Mathf.Max(50, maxTweens);
            maxSequences = Mathf.Max(10, maxSequences);
        }
        #endif
    }
}
