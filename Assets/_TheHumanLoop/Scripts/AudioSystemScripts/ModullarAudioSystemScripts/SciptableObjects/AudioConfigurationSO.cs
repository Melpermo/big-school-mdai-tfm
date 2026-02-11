using UnityEngine;
using UnityEngine.Audio;

namespace HumanLoop.AudioSystem
{
    [CreateAssetMenu(fileName = "NewAudioConfig", menuName = "The Human Loop/Audio/Configuration")]
    public class AudioConfigurationSO : ScriptableObject
    {
        [Header("Mixer Settings")]
        public AudioMixerGroup OutputGroup;

        [Header("Playback Settings")]
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0.1f, 3f)] public float Pitch = 1f;

        [Tooltip("Randomize pitch slightly to avoid ear fatigue")]
        [Range(0f, 0.5f)] public float PitchVariation = 0f;

        public bool Loop = false;

        /// <summary>
        /// Applies the configuration parameters to a specific AudioSource.
        /// </summary>
        public void ApplyTo(AudioSource source)
        {
            source.outputAudioMixerGroup = OutputGroup;
            source.volume = Volume;
            source.pitch = Pitch + Random.Range(-PitchVariation, PitchVariation);
            source.loop = Loop;
        }
    }
}