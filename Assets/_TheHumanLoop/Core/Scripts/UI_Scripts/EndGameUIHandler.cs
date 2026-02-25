using DG.Tweening;
using HumanLoop.Data;
using HumanLoop.LocalizationSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    /// <summary>
    /// Handles end game UI animations and presentation based on different end game conditions.
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
        [SerializeField] private float _dropDistance = 500f;        

        private Vector3 _originalTitlePos;
        private Sequence _activeSequence;

        private void Awake()
        {
            InitializeUI();
        }

        private void OnDestroy()
        {
            KillActiveSequence();
        }

        /// <summary>
        /// Displays the appropriate end game sequence based on the condition type.
        /// </summary>
        public void SelectConditionTypeToShow(EndGameConditionSO endGameCondition)
        {
            if (endGameCondition == null)
            {
                //Debug.LogError("EndGameConditionSO is null!");
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

            //Debug.Log($"{endGameCondition.conditionType} condition met: {endGameCondition.conditionName}");
        }

        /// <summary>
        /// Plays victory animation sequence with bouncy drop effect and pulse.
        /// </summary>
        public void PlayVictorySequence(EndGameConditionSO endGameCondition)
        {
            PrepareUI(endGameCondition);

            _activeSequence = DOTween.Sequence();

            // Dim background
            if (_backgroundDimmer != null)
            {
                _activeSequence.Append(_backgroundDimmer.DOFade(1f, 0.5f));
            }

            // Drop title from above with bounce
            _endGameTitleTransform.localPosition = _originalTitlePos + new Vector3(0, _dropDistance, 0);
            _activeSequence.Append(_endGameTitleTransform.DOLocalMove(_originalTitlePos, 0.8f).SetEase(Ease.OutBounce));
            _activeSequence.Join(_endGameTitleTransform.DOScale(1f, 0.8f).SetEase(Ease.OutElastic));

            // Continuous pulse effect
            _activeSequence.OnComplete(() =>
            {
                _endGameTitleTransform.DOScale(1f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
        }

        /// <summary>
        /// Plays defeat animation sequence with fade-in and shake effect.
        /// </summary>
        public void PlayDefeatSequence(EndGameConditionSO endGameCondition)
        {
            PrepareUI(endGameCondition);

            _activeSequence = DOTween.Sequence();

            // Darken background
            if (_backgroundDimmer != null)
            {
                _activeSequence.Append(_backgroundDimmer.DOFade(1f, 1.5f));
            }

            // Fade in text and sink slowly
            _endGameTitleText.alpha = 0;
            _endGameTitleTransform.localScale = Vector3.one * 0.8f;

            _activeSequence.Append(_endGameTitleText.DOFade(1f, 2f));
            _activeSequence.Join(_endGameTitleTransform.DOLocalMoveY(_originalTitlePos.y - 30f, 2f).SetEase(Ease.OutSine));

            // Shake for impact
            _activeSequence.Append(_endGameTitleTransform.DOShakePosition(0.5f, 10f, 10, 90, false, true));
        }

        /// <summary>
        /// Hides all end game UI elements.
        /// </summary>
        public void HideUI()
        {
            KillActiveSequence();
            _endGamePanel.SetActive(false);
            _endGameTitleTransform.gameObject.SetActive(false);
            if (_backgroundDimmer != null)
            {
                _backgroundDimmer.alpha = 0;
            }
        }

        #region Private Methods

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
            KillActiveSequence();

            _endGamePanel.SetActive(true);
            _endGameTitleTransform.gameObject.SetActive(true);

            //_endGameTitleText.text = endGameCondition.conditionName;
            //_endGameFeedbackMsgText.text = endGameCondition.endGameMessage;

            var loc = LocalizedTextTMP.Service;

            _endGameTitleText.text = Resolve(loc, endGameCondition.ConditionNameID, endGameCondition.conditionName);
            _endGameFeedbackMsgText.text = Resolve(loc, endGameCondition.EndGameMessageID, endGameCondition.endGameMessage);


            if (_endGameBG_image != null && endGameCondition.endGameBG_image != null)
            {
                _endGameBG_image.sprite = endGameCondition.endGameBG_image;
            }

            ResetTransforms();
        }

        private static string Resolve(LocalizationService loc, string id, string fallback)
        {
            if (loc == null) return fallback;
            if (string.IsNullOrWhiteSpace(id)) return fallback;

            var translated = loc.Get(id);

            // Si Get() devuelve "#id#" cuando no existe en tabla:
            if (translated.Length >= 2 && translated[0] == '#' && translated[^1] == '#')
                return fallback;

            return translated;
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

        private void KillActiveSequence()
        {
            if (_activeSequence != null && _activeSequence.IsActive())
            {
                _activeSequence.Kill();
                _activeSequence = null;
            }

            _endGameTitleTransform.DOKill();
        }

        #endregion

        #region Testing Methods

        [ContextMenu("Test Victory")]
        private void TestVictory()
        {
            EndGameConditionSO testCondition = CreateTestCondition(
                "¡Victoria de Prueba!",
                "¡Enhorabuena! Has completado el juego exitosamente.",
                EndGameConditionSO.ConditionType.Victory
            );
            PlayVictorySequence(testCondition);
        }

        [ContextMenu("Test Defeat")]
        private void TestDefeat()
        {
            EndGameConditionSO testCondition = CreateTestCondition(
                "Game Over",
                "El proyecto ha colapsado. Intenta de nuevo.",
                EndGameConditionSO.ConditionType.GameOver
            );
            PlayDefeatSequence(testCondition);
        }

        private EndGameConditionSO CreateTestCondition(string name, string message, EndGameConditionSO.ConditionType type)
        {
            EndGameConditionSO condition = ScriptableObject.CreateInstance<EndGameConditionSO>();
            condition.conditionName = name;
            condition.endGameMessage = message;
            condition.conditionType = type;
            return condition;
        }

        #endregion
    }
}
