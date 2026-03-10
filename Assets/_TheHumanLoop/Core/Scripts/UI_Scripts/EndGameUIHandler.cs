using DG.Tweening;
using HumanLoop.AudioSystem;
using HumanLoop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    /// <summary>
    /// Handles end game UI animations and presentation.
    /// Optimized for performance: Uses Canvas.enabled for instant show/hide.
    /// DOTween animations maintained for visual polish (efficient).
    /// </summary>
    public class EndGameUIHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _endGamePanel;
        [SerializeField] private RectTransform _endGameTitleTransform;
        [SerializeField] private TextMeshProUGUI _endGameTitleText;
        [SerializeField] private TextMeshProUGUI _endGameFeedbackMsgText;
        [SerializeField] private Image _endGameBG_image;
        [SerializeField] private CanvasGroup _backgroundDimmer;

        [Header("Canvas Components (Performance)")]
        [Tooltip("Canvas component of EndGame Panel - disabling stops rendering")]
        [SerializeField] private Canvas _endGamePanelCanvas;

        [Tooltip("Canvas component of Title - disabling stops rendering")]
        [SerializeField] private Canvas _endGameTitleCanvas;

        [Header("Animation Settings")]
        [Tooltip("Distance title drops from above")]
        [SerializeField] private float _dropDistance = 500f;

        [Tooltip("Duration for victory bounce animation")]
        [SerializeField] private float _victoryBounceDuration = 0.8f;

        [Tooltip("Duration for defeat fade animation")]
        [SerializeField] private float _defeatFadeDuration = 2f;

        [Tooltip("Vibration intensity for shake effect")]
        [SerializeField] private float _shakeStrength = 10f;

        [Header("EndGameMoments SoundEventSOs")]
        [SerializeField] private SoundEventSO _victorySoundEvent;
        [SerializeField] private SoundEventSO _gameOverSoundEvent;
        [SerializeField] private SoundEventSO _specialFailSoundEvent;
        [SerializeField] private SoundEventSO _specialMetSoundEvent;

        // Cached data
        private Vector3 _originalTitlePos;

        // Tween management
        private Sequence _activeSequence;
        private Tween _infiniteLoopTween;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            CleanupAllTweens();
        }

        private void OnDestroy()
        {
            CleanupAllTweens();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the appropriate end game sequence based on condition type.
        /// </summary>
        public void SelectConditionTypeToShow(EndGameConditionSO endGameCondition)
        {
            if (endGameCondition == null)
            {
                Debug.LogWarning("[EndGameUIHandler] EndGameConditionSO is null!");
                return;
            }

            // Determine if it's a victory or defeat condition
            bool isVictory = endGameCondition.conditionType == EndGameConditionSO.ConditionType.Victory ||
                            endGameCondition.conditionType == EndGameConditionSO.ConditionType.SpecialMet;

            if (isVictory)
            {
                PlayVictorySequence(endGameCondition);
                PlayCorrespondingSound(endGameCondition);
            }
            else
            {
                PlayDefeatSequence(endGameCondition);
                PlayCorrespondingSound(endGameCondition);
            }
        }

        /// <summary>
        /// Hides all end game UI elements and stops animations.
        /// </summary>
        public void HideUI()
        {
            CleanupAllTweens();

            // Disable Canvas rendering (performance optimized)
            SetCanvasState(_endGamePanelCanvas, false);
            SetCanvasState(_endGameTitleCanvas, false);

            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.alpha = 0;
            }
        }

        #endregion

        #region Animation Sequences

        /// <summary>
        /// Plays victory animation with bouncy drop and pulse.
        /// </summary>
        public void PlayVictorySequence(EndGameConditionSO endGameCondition)
        {
            PrepareUI(endGameCondition);

            // Create main sequence
            _activeSequence = DOTween.Sequence()
                .SetTarget(_endGameTitleTransform)
                .SetAutoKill(true)
                .SetRecyclable(true);

            // Dim background (fade is appropriate here for visual effect)
            if (_backgroundDimmer != null)
            {
                _activeSequence.Append(_backgroundDimmer.DOFade(1f, 0.5f));
            }

            // Position title above screen
            _endGameTitleTransform.localPosition = _originalTitlePos + new Vector3(0, _dropDistance, 0);

            // Drop title with bounce
            _activeSequence.Append(
                _endGameTitleTransform.DOLocalMove(_originalTitlePos, _victoryBounceDuration)
                    .SetEase(Ease.OutBounce)
            );

            // Scale with elastic effect (parallel)
            _activeSequence.Join(
                _endGameTitleTransform.DOScale(1f, _victoryBounceDuration)
                    .SetEase(Ease.OutElastic)
            );

            // Start infinite pulse after main animation
            _activeSequence.OnComplete(StartVictoryPulse);
        }

        /// <summary>
        /// Plays defeat animation with fade and shake.
        /// </summary>
        public void PlayDefeatSequence(EndGameConditionSO endGameCondition)
        {
            PrepareUI(endGameCondition);

            // Create main sequence
            _activeSequence = DOTween.Sequence()
                .SetTarget(_endGameTitleTransform)
                .SetAutoKill(true)
                .SetRecyclable(true);

            // Darken background (fade is appropriate here for visual effect)
            if (_backgroundDimmer != null)
            {
                _activeSequence.Append(_backgroundDimmer.DOFade(1f, 1.5f));
            }

            // Set initial state for fade-in
            _endGameTitleText.alpha = 0;
            _endGameTitleTransform.localScale = Vector3.one * 0.8f;

            // Fade in text (part of animation, not visibility control)
            _activeSequence.Append(_endGameTitleText.DOFade(1f, _defeatFadeDuration));

            // Sink slowly (parallel)
            _activeSequence.Join(
                _endGameTitleTransform.DOLocalMoveY(_originalTitlePos.y - 30f, _defeatFadeDuration)
                    .SetEase(Ease.OutSine)
            );

            // Shake for impact (reduced vibrations for WebGL)
            _activeSequence.Append(
                _endGameTitleTransform.DOShakePosition(0.5f, _shakeStrength, 20, 90, false, true)
            );

            _activeSequence.OnComplete(OnDefeatComplete);
        }

        #endregion

        #region Animation Callbacks

        private void StartVictoryPulse()
        {
            // Kill any previous infinite loop
            if (_infiniteLoopTween != null && _infiniteLoopTween.IsActive())
            {
                _infiniteLoopTween.Kill();
            }

            // Create infinite pulse tween
            _infiniteLoopTween = _endGameTitleTransform
                .DOScale(1.1f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(_endGameTitleTransform)
                .SetAutoKill(false);

            _activeSequence = null;
        }

        private void OnDefeatComplete()
        {
            _activeSequence = null;
        }

        #endregion

        #region Sound Management

        /// <summary>
        /// Plays the corresponding sound based on the end game condition type.
        /// </summary>
        private void PlayCorrespondingSound(EndGameConditionSO endGameConditionSO)
        {
            if (AudioManager.Instance == null) return;

            switch (endGameConditionSO.conditionType)
            {
                case EndGameConditionSO.ConditionType.Victory:
                    if (_victorySoundEvent != null)
                        AudioManager.Instance.PlaySound(_victorySoundEvent);
                    break;

                case EndGameConditionSO.ConditionType.GameOver:
                    if (_gameOverSoundEvent != null)
                        AudioManager.Instance.PlaySound(_gameOverSoundEvent);
                    break;

                case EndGameConditionSO.ConditionType.SpecialMet:
                    if (_specialMetSoundEvent != null)
                        AudioManager.Instance.PlaySound(_specialMetSoundEvent);
                    break;

                case EndGameConditionSO.ConditionType.SpecialFailed:
                    if (_specialFailSoundEvent != null)
                        AudioManager.Instance.PlaySound(_specialFailSoundEvent);
                    break;

                default:
                    if (_gameOverSoundEvent != null)
                        AudioManager.Instance.PlaySound(_gameOverSoundEvent);
                    break;
            }
        }

        #endregion

        #region UI Management

        private void InitializeUI()
        {
            _originalTitlePos = _endGameTitleTransform.localPosition;
            _endGameTitleTransform.localScale = Vector3.zero;

            // Disable Canvas rendering (performance optimized)
            SetCanvasState(_endGamePanelCanvas, false);
            SetCanvasState(_endGameTitleCanvas, false);

            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.alpha = 0;
            }
        }

        private void PrepareUI(EndGameConditionSO endGameCondition)
        {
            CleanupAllTweens();

            // Enable Canvas rendering (performance optimized)
            SetCanvasState(_endGamePanelCanvas, true);
            SetCanvasState(_endGameTitleCanvas, true);

            _endGameTitleText.text = endGameCondition.conditionName;
            _endGameFeedbackMsgText.text = endGameCondition.endGameMessage;

            if (_endGameBG_image != null && endGameCondition.endGameBG_image != null)
            {
                _endGameBG_image.sprite = endGameCondition.endGameBG_image;
            }

            ResetTransforms();
        }

        private void ResetTransforms()
        {
            _endGameTitleTransform.localPosition = _originalTitlePos;
            _endGameTitleTransform.localScale = Vector3.zero;

            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.alpha = 0;
            }
        }

        /// <summary>
        /// Enables/disables Canvas rendering.
        /// CRITICAL for performance: Disabled canvas stops rendering but preserves geometry buffer.
        /// </summary>
        private void SetCanvasState(Canvas canvas, bool enabled)
        {
            if (canvas == null) return;
            canvas.enabled = enabled;
        }

        #endregion

        #region Tween Cleanup

        private void CleanupAllTweens()
        {
            // Kill main sequence
            if (_activeSequence != null && _activeSequence.IsActive())
            {
                _activeSequence.Kill(complete: false);
                _activeSequence = null;
            }

            // Kill infinite loop (CRITICAL for memory)
            if (_infiniteLoopTween != null && _infiniteLoopTween.IsActive())
            {
                _infiniteLoopTween.Kill(complete: false);
                _infiniteLoopTween = null;
            }

            // Safety: Kill all tweens on these components
            if (_endGameTitleTransform != null)
            {
                _endGameTitleTransform.DOKill(complete: false);
            }

            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.DOKill(complete: false);
            }

            if (_endGameTitleText != null)
            {
                _endGameTitleText.DOKill(complete: false);
            }
        }

        #endregion

        #region Testing Methods

