using DG.Tweening;
using HumanLoop.Data;
using HumanLoop.Events;
using HumanLoop.LocalizationSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace HumanLoop.UI
{
    public class CardDisplay : MonoBehaviour
    {
        [Header("General UI")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private Image artImage;

        [Header("Choice UI")]
        [SerializeField] private TextMeshProUGUI leftChoiceText;
        [SerializeField] private TextMeshProUGUI rightChoiceText;
        [SerializeField] private Color colorLeft = Color.red;
        [SerializeField] private Color colorRight = Color.green;

        [Header("Category Visuals")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image categoryIconImage;

        [Header("Flip Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;
        [SerializeField] private float flipDuration = 0.5f;

        [Header("Mechanics GameEvents")]
        [SerializeField] private GameEventSO onCardAddedEvent;
        [SerializeField] private GameEventSO onCardFlipEvent;

        public CardDataSO Data { get; private set; }

        private Sequence _flipSeq;
        private Sequence _animateInSeq;

        // Bind-safe localization (avoid stale Service)
        private LocalizationService _boundLoc;

        private void OnEnable()
        {
            _boundLoc = LocalizedTextTMP.Service;
            if (_boundLoc == null) return;

            _boundLoc.LanguageChanged -= RefreshLocalizedTexts;
            _boundLoc.LanguageChanged += RefreshLocalizedTexts;

/*
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log($"[CardDisplay] ENABLE {GetInstanceID()} - Total: {FindObjectsByType<CardDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length}");
#endif*/
        }

        private void OnDisable()
        {
            // Kill tweens to avoid leaks in WebGL
            _flipSeq?.Kill(true);
            _flipSeq = null;

            _animateInSeq?.Kill(true);
            _animateInSeq = null;

            transform.DOKill(true);
            if (canvasGroup != null) canvasGroup.DOKill(true);

            if (_boundLoc != null)
            {
                _boundLoc.LanguageChanged -= RefreshLocalizedTexts;
                _boundLoc = null;
            }
        }

        public void Setup(CardDataSO data)
        {
            Data = data;

            artImage.sprite = data.cardArt;
            ApplyTextsWithLocalization(data);

            leftChoiceText.alpha = 0;
            rightChoiceText.alpha = 0;
        }

        public void SetAsBackground()
        {
            frontFace.SetActive(false);
            backFace.SetActive(true);

            transform.localEulerAngles = new Vector3(0, 180f, 0);
            transform.localScale = Vector3.one * 0.9f;

            transform.SetAsFirstSibling();

            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }

        public void ApplyCategoryStyle(Visuals.CategoryStyle style)
        {
            if (frameImage != null) frameImage.color = style.themeColor;

            if (categoryIconImage != null)
            {
                categoryIconImage.sprite = style.categoryIcon;
                categoryIconImage.color = Color.white;
            }
        }

        private void RefreshLocalizedTexts()
        {
            if (Data == null) return;
            ApplyTextsWithLocalization(Data);
        }

        private void ApplyTextsWithLocalization(CardDataSO data)
        {
            var loc = _boundLoc ?? LocalizedTextTMP.Service;

            string title = data.cardName;
            string desc = data.description;
            string left = data.leftChoiceText;
            string right = data.rightChoiceText;

            if (data is HumanLoop.Data.SimpleCardData simple && loc != null)
            {
                title = Resolve(loc, simple.TitleID, title);
                desc = Resolve(loc, simple.DescriptionID, desc);
                left = Resolve(loc, simple.LeftChoiceID, left);
                right = Resolve(loc, simple.RightChoiceID, right);
            }

            cardNameText.text = title;
            descriptionText.text = desc;
            leftChoiceText.text = left;
            rightChoiceText.text = right;
        }

        private static string Resolve(LocalizationService loc, string id, string fallback)
        {
            if (string.IsNullOrWhiteSpace(id)) return fallback;

            var translated = loc.Get(id);
            if (translated.Length >= 2 && translated[0] == '#' && translated[^1] == '#')
                return fallback;

            return translated;
        }

        public void FlipToFront()
        {
            transform.SetAsLastSibling();

            // Kill any previous flip tweens
            _flipSeq?.Kill(true);
            transform.DOKill(true);

            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

            _flipSeq = DOTween.Sequence()
                .SetTarget(this); // allows DOTween.Kill(this) if needed

            _flipSeq.Append(transform.DORotate(new Vector3(0, 90f, 0), flipDuration * 0.5f)
                .SetEase(Ease.InQuad));

            _flipSeq.AppendCallback(() =>
            {
                frontFace.SetActive(true);
                backFace.SetActive(false);
            });

            _flipSeq.Append(transform.DORotate(Vector3.zero, flipDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            _flipSeq.Join(transform.DOScale(1f, flipDuration).SetEase(Ease.OutBack));

            _flipSeq.OnComplete(() =>
            {
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            });

            onCardFlipEvent?.Raise();
        }

        public void AnimateIn()
        {
            _animateInSeq?.Kill(true);
            transform.DOKill(true);
            if (canvasGroup != null) canvasGroup.DOKill(true);

            transform.localScale = Vector3.one * 0.5f;
            if (canvasGroup != null) canvasGroup.alpha = 0;

            _animateInSeq = DOTween.Sequence()
                .SetTarget(this);

            _animateInSeq.Append(transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            if (canvasGroup != null)
                _animateInSeq.Join(canvasGroup.DOFade(1f, 0.3f));

            onCardAddedEvent?.Raise();
        }

        public void UpdateChoiceVisuals(float normalizedOffset)
        {
            float alpha = Mathf.Abs(normalizedOffset);

            if (normalizedOffset > 0)
            {
                rightChoiceText.alpha = alpha;
                leftChoiceText.alpha = 0;
                rightChoiceText.color = colorRight;
            }
            else
            {
                leftChoiceText.alpha = alpha;
                rightChoiceText.alpha = 0;
                leftChoiceText.color = colorLeft;
            }
        }

        public void HideChoices()
        {
            leftChoiceText.alpha = 0;
            rightChoiceText.alpha = 0;
        }
    }
}