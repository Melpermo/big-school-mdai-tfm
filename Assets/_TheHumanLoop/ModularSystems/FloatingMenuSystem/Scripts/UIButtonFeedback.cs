using DG.Tweening;
using HumanLoop.AudioSystem;
using UnityEngine;

namespace TheHumanLoop.UI
{
    /// <summary>
    /// Provides visual and audio feedback for button clicks.
    /// Optimized for WebGL with proper tween cleanup to prevent memory leaks.
    /// </summary>
    public class UIButtonFeedback : MonoBehaviour
    {
        [Header("Sound Event")]
        [SerializeField] private SoundEventSO _buttonPressedSoundEvent;

        [Header("Animation Settings")]
        [SerializeField] private float scaleAmount = 0.95f;
        [SerializeField] private float animationDuration = 0.1f;

        // Tween reference for proper cleanup
        private Sequence _activeSequence;

        #region Unity Lifecycle

        private void OnDisable()
        {
            CleanupTween();
        }

        private void OnDestroy()
        {
            CleanupTween();
        }

        #endregion

        #region Button Interaction

        /// <summary>
        /// Called when button is clicked. Provides visual and audio feedback.
        /// </summary>
        public void OnButtonClick()
        {
            // CRITICAL: Kill previous tweens BEFORE creating new ones
            CleanupTween();

            // Create sequence to ensure both tweens are tracked
            _activeSequence = DOTween.Sequence()
                .SetTarget(transform)
                .SetAutoKill(true)
                .SetRecyclable(true)
                .SetUpdate(true); // WebGL safe

            // Scale down (press effect)
            _activeSequence.Append(
                transform.DOScale(scaleAmount, animationDuration)
            );

            // Scale back up (release effect)
            _activeSequence.Append(
                transform.DOScale(1f, animationDuration)
            );

            _activeSequence.OnComplete(() => {
                _activeSequence = null;
            });

            // Play click sound effect
            if (_buttonPressedSoundEvent != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(_buttonPressedSoundEvent);
            }
        }

        #endregion

        #region Cleanup

        private void CleanupTween()
        {
            if (_activeSequence != null && _activeSequence.IsActive())
            {
                _activeSequence.Kill(complete: false);
                _activeSequence = null;
            }

            // Safety cleanup
            if (transform != null)
            {
                transform.DOKill(complete: false);
            }
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR
        [ContextMenu("Test/Simulate Click")]
        private void TestClick()
        {
            OnButtonClick();
        }
        #endif

        #endregion
    }
}
