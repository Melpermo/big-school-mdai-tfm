using HumanLoop.Data;
using HumanLoop.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanLoop.UI
{
    /// <summary>
    /// Pure view component for displaying card data.
    /// NO animations - only visual state management.
    /// CardController handles all animations.
    /// Optimized for performance: Uses Canvas.enabled for instant show/hide without fade.
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
        [SerializeField] CardCategorySettingsSO style;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image categoryIconImage;

        [Header("Flip Visuals (Performance Optimized)")]
        [Tooltip("Canvas component controlling text visibility - disabling stops rendering")]
        [SerializeField] private Canvas visualsCanvas;

        [Tooltip("Optional CanvasGroup for raycast blocking (alpha always 1)")]
        [SerializeField] private CanvasGroup visualsCanvasGroup;

        [Header("Legacy Flip References (Not Used)")]
        [Tooltip("Keep for backwards compatibility")]
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
            ValidateVisualsCanvas();
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

        private void ValidateVisualsCanvas()
        {
            if (visualsCanvas == null)
            {
                Debug.LogWarning("[CardDisplay] visualsCanvas not assigned! Text visibility won't work correctly.", this);
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
            ApplyCategoryStyle(data);            
        }

        /// <summary>
        /// Applies category-specific visual style.
        /// </summary>
        public void ApplyCategoryStyle(CardDataSO data)
        {
            if (style == null)
            {
                Debug.LogWarning("[CardDisplay] style (CardCategorySettingsSO) not assigned! Cannot apply category visuals.", this);
                return;
            }

            var categorySettings = style.GetStyle(data.category);

            if (frameImage != null)
            {
                frameImage.color = categorySettings.themeColor;
                categoryIconImage.sprite = categorySettings.categoryIcon;
            }
        }

        #endregion

        #region Visual State (Instant, No Animations)

        /// <summary>
        /// Shows front face of card (instant).
        /// Enables Canvas rendering for maximum performance.
        /// </summary>
        public void ShowFrontFace()
        {
            // Enable Canvas to start rendering
            if (visualsCanvas != null)
            {
                visualsCanvas.enabled = true;
            }

            // Optional: Enable raycasts if CanvasGroup is used
            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = 1f; // Always 1
                visualsCanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Shows back face of card (instant).
        /// Disables Canvas to stop rendering (preserves geometry buffer).
        /// </summary>
        public void ShowBackFace()
        {
            // Disable Canvas to stop rendering
            if (visualsCanvas != null)
            {
                visualsCanvas.enabled = false;
            }

            // Optional: Disable raycasts if CanvasGroup is used
            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = 1f; // Keep at 1
                visualsCanvasGroup.blocksRaycasts = false;
            }
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
            Debug.Log("[CardDisplay] Front face shown (Canvas enabled)");
        }

        [ContextMenu("Debug/Test Show Back Face")]
        private void DebugShowBackFace()
        {
            ShowBackFace();
            Debug.Log("[CardDisplay] Back face shown (Canvas disabled)");
        }

        [ContextMenu("Debug/Check Visuals Canvas")]
        private void DebugCheckVisualsCanvas()
        {
            if (visualsCanvas != null)
            {
                string state = visualsCanvas.enabled ? "ENABLED (Rendering)" : "DISABLED (Not Rendering)";
                Debug.Log($"[CardDisplay] Visuals Canvas: ✓ EXISTS - {state}");
            }
            else
            {
                Debug.LogError("[CardDisplay] Visuals Canvas: ✗ NULL", this);
            }

            if (visualsCanvasGroup != null)
            {
                Debug.Log($"[CardDisplay] Visuals CanvasGroup: Alpha={visualsCanvasGroup.alpha:F2}, Raycasts={visualsCanvasGroup.blocksRaycasts}");
            }
        }

        [ContextMenu("Debug/Log TMP Memory Stats")]
        private void DebugLogTMPMemory()
        {
            int totalChars = 0;
            if (cardNameText != null) totalChars += cardNameText.text.Length;
            if (descriptionText != null) totalChars += descriptionText.text.Length;
            if (leftChoiceText != null) totalChars += leftChoiceText.text.Length;
            if (rightChoiceText != null) totalChars += rightChoiceText.text.Length;

            // Estimación: ~20 bytes por carácter en mesh data
            int estimatedBytes = totalChars * 20;

            Debug.Log($"[CardDisplay] Total chars: {totalChars}, " +
                      $"Estimated mesh memory: {estimatedBytes / 1024f:F2}KB");
        }
#endif

        #endregion
    }
}