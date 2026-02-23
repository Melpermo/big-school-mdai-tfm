using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheHumanLoop.UI
{

    public class BackgroundParallax : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [SerializeField] private float moveIntensity = 40f;
        [SerializeField] private float smoothTime = 0.4f;

        private Vector3 _initialPosition;
        private Vector2 _screenCenter;

        // Changing the type from 'Tween' to 'Tweener' gives us access to ChangeEndValue
        private Tweener _moveTweener;

        void Start()
        {
            _initialPosition = transform.localPosition;
            _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // Initialize the tweener
            // SetAutoKill(false) is vital so we can reuse it every frame
            _moveTweener = transform.DOLocalMove(_initialPosition, smoothTime)
                .SetEase(Ease.OutSine)
                .SetAutoKill(false)
                .SetUpdate(true);
        }

        void Update()
        {
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Calculate offset (-0.5 to 0.5)
            float offsetX = (mousePos.x - _screenCenter.x) / Screen.width;
            float offsetY = (mousePos.y - _screenCenter.y) / Screen.height;

            Vector3 targetPos = _initialPosition + new Vector3(
                offsetX * moveIntensity,
                offsetY * moveIntensity,
                0
            );

            // This should now work perfectly without the CS1061 error
            _moveTweener.ChangeEndValue(targetPos, true).Restart();
        }

        void OnDestroy()
        {
            if (_moveTweener != null) _moveTweener.Kill();
        }
    }
}
