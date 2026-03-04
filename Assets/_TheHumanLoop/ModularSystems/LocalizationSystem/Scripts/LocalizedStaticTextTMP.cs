using UnityEngine;
using TMPro;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// Component that localizes a TextMeshProUGUI element based on current language.
    /// Updates automatically when language changes.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedStaticTextTMP : MonoBehaviour
    {
        [Header("Localization Settings")]
        [Tooltip("The localization table containing the translations")]
        [SerializeField] private LocalizationStaticsTableSO localizationTable;

        [Tooltip("The key to look up in the localization table")]
        [SerializeField] private string translationKey;

        [Header("Optional Formatting")]
        [Tooltip("If true, uses string.Format with dynamic values")]
        [SerializeField] private bool useFormatting = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Cached components
        private TextMeshProUGUI _textComponent;

        // Dynamic format values (can be set from code)
        private object[] _formatArgs;

        #region Unity Lifecycle

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateText();
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets dynamic format arguments for text with placeholders (e.g., "Score: {0}").
        /// </summary>
        public void SetFormatArgs(params object[] args)
        {
            _formatArgs = args;
            UpdateText();
        }

        /// <summary>
        /// Changes the translation key and updates the text.
        /// </summary>
        public void SetTranslationKey(string newKey)
        {
            translationKey = newKey;
            UpdateText();
        }

        /// <summary>
        /// Manually forces text update.
        /// </summary>
        public void RefreshText()
        {
            UpdateText();
        }

        #endregion

        #region Private Methods

        private void OnLanguageChanged(LanguageManager.Language newLanguage)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (_textComponent == null)
            {
                Debug.LogError("[LocalizedTextTMP] TextMeshProUGUI component not found!", this);
                return;
            }

            if (localizationTable == null)
            {
                Debug.LogError($"[LocalizedTextTMP] LocalizationTable not assigned on {gameObject.name}!", this);
                return;
            }

            if (string.IsNullOrEmpty(translationKey))
            {
                Debug.LogWarning($"[LocalizedTextTMP] Translation key is empty on {gameObject.name}!");
                return;
            }

            if (LanguageManager.Instance == null)
            {
                Debug.LogError("[LocalizedTextTMP] LanguageManager.Instance is null!");
                return;
            }

            // Get localized text
            string localizedText = localizationTable.GetText(translationKey, LanguageManager.Instance.CurrentLanguage);

            // Apply formatting if needed
            if (useFormatting && _formatArgs != null && _formatArgs.Length > 0)
            {
                try
                {
                    localizedText = string.Format(localizedText, _formatArgs);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LocalizedTextTMP] Format error on {gameObject.name}: {e.Message}");
                }
            }

            _textComponent.text = localizedText;

            if (showDebugLogs)
            {
                Debug.Log($"[LocalizedTextTMP] Updated {gameObject.name} with key '{translationKey}': {localizedText}");
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test/Update Text Now")]
        private void TestUpdateText()
        {
            if (_textComponent == null)
                _textComponent = GetComponent<TextMeshProUGUI>();

            UpdateText();
        }

        [ContextMenu("Debug/Log Current State")]
        private void DebugLogState()
        {
            Debug.Log($"[LocalizedTextTMP] GameObject: {gameObject.name}\n" +
                     $"Translation Key: {translationKey}\n" +
                     $"Table Assigned: {(localizationTable != null ? localizationTable.name : "NULL")}\n" +
                     $"Current Text: {(_textComponent != null ? _textComponent.text : "NULL")}");
        }
#endif

        #endregion
    }
}