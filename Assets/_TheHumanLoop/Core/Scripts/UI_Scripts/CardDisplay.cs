using DG.Tweening;
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
    /// Optimized for WebGL with shader-based flip (no SetActive during animations).
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

        [Header("Flip Visuals (Shader-Based)")]
        [SerializeField] private CanvasGroup visualsCanvasGroup;

        [Header("Legacy Flip References (Not Used with Shader)")]
        [Tooltip("Keep for backwards compatibility, but no longer using SetActive")]
        [SerializeField] private GameObject frontFace;
        [SerializeField] private GameObject backFace;

        [Header("Fade Settings")]
        [Tooltip("Duration for face fade in/out transitions")]
        [SerializeField] private float faceFadeDuration = 0.15f;

        // Public properties for CardController to access
        public CardDataSO Data { get; private set; }
        public GameObject FrontFace => frontFace;
        public GameObject BackFace => backFace;
        public TextMeshProUGUI LeftChoiceText => leftChoiceText;
        public TextMeshProUGUI RightChoiceText => rightChoiceText;

        // Cached controller reference
        private CardController _cachedController;
        public CardController Controller => _cachedController;

        // NEW: Tween para fade del visualsCanvasGroup
        private Tween _faceFadeTween;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateVisualsCanvasGroup();
        }

        private void OnDestroy()
        {
            CleanupFadeTween();
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

        private void ValidateVisualsCanvasGroup()
        {
            if (visualsCanvasGroup == null)
            {
                Debug.LogWarning("[CardDisplay] visualsCanvasGroup not assigned! Text visibility won't work correctly.", this);
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
            Debug.Log($"[CardDisplay] Applying category style for {data.category}");
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

        #region Visual State (No Animations)

        /// <summary>
        /// Shows front face of card with smooth fade transition.
        /// Uses CanvasGroup alpha fade (shader handles actual card flip).
        /// WebGL-safe: smooth transition without SetActive.
        /// </summary>
        public void ShowFrontFace()
        {
            if (visualsCanvasGroup == null) return;

            // Cancel any active fade
            CleanupFadeTween();

            // Fade in to alpha 1
            _faceFadeTween = visualsCanvasGroup
                .DOFade(1f, faceFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetTarget(visualsCanvasGroup)
                .SetAutoKill(true)
                .OnComplete(() => {
                    visualsCanvasGroup.blocksRaycasts = true;
                    _faceFadeTween = null;
                });
        }

        /// <summary>
        /// Shows back face of card with smooth fade transition.
        /// Uses CanvasGroup alpha fade (shader handles actual card flip).
        /// WebGL-safe: smooth transition without SetActive.
        /// </summary>
        public void ShowBackFace()
        {
            if (visualsCanvasGroup == null) return;

            // Cancel any active fade
            CleanupFadeTween();

            // Disable raycasts immediately (don't wait for fade)
            visualsCanvasGroup.blocksRaycasts = false;

            // Fade out to alpha 0
            _faceFadeTween = visualsCanvasGroup
                .DOFade(0f, faceFadeDuration)
                .SetEase(Ease.InQuad)
                .SetTarget(visualsCanvasGroup)
                .SetAutoKill(true)
                .OnComplete(() => {
                    _faceFadeTween = null;
                });
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

        private void CleanupFadeTween()
        {
            if (_faceFadeTween != null && _faceFadeTween.IsActive())
            {
                _faceFadeTween.Kill(complete: false);
                _faceFadeTween = null;
            }
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
            Debug.Log("[CardDisplay] Front face shown (CanvasGroup alpha = 1)");
        }

        [ContextMenu("Debug/Test Show Back Face")]
        private void DebugShowBackFace()
        {
            ShowBackFace();
            Debug.Log("[CardDisplay] Back face shown (CanvasGroup alpha = 0)");
        }

        [ContextMenu("Debug/Check Visuals CanvasGroup")]
        private void DebugCheckVisualsCanvasGroup()
        {
            if (visualsCanvasGroup != null)
            {
                Debug.Log($"[CardDisplay] Visuals CanvasGroup: ✓ EXISTS (Alpha: {visualsCanvasGroup.alpha})");
            }
            else
            {
                Debug.LogError("[CardDisplay] Visuals CanvasGroup: ✗ NULL", this);
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