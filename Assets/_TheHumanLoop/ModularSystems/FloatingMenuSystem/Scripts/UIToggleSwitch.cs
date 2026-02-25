using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TheHumanLoop.UI
{
    public class UIToggleSwitch : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform handle;
        [SerializeField] private Image backgroundImage;

        [Header("Settings")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float handleMargin = 10f; // Space from the edges

        [Header("Colors")]
        [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.2f); // Greenish
        [SerializeField] private Color offColor = new Color(0.7f, 0.7f, 0.7f); // Grayish                                                                               
        

        // Internal state
        // We keep track of the current state and the positions for ON and OFF states.
        private bool _isOn = true;
        private float _onXPosition;
        private float _offXPosition;

        // Public property to access the current state of the switch.
        public bool IsOn => _isOn;
        public Action<bool> OnToggleChanged;

        

        private void Start()
        {
            CalculatePositions();
            //Setup(!_isOn); // Initialize the switch state
        }

        /// <summary>
        /// Calculates the X positions based on the background width and margin.
        /// </summary>
        [ContextMenu("Recalculate Positions")]
        public void CalculatePositions()
        {
            float bgWidth = backgroundImage.rectTransform.rect.width;
            float handleWidth = handle.rect.width;

            // Calculate limits so the handle stays inside the background
            _onXPosition = (bgWidth * 0.5f) - (handleWidth * 0.5f) - handleMargin;
            _offXPosition = -_onXPosition;
        }

        // This method allows external scripts to set the initial state of the switch and
        // ensures the visual elements are updated accordingly.
        public void Setup(bool initialState)
        {
            _isOn = initialState;
            CalculatePositions(); // Ensure positions are ready

            float targetX = _isOn ? _onXPosition : _offXPosition;
            handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
            backgroundImage.color = _isOn ? onColor : offColor;
        }

        // This method toggles the state of the switch, animates the transition, and invokes the OnToggleChanged event.
        public void Toggle()
        {
            _isOn = !_isOn;
            AnimateSwitch();          
            
            OnToggleChanged?.Invoke(_isOn);            
        }

        // This method handles the animation of the handle and background color when the switch is toggled.
        private void AnimateSwitch()
        {
            handle.DOKill();
            backgroundImage.DOKill();

            float targetX = _isOn ? _onXPosition : _offXPosition;
            Color targetColor = _isOn ? onColor : offColor;

            // Smooth move with OutBack for that juicy "spring" feel
            handle.DOAnchorPosX(targetX, duration).SetEase(Ease.OutBack);
            backgroundImage.DOColor(targetColor, duration);

            // Visual feedback on the handle
            handle.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }

        // This method can be called to play a "locked" effect, indicating that the switch cannot be toggled.
        public void PlayLockedEffect()
        {
            transform.DOKill(true);
            // Horizontal shake to indicate "No"
            transform.DOShakePosition(0.4f, new Vector3(10f, 0, 0), 10, 90);

            // Brief red flash
            backgroundImage.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => backgroundImage.DOColor(_isOn ? onColor : offColor, 0.1f));
        }
        
        
    }
}
