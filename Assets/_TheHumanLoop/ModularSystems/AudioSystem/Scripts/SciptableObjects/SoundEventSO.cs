using UnityEngine;

namespace HumanLoop.AudioSystem
{
    [CreateAssetMenu(fileName = "NewSoundEvent", menuName = "The Human Loop/Audio/Sound Event")]
    public class SoundEventSO : ScriptableObject
    {
        public AudioClip[] _clips;
        public AudioConfigurationSO _config;

        /// <summary>
        /// Configures an AudioSource and plays a random clip from the library.
        /// </summary>
        /// <param name="source">The AudioSource that will play the sound.</param>
        public void Play(AudioSource source)
        {
            if (_clips == null || _clips.Length == 0)
            {
                //Debug.LogWarning($"SoundEvent: {name} has no clips assigned.");
                return;
            }

            if (_config == null)
            {
                //Debug.LogError($"SoundEvent: {name} is missing an AudioConfiguration.");
                return;
            }

            _config.ApplyTo(source);
            source.clip = _clips[Random.Range(0, _clips.Length)];
            source.Play();
        }
    }
}