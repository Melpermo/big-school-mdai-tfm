using UnityEngine;
using UnityEngine.InputSystem;

namespace TheHumanLoop.UI
{
    /// <summary>
    /// Creates a parallax effect where the background follows mouse movement.
    /// Uses manual lerping instead of DOTween for smooth transitions.
    /// </summary>
    public class BackgroundParallax : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("How far the background moves relative to mouse position")]
        [SerializeField] private float moveIntensity = 40f;
        
        [Tooltip("Speed of the smooth transition (lower = smoother but slower)")]
        [Range(0.01f, 1f)]
        [SerializeField] private float smoothSpeed = 0.1f;

        [Header("Optional: Velocity-based smoothing")]
        [Tooltip("Use SmoothDamp instead of Lerp for more natural deceleration")]
        [SerializeField] private bool useVelocitySmoothing = false;
        
        [Tooltip("Time to reach target when using velocity smoothing")]
        [SerializeField] private float smoothTime = 0.3f;

        private Vector3 _initialPosition;
        private Vector2 _screenCenter;
        private Vector3 _currentVelocity; // For SmoothDamp

        private void Start()
        {
            _initialPosition = transform.localPosition;
            UpdateScreenCenter();
        }

        private void Update()
        {
            // Update screen center if resolution changed
            if (_screenCenter.x != Screen.width * 0.5f || _screenCenter.y != Screen.height * 0.5f)
            {
                UpdateScreenCenter();
            }

            if (Mouse.current == null) return;

            // Get mouse position
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Calculate offset (-0.5 to 0.5 range)
            float offsetX = (mousePos.x - _screenCenter.x) / Screen.width;
            float offsetY = (mousePos.y - _screenCenter.y) / Screen.height;

            // Calculate target position
            Vector3 targetPos = _initialPosition + new Vector3(
                offsetX * moveIntensity,
                offsetY * moveIntensity,
                0
            );

            // Apply smooth movement
            if (useVelocitySmoothing)
            {
                // SmoothDamp: More natural deceleration
                transform.localPosition = Vector3.SmoothDamp(
                    transform.localPosition,
                    targetPos,
                    ref _currentVelocity,
                    smoothTime
                );
            }
            else
            {
                // Lerp: Constant smooth speed
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    targetPos,
                    smoothSpeed
                );
            }
        }

        private void UpdateScreenCenter()
        {
            _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        /// <summary>
        /// Resets the background to its initial position smoothly.
        /// </summary>
        public void ResetPosition()
        {
            StartCoroutine(SmoothResetCoroutine());
        }

        private System.Collections.IEnumerator SmoothResetCoroutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startPos = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(startPos, _initialPosition, t);
                yield return null;
            }

            transform.localPosition = _initialPosition;
        }
    }
}