#if UNITY_EDITOR
        [ContextMenu("Test/Victory Animation")]
        private void TestVictory()
        {
            EndGameConditionSO testCondition = CreateTestCondition(
                "¡Victoria de Prueba!",
                "¡Enhorabuena! Has completado el juego exitosamente.",
                EndGameConditionSO.ConditionType.Victory
            );
            SelectConditionTypeToShow(testCondition);
        }

        [ContextMenu("Test/Defeat Animation")]
        private void TestDefeat()
        {
            EndGameConditionSO testCondition = CreateTestCondition(
                "Game Over",
                "El proyecto ha colapsado. Intenta de nuevo.",
                EndGameConditionSO.ConditionType.GameOver
            );
            SelectConditionTypeToShow(testCondition);
        }

        [ContextMenu("Test/Hide UI")]
        private void TestHideUI()
        {
            HideUI();
        }

        [ContextMenu("Debug/Check Active Tweens")]
        private void DebugCheckActiveTweens()
        {
            bool sequenceActive = _activeSequence != null && _activeSequence.IsActive();
            bool loopActive = _infiniteLoopTween != null && _infiniteLoopTween.IsActive();

            Debug.Log($"[EndGameUIHandler] Sequence Active: {sequenceActive}, Infinite Loop Active: {loopActive}");
        }

        [ContextMenu("Debug/Check Canvas State")]
        private void DebugCheckCanvasState()
        {
            Debug.Log($"=== ENDGAME CANVAS STATE ===\n" +
                     $"Panel Canvas: {GetCanvasStateString(_endGamePanelCanvas)}\n" +
                     $"Title Canvas: {GetCanvasStateString(_endGameTitleCanvas)}\n" +
                     $"Background Dimmer Alpha: {_backgroundDimmer?.alpha:F2}");
        }

        private string GetCanvasStateString(Canvas canvas)
        {
            if (canvas == null) return "NULL";
            return canvas.enabled ? "ENABLED (Rendering)" : "DISABLED (Not Rendering)";
        }

        private EndGameConditionSO CreateTestCondition(string name, string message, EndGameConditionSO.ConditionType type)
        {
            EndGameConditionSO condition = ScriptableObject.CreateInstance<EndGameConditionSO>();
            condition.conditionName = name;
            condition.endGameMessage = message;
            condition.conditionType = type;
            return condition;
        }
#endif

        #endregion
    }
}
