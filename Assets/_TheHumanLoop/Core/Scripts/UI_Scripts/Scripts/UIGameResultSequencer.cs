using UnityEngine;
using TMPro; // Required for TextMeshPro
using DG.Tweening;

namespace TheHumanLoop.UI
{
    public class UIGameResultSequencer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CanvasGroup backgroundDimmer;

        [Header("Victory Settings")]
        [SerializeField] private string victoryString = "VICTORY";
        [SerializeField] private Color victoryColor = Color.yellow;
        [SerializeField] private float dropDistance = 500f;

        [Header("Defeat Settings")]
        [SerializeField] private string defeatString = "DEFEAT";
        [SerializeField] private Color defeatColor = Color.red;

        private Vector3 _originalTitlePos;

        void Awake()
        {
            _originalTitlePos = titleTransform.localPosition;

            // Hide elements initially
            titleTransform.localScale = Vector3.zero;
            if (backgroundDimmer != null) backgroundDimmer.alpha = 0;
        }

        [ContextMenu("Test Victory")] // Allows you to right-click the script in Inspector to test
        public void PlayVictorySequence()
        {
            ResetUI();
            titleText.text = victoryString;
            titleText.color = victoryColor;

            Sequence vSeq = DOTween.Sequence();

            // 1. Dim the background
            if (backgroundDimmer != null)
                vSeq.Append(backgroundDimmer.DOFade(1f, 0.5f));

            // 2. Drop the title from above
            titleTransform.localPosition = _originalTitlePos + new Vector3(0, dropDistance, 0);
            vSeq.Append(titleTransform.DOLocalMove(_originalTitlePos, 0.8f).SetEase(Ease.OutBounce));

            // 3. Simultaneously scale it up
            vSeq.Join(titleTransform.DOScale(1.2f, 0.8f).SetEase(Ease.OutElastic));

            // 4. Constant "shine" or pulse effect after it lands
            vSeq.OnComplete(() => {
                titleTransform.DOScale(1f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            });
        }

        [ContextMenu("Test Defeat")]
        public void PlayDefeatSequence()
        {
            ResetUI();
            titleText.text = defeatString;
            titleText.color = defeatColor;

            Sequence dSeq = DOTween.Sequence();

            // 1. Darken the background more aggressively
            if (backgroundDimmer != null)
                dSeq.Append(backgroundDimmer.DOFade(1f, 1.5f));

            // 2. Fade text in slowly and "sink" it slightly
            titleText.alpha = 0;
            titleTransform.localScale = Vector3.one * 0.8f;

            dSeq.Append(titleText.DOFade(1f, 2f));
            dSeq.Join(titleTransform.DOLocalMoveY(_originalTitlePos.y - 30f, 2f).SetEase(Ease.OutSine));

            // 3. Subtle camera-like shake for "impact" of loss
            dSeq.Append(titleTransform.DOShakePosition(0.5f, 10f, 10, 90, false, true));
        }

        private void ResetUI()
        {
            titleTransform.DOKill();
            titleTransform.localPosition = _originalTitlePos;
            titleTransform.localScale = Vector3.zero;
            if (backgroundDimmer != null) backgroundDimmer.alpha = 0;
        }
    }
}
