using UnityEngine;
using UnityEngine.Audio;

namespace HumanLoop.AudioSystem
{
    public class AudioSnapshotController : MonoBehaviour
    {
        public static AudioSnapshotController Instance { get; private set; }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerSnapshot _defaultSnapshot;
        [SerializeField] private AudioMixerSnapshot _narrativeSnapshot;

        [Header("Transition Settings")]
        [SerializeField] private float _transitionTime = 1.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Transitions to the Narrative snapshot to emphasize dialogue/text.
        /// </summary>
        public void EnterNarrativeMode()
        {
            _narrativeSnapshot.TransitionTo(_transitionTime);
        }

        /// <summary>
        /// Transitions back to the normal audio state.
        /// </summary>
        public void ExitNarrativeMode()
        {
            _defaultSnapshot.TransitionTo(_transitionTime);
        }
    }
}