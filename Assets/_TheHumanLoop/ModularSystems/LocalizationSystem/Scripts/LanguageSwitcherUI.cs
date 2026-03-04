using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// UI component for switching between languages.
    /// Can be used with buttons or toggle.
    /// </summary>
    public class LanguageSwitcherUI : MonoBehaviour
    {
        [Header("UI Mode")]
        [SerializeField] private UIMode mode = UIMode.Buttons;

        [Header("Button Mode (Optional)")]
        [SerializeField] private Button spanishButton;
        [SerializeField] private Button englishButton;

        [Header("Toggle Mode (Optional)")]
        [SerializeField] private Toggle languageToggle;
        [SerializeField] private TextMeshProUGUI toggleLabel;
        [SerializeField] private string spanishLabel = "Español";
        [SerializeField] private string englishLabel = "English";

        [Header("Visual Feedback")]
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = Color.gray;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        public enum UIMode
        {
            Buttons,
            Toggle
        }

        private Image _spanishButtonImage;
        private Image _englishButtonImage;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            SetupListeners();
        }

        private void Start()
        {
            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (LanguageManager.Instance != null)
            {
                LanguageManager.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        }

        #endregion

        #region Setup

        private void CacheComponents()
        {
            if (mode == UIMode.Buttons)
            {
                if (spanishButton != null)
                    _spanishButtonImage = spanishButton.GetComponent<Image>();

                if (englishButton != null)
                    _englishButtonImage = englishButton.GetComponent<Image>();
            }
        }

        private void SetupListeners()
        {
            if (mode == UIMode.Buttons)
            {
                if (spanishButton != null)
                    spanishButton.onClick.AddListener(OnSpanishButtonClicked);

                if (englishButton != null)
                    englishButton.onClick.AddListener(OnEnglishButtonClicked);
            }
            else if (mode == UIMode.Toggle)
            {
                if (languageToggle != null)
                    languageToggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }

        #endregion

        #region Button Handlers

        public void OnSpanishButtonClicked()
        {
            SetLanguage(LanguageManager.Language.Spanish);
        }

        public void OnEnglishButtonClicked()
        {
            SetLanguage(LanguageManager.Language.English);
        }

        private void OnToggleChanged(bool isOn)
        {
            // Toggle ON = English, Toggle OFF = Spanish
            LanguageManager.Language newLang = isOn
                ? LanguageManager.Language.English
                : LanguageManager.Language.Spanish;

            SetLanguage(newLang);
        }

        #endregion

        #region Language Control

        private void SetLanguage(LanguageManager.Language language)
        {
            if (LanguageManager.Instance == null)
            {
                Debug.LogError("[LanguageSwitcherUI] LanguageManager.Instance is null!");
                return;
            }

            LanguageManager.Instance.SetLanguage(language);

            if (showDebugLogs)
            {
                Debug.Log($"[LanguageSwitcherUI] Language set to: {language}");
            }

            UpdateVisuals();
        }

        private void OnLanguageChanged(LanguageManager.Language newLanguage)
        {
            UpdateVisuals();
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            if (LanguageManager.Instance == null) return;

            LanguageManager.Language currentLang = LanguageManager.Instance.CurrentLanguage;

            if (mode == UIMode.Buttons)
            {
                UpdateButtonVisuals(currentLang);
            }
            else if (mode == UIMode.Toggle)
            {
                UpdateToggleVisuals(currentLang);
            }
        }

        private void UpdateButtonVisuals(LanguageManager.Language currentLang)
        {
            bool isSpanish = currentLang == LanguageManager.Language.Spanish;

            // Update button colors
            if (_spanishButtonImage != null)
                _spanishButtonImage.color = isSpanish ? selectedColor : unselectedColor;

            if (_englishButtonImage != null)
                _englishButtonImage.color = isSpanish ? unselectedColor : selectedColor;

            // Update button interactivity
            if (spanishButton != null)
                spanishButton.interactable = !isSpanish;

            if (englishButton != null)
                englishButton.interactable = isSpanish;
        }

        private void UpdateToggleVisuals(LanguageManager.Language currentLang)
        {
            bool isEnglish = currentLang == LanguageManager.Language.English;

            // Update toggle without triggering event
            if (languageToggle != null)
            {
                languageToggle.SetIsOnWithoutNotify(isEnglish);
            }           

            // Update label text (optional, can be used to show current language)
            if (toggleLabel != null)
            {
                toggleLabel.text = isEnglish ? englishLabel : spanishLabel;
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test/Switch to Spanish")]
        private void TestSwitchToSpanish()
        {
            OnSpanishButtonClicked();
        }

        [ContextMenu("Test/Switch to English")]
        private void TestSwitchToEnglish()
        {
            OnEnglishButtonClicked();
        }

        [ContextMenu("Debug/Log Current State")]
        private void DebugLogState()
        {
            if (LanguageManager.Instance != null)
            {
                Debug.Log($"[LanguageSwitcherUI] Current Language: {LanguageManager.Instance.CurrentLanguage}");
            }
        }
#endif

        #endregion
    }
}