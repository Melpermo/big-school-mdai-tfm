using DG.Tweening;
using HumanLoop.Data;
using HumanLoop.Events;
using HumanLoop.LocalizationSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private void OnEnable()
        {
            if (LocalizedTextTMP.Service == null) return;
            LocalizedTextTMP.Service.LanguageChanged += RefreshLocalizedTexts;
        }

        private void OnDisable()
        {
            if (LocalizedTextTMP.Service == null) return;
            LocalizedTextTMP.Service.LanguageChanged -= RefreshLocalizedTexts;
        }

        /// Initializes the card display with the provided data.
        public void Setup(CardDataSO data)
        {
            Data = data;

            // Arte no cambia por idioma
            artImage.sprite = data.cardArt;

            // Textos (localizables con fallback)
            ApplyTextsWithLocalization(data);

            // Initialize choices
            leftChoiceText.alpha = 0;
            rightChoiceText.alpha = 0;
        }

        private void RefreshLocalizedTexts()
        {
            if (Data == null) return;
            ApplyTextsWithLocalization(Data);
        }

        private void ApplyTextsWithLocalization(CardDataSO data)
        {
            var loc = LocalizedTextTMP.Service;

            // Defaults (legacy)
            string title = data.cardName;
            string desc = data.description;
            string left = data.leftChoiceText;
            string right = data.rightChoiceText;

            // Si es SimpleCardData y trae IDs, intentamos traducir
            if (data is HumanLoop.Data.SimpleCardData simple)
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

            // Si tu Get() devuelve "#id#" cuando falta, usamos fallback
            if (translated.Length >= 2 && translated[0] == '#' && translated[^1] == '#')
                return fallback;

            return translated;
        }

        /// Applies category-based styling to the card.
        public void ApplyCategoryStyle(Visuals.CategoryStyle style)
        {
            if (frameImage != null) frameImage.color = style.themeColor;
            if (categoryIconImage != null)
            {
                categoryIconImage.sprite = style.categoryIcon;
                categoryIconImage.color = Color.white; // Ensure it's visible
            }
        }

        public void SetAsBackground()
        {
            frontFace.SetActive(false);
            backFace.SetActive(true);
            transform.localEulerAngles = new Vector3(0, 180f, 0); // Rotated
            transform.localScale = Vector3.one * 0.9f; // Slightly smaller
            transform.SetAsFirstSibling();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        }

        public void FlipToFront()
        {
            transform.SetAsLastSibling();

            Sequence flipSeq = DOTween.Sequence();

            flipSeq.Append(transform.DORotate(new Vector3(0, 90f, 0), flipDuration / 2).SetEase(Ease.InQuad));
            flipSeq.AppendCallback(() => {
                frontFace.SetActive(true);
                backFace.SetActive(false);
            });
            flipSeq.Append(transform.DORotate(new Vector3(0, 0f, 0), flipDuration / 2).SetEase(Ease.OutQuad));

            transform.DOScale(1f, flipDuration).SetEase(Ease.OutBack);

            flipSeq.OnComplete(() => {
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            });

            onCardFlipEvent?.Raise();
        }

        public void AnimateIn()
        {
            transform.localScale = Vector3.one * 0.5f;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0;

            transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.3f);

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