using DG.Tweening;
using HumanLoop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    /// <summary>
    /// Handles end game UI animations and presentation.
    /// Optimized for WebGL with proper tween cleanup and no memory leaks.
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

        [Header("Animation Settings")]
        [Tooltip("Distance title drops from above")]
        [SerializeField] private float _dropDistance = 500f;

        [Tooltip("Duration for victory bounce animation")]
        [SerializeField] private float _victoryBounceDuration = 0.8f;

        [Tooltip("Duration for defeat fade animation")]
        [SerializeField] private float _defeatFadeDuration = 2f;

        [Tooltip("Vibration intensity for shake effect")]
        [SerializeField] private float _shakeStrength = 10f;

        // Cached data
        private Vector3 _originalTitlePos;

        // Tween management
        private Sequence _activeSequence;
        private Tween _infiniteLoopTween; // ← Reference to infinite loop

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

            bool isVictory = endGameCondition.conditionType == EndGameConditionSO.ConditionType.Victory ||
                            endGameCondition.conditionType == EndGameConditionSO.ConditionType.SpecialMet;

            if (isVictory)
            {
                PlayVictorySequence(endGameCondition);
            }
            else
            {
                PlayDefeatSequence(endGameCondition);
            }
        }

        /// <summary>
        /// Hides all end game UI elements and stops animations.
        /// </summary>
        public void HideUI()
        {
            CleanupAllTweens();

            _endGamePanel.SetActive(false);
            _endGameTitleTransform.gameObject.SetActive(false);

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

            // Dim background
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

            // Darken background
            if (_backgroundDimmer != null)
            {
                _activeSequence.Append(_backgroundDimmer.DOFade(1f, 1.5f));
            }

            // Set initial state for fade-in
            _endGameTitleText.alpha = 0;
            _endGameTitleTransform.localScale = Vector3.one * 0.8f;

            // Fade in text
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

            // Create NEW infinite pulse tween with proper reference
            _infiniteLoopTween = _endGameTitleTransform
                .DOScale(1.1f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(_endGameTitleTransform)
                .SetAutoKill(false); // Keep alive until manually killed

            // Clear sequence reference (completed)
            _activeSequence = null;
        }

        private void OnDefeatComplete()
        {
            _activeSequence = null;
        }

        #endregion

        #region UI Management

        private void InitializeUI()
        {
            _originalTitlePos = _endGameTitleTransform.localPosition;
            _endGamePanel.SetActive(false);
            _endGameTitleTransform.localScale = Vector3.zero;

            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.alpha = 0;
            }
        }

        private void PrepareUI(EndGameConditionSO endGameCondition)
        {
            CleanupAllTweens();

            _endGamePanel.SetActive(true);
            _endGameTitleTransform.gameObject.SetActive(true);

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
            PlayVictorySequence(testCondition);
        }

        [ContextMenu("Test/Defeat Animation")]
        private void TestDefeat()
        {
            EndGameConditionSO testCondition = CreateTestCondition(
                "Game Over",
                "El proyecto ha colapsado. Intenta de nuevo.",
                EndGameConditionSO.ConditionType.GameOver
            );
            PlayDefeatSequence(testCondition);
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
