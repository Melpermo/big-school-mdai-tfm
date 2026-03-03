using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TheHumanLoop.Tools
{
    /// <summary>
    /// Manages aggressive cleanup when loading/unloading scenes to prevent memory leaks in WebGL.
    /// </summary>
    public class SceneCleanupManager : MonoBehaviour
    {
        public static SceneCleanupManager Instance { get; private set; }

        [Header("Cleanup Settings")]
        [SerializeField] private bool aggressiveCleanup = true;
        [SerializeField] private float cleanupDelay = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (showDebugLogs)
            {
                long memoryBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneCleanup] Scene unloaded: {scene.name}, Memory: {memoryBefore / 1048576}MB");
            }

            if (aggressiveCleanup)
            {
                StartCoroutine(AggressiveCleanupCoroutine());
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (showDebugLogs)
            {
                long memoryBefore = System.GC.GetTotalMemory(false);
                Debug.Log($"[SceneCleanup] Scene loaded: {scene.name}, Memory: {memoryBefore / 1048576}MB");
            }
        }

        private IEnumerator AggressiveCleanupCoroutine()
        {
            yield return new WaitForSeconds(cleanupDelay);

            long memoryBefore = System.GC.GetTotalMemory(false);

            // Step 1: Unload unused assets (textures, audio, etc.)
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            yield return unloadOp;

            // Step 2: Force garbage collection (multiple passes)
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long memoryAfter = System.GC.GetTotalMemory(false);
            long freed = memoryBefore - memoryAfter;

            if (showDebugLogs)
            {
                Debug.Log($"[SceneCleanup] Cleanup complete:\n" +
                         $"  Before: {memoryBefore / 1048576}MB\n" +
                         $"  After: {memoryAfter / 1048576}MB\n" +
                         $"  Freed: {freed / 1048576}MB");
            }
        }

        /// <summary>
        /// Call this manually before loading a new scene for immediate cleanup.
        /// </summary>
        public void CleanupBeforeSceneLoad()
        {
            if (showDebugLogs)
            {
                Debug.Log("[SceneCleanup] Manual cleanup triggered");
            }

            StartCoroutine(AggressiveCleanupCoroutine());
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Force Cleanup Now")]
        private void DebugForceCleanup()
        {
            StartCoroutine(AggressiveCleanupCoroutine());
        }

        [ContextMenu("Debug/Log Memory Stats")]
        private void DebugLogMemory()
        {
            long memory = System.GC.GetTotalMemory(false);
            Debug.Log($"[SceneCleanup] Current Memory: {memory / 1048576}MB");
        }
#endif
    }
}