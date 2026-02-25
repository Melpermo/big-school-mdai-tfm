using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanLoop.AudioSystem
{
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
        [SerializeField] private GameObject _audioSourcePrefab;

        private List<AudioSource> _sourcePool;

        private void Awake()
        {
            // Simple Singleton Pattern
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

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
        /// </summary>
        private AudioSource GetAvailableSource()
        {
            foreach (var source in _sourcePool)
            {
                if (!source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    return source;
                }
            }

            // If pool is empty, expand it
            AudioSource newSource = CreateNewSource();
            newSource.gameObject.SetActive(true);
            return newSource;
        }

        /// <summary>
        /// Plays a SoundEvent using an available source from the pool.
        /// </summary>
        public void PlaySound(SoundEventSO soundEvent)
        {
            AudioSource source = GetAvailableSource();
            soundEvent.Play(source);

            // If it's not looping, we need to return it to the pool when finished
            if (!source.loop)
            {
                StartCoroutine(ReturnToPoolAfterFinished(source));
            }
        }

        private IEnumerator ReturnToPoolAfterFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            source.gameObject.SetActive(false);
        }

        /// <summary>
        /// Plays a new music track with a smooth cross-fade transition.
        /// </summary>
        public void PlayMusic(SoundEventSO musicEvent, float fadeDuration = -1f)
        {
            float duration = fadeDuration < 0 ? _defaultFadeDuration : fadeDuration;

            AudioSource activeSource = _isUsingSourceA ? _musicSourceA : _musicSourceB;
            AudioSource newSource = _isUsingSourceA ? _musicSourceB : _musicSourceA;

            _isUsingSourceA = !_isUsingSourceA;

            // Prepare the new source (it will use the ScriptableObject config)
            musicEvent.Play(newSource);

            StartCoroutine(CrossFadeCoroutine(activeSource, newSource, duration));
        }

        private IEnumerator CrossFadeCoroutine(AudioSource outSource, AudioSource inSource, float duration)
        {
            float timer = 0f;
            float startVolume = outSource.volume;
            float targetVolume = inSource.volume; // Targeted volume from ScriptableObject

            inSource.volume = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float percent = timer / duration;

                outSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                inSource.volume = Mathf.Lerp(0f, targetVolume, percent);

                yield return null;
            }

            outSource.Stop();
            outSource.volume = startVolume;
        }
    }
}
