using DG.Tweening;
using HumanLoop.Data;
using HumanLoop.Events;
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

        /// Initializes the card display with the provided data.
        public void Setup(CardDataSO data)
        {
            Data = data;
            descriptionText.text = data.description;
            cardNameText.text = data.cardName;
            artImage.sprite = data.cardArt;

            // Initialize choices
            leftChoiceText.text = data.leftChoiceText;
            rightChoiceText.text = data.rightChoiceText;
            leftChoiceText.alpha = 0;
            rightChoiceText.alpha = 0;
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

        /// <summary>
        /// Sets the card as a background card (face down).
        /// </summary>
        public void SetAsBackground()
        {
            frontFace.SetActive(false);
            backFace.SetActive(true);
            transform.localEulerAngles = new Vector3(0, 180f, 0); // Rotated
            transform.localScale = Vector3.one * 0.9f; // Slightly smaller
            // Set sibling index so it renders behind
            transform.SetAsFirstSibling();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false; // No tocar la de atrás
        }

        /// <summary>
        /// Flips the card to the front with an animation.
        /// </summary>
        public void FlipToFront()
        {
            transform.SetAsLastSibling(); // Move to front of UI

            // Sequence for the flip effect
            Sequence flipSeq = DOTween.Sequence();

            // Rotate to 90 degrees, swap faces, then finish rotation
            flipSeq.Append(transform.DORotate(new Vector3(0, 90f, 0), flipDuration / 2).SetEase(Ease.InQuad));
            flipSeq.AppendCallback(() => {
                frontFace.SetActive(true);
                backFace.SetActive(false);
            });
            flipSeq.Append(transform.DORotate(new Vector3(0, 0f, 0), flipDuration / 2).SetEase(Ease.OutQuad));

            // Scale up to normal size
            transform.DOScale(1f, flipDuration).SetEase(Ease.OutBack);

            flipSeq.OnComplete(() => {
                if (canvasGroup != null) canvasGroup.blocksRaycasts = true; // Activar interacción al terminar el giro
            });

            // Raise flip event
            onCardFlipEvent?.Raise();
        }


        /// <summary>
        /// Performs an intro animation when the card is spawned.
        /// </summary>
        public void AnimateIn()
        {
            // 1. Set initial state: tiny and transparent
            transform.localScale = Vector3.one * 0.5f;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0;

            // 2. Animate to natural state
            transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            if (canvasGroup != null) canvasGroup.DOFade(1f, 0.3f);

            // Raise card added event
            onCardAddedEvent?.Raise();
        }

        /// <summary>
        /// Updates the visual feedback for choices based on swipe intensity.
        /// </summary>
        /// <param name="normalizedOffset">Value from -1 (Left) to 1 (Right)</param>
        public void UpdateChoiceVisuals(float normalizedOffset)
        {
            float alpha = Mathf.Abs(normalizedOffset);

            if (normalizedOffset > 0) // Swiping Right
            {
                rightChoiceText.alpha = alpha;
                leftChoiceText.alpha = 0;
                rightChoiceText.color = colorRight;
            }
            else // Swiping Left
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