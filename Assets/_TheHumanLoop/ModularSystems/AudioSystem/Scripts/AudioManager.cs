using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanLoop.AudioSystem
{
    /// <summary>
    /// Manages audio playback with object pooling.
    /// Optimized for WebGL with bounded pool size.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music Management")]
        [SerializeField] private AudioSource _musicSourceA;
        [SerializeField] private AudioSource _musicSourceB;
        [SerializeField] private float _defaultFadeDuration = 2.0f;

        private bool _isUsingSourceA = true;

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private int _maxPoolSize = 50; // ← NUEVO: límite máximo
        [SerializeField] private GameObject _audioSourcePrefab;

        private List<AudioSource> _sourcePool;
        private Coroutine _cleanupCoroutine;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_cleanupCoroutine != null)
            {
                StopCoroutine(_cleanupCoroutine);
            }
        }

        #endregion

        #region Pool Management

        private void InitializePool()
        {
            _sourcePool = new List<AudioSource>();

            for (int i = 0; i < _poolSize; i++)
            {
                CreateNewSource();
            }
        }

        private AudioSource CreateNewSource()
        {
            GameObject go = Instantiate(_audioSourcePrefab, transform);
            AudioSource source = go.GetComponent<AudioSource>();
            go.SetActive(false);
            _sourcePool.Add(source);
            return source;
        }

        /// <summary>
        /// Finds an available AudioSource in the pool or creates a new one if necessary.
        /// Bounded to maxPoolSize to prevent memory leaks.
        /// </summary>
        private AudioSource GetAvailableSource()
        {
            // Try to find inactive source
            foreach (var source in _sourcePool)
            {
                if (!source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    return source;
                }
            }

            // Pool exhausted - check if we can grow
            if (_sourcePool.Count < _maxPoolSize)
            {
                AudioSource newSource = CreateNewSource();
                newSource.gameObject.SetActive(true);
                return newSource;
            }

            // Max pool size reached - reuse oldest active source
            Debug.LogWarning($"[AudioManager] Pool exhausted! Max size {_maxPoolSize} reached. Reusing oldest source.");

            // Find first playing source and reuse it
            foreach (var source in _sourcePool)
            {
                if (source.gameObject.activeInHierarchy)
                {
                    source.Stop();
                    return source;
                }
            }

            // Fallback (should never happen)
            return _sourcePool[0];
        }

        #endregion

        #region Sound Playback

        /// <summary>
        /// Plays a SoundEvent using an available source from the pool.
        /// </summary>
        public void PlaySound(SoundEventSO soundEvent)
        {
            if (soundEvent == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null SoundEvent");
                return;
            }

            AudioSource source = GetAvailableSource();
            soundEvent.Play(source);

            // If it's not looping, return it to pool when finished
            if (!source.loop)
            {
                StartCoroutine(ReturnToPoolAfterFinished(source));
            }
        }

        private IEnumerator ReturnToPoolAfterFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source != null && source.isPlaying);

            if (source != null && source.gameObject != null)
            {
                source.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Music Playback

        /// <summary>
        /// Plays a new music track with a smooth cross-fade transition.
        /// </summary>
        public void PlayMusic(SoundEventSO musicEvent, float fadeDuration = -1f)
        {
            if (musicEvent == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null music event");
                return;
            }

            float duration = fadeDuration < 0 ? _defaultFadeDuration : fadeDuration;

            AudioSource activeSource = _isUsingSourceA ? _musicSourceA : _musicSourceB;
            AudioSource newSource = _isUsingSourceA ? _musicSourceB : _musicSourceA;

            _isUsingSourceA = !_isUsingSourceA;

            // Prepare the new source
            musicEvent.Play(newSource);

            // Stop previous crossfade if running
            if (_cleanupCoroutine != null)
            {
                StopCoroutine(_cleanupCoroutine);
            }

            _cleanupCoroutine = StartCoroutine(CrossFadeCoroutine(activeSource, newSource, duration));
        }

        private IEnumerator CrossFadeCoroutine(AudioSource outSource, AudioSource inSource, float duration)
        {
            float timer = 0f;
            float startVolume = outSource.volume;
            float targetVolume = inSource.volume;

            inSource.volume = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime; // Better for audio
                float percent = timer / duration;

                outSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                inSource.volume = Mathf.Lerp(0f, targetVolume, percent);

                yield return null;
            }

            outSource.Stop();
            outSource.volume = startVolume;
            inSource.volume = targetVolume;

            _cleanupCoroutine = null;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug/Log Pool Stats")]
        private void DebugLogPoolStats()
        {
            int active = 0;
            int inactive = 0;

            foreach (var source in _sourcePool)
            {
                if (source.gameObject.activeInHierarchy)
                    active++;
                else
                    inactive++;
            }

            Debug.Log($"=== AudioManager Pool Stats ===\n" +
                     $"Total: {_sourcePool.Count}\n" +
                     $"Active: {active}\n" +
                     $"Inactive: {inactive}\n" +
                     $"Max Size: {_maxPoolSize}");
        }
#endif

        #endregion
    }
}