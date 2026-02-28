using HumanLoop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    /// <summary>
    /// Pure view component for displaying card data.
    /// NO animations - only visual state management.
    /// CardController handles all animations.
    /// </summary>
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
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;

        // Public properties for CardController to access
        public CardDataSO Data { get; private set; }
        public GameObject FrontFace => frontFace;
        public GameObject BackFace => backFace;
        public TextMeshProUGUI LeftChoiceText => leftChoiceText;
        public TextMeshProUGUI RightChoiceText => rightChoiceText;

        // Cached controller reference
        private CardController _cachedController;
        public CardController Controller => _cachedController;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
        }

        #endregion

        #region Component Caching

        private void CacheComponents()
        {
            _cachedController = GetComponent<CardController>();

            if (_cachedController == null)
            {
                Debug.LogError("[CardDisplay] CardController component not found!", this);
            }
        }

        #endregion

        #region Setup & Data

        /// <summary>
        /// Initializes card with data. Pure visual setup, no animations.
        /// </summary>
        public void Setup(CardDataSO data)
        {
            Data = data;

            if (artImage != null && data.cardArt != null)
            {
                artImage.sprite = data.cardArt;
            }

            ApplyTexts(data);
            HideChoices();
        }

        /// <summary>
        /// Applies category-specific visual style.
        /// </summary>
        public void ApplyCategoryStyle(Visuals.CategoryStyle style)
        {
            if (frameImage != null)
            {
                frameImage.color = style.themeColor;
            }

            if (categoryIconImage != null)
            {
                categoryIconImage.sprite = style.categoryIcon;
                categoryIconImage.color = Color.white;
            }
        }

        #endregion

        #region Visual State (No Animations)

        /// <summary>
        /// Sets card to show front face (instant, no animation).
        /// </summary>
        public void ShowFrontFace()
        {
            if (frontFace != null) frontFace.SetActive(true);
            if (backFace != null) backFace.SetActive(false);
        }

        /// <summary>
        /// Sets card to show back face (instant, no animation).
        /// </summary>
        public void ShowBackFace()
        {
            if (frontFace != null) frontFace.SetActive(false);
            if (backFace != null) backFace.SetActive(true);
        }

        /// <summary>
        /// Updates choice text visibility based on swipe direction.
        /// </summary>
        public void UpdateChoiceVisuals(float normalizedOffset)
        {
            if (leftChoiceText == null || rightChoiceText == null) return;

            float alpha = Mathf.Abs(normalizedOffset);

            if (normalizedOffset > 0)
            {
                // Swiping right
                rightChoiceText.alpha = alpha;
                leftChoiceText.alpha = 0;
                rightChoiceText.color = colorRight;
            }
            else
            {
                // Swiping left
                leftChoiceText.alpha = alpha;
                rightChoiceText.alpha = 0;
                leftChoiceText.color = colorLeft;
            }
        }

        /// <summary>
        /// Hides both choice texts.
        /// </summary>
        public void HideChoices()
        {
            if (leftChoiceText != null) leftChoiceText.alpha = 0;
            if (rightChoiceText != null) rightChoiceText.alpha = 0;
        }

        #endregion

        #region Private Helpers

        private void ApplyTexts(CardDataSO data)
        {
            if (cardNameText != null) cardNameText.text = data.cardName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (leftChoiceText != null) leftChoiceText.text = data.leftChoiceText;
            if (rightChoiceText != null) rightChoiceText.text = data.rightChoiceText;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug/Log Card Data")]
        private void DebugLogCardData()
        {
            if (Data != null)
            {
                Debug.Log($"[CardDisplay] Card: {Data.cardName}, Category: {Data.category}");
            }
            else
            {
                Debug.LogWarning("[CardDisplay] No data assigned!");
            }
        }

        [ContextMenu("Debug/Check Controller")]
        private void DebugCheckController()
        {
            if (_cachedController != null)
            {
                Debug.Log($"[CardDisplay] Controller: ✓ EXISTS");
            }
            else
            {
                Debug.LogError($"[CardDisplay] Controller: ✗ NULL", this);
            }
        }

        [ContextMenu("Debug/Test Show Front Face")]
        private void DebugShowFrontFace()
        {
            ShowFrontFace();
            Debug.Log("[CardDisplay] Front face shown");
        }

        [ContextMenu("Debug/Test Show Back Face")]
        private void DebugShowBackFace()
        {
            ShowBackFace();
            Debug.Log("[CardDisplay] Back face shown");
        }
#endif

        #endregion
    }
}