using HumanLoop.LocalizationSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheHumanLoop.LocalizationSystem
{
    public class LanguageSwitcherButton : MonoBehaviour
    {

        private Button button;
        private TextMeshProUGUI buttonText;

        bool isSpanish = true;

        #region Unity Lifecycle        

        private void CacheComponents()
        {
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
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

        private void OnDestroy()
        {
            LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        }

        void Start()
        {
            CacheComponents();
            LanguageManager.Instance.SetLanguage(LanguageManager.Language.Spanish);
            UpdateButtonVisuals(LanguageManager.Instance.CurrentLanguage);
        }
        #endregion

        #region Event Handlers
        private void OnLanguageChanged(LanguageManager.Language newLanguage)
        {
            UpdateButtonVisuals(newLanguage);
        }

        public void OnToggleChanged()
        {
            isSpanish = !isSpanish; // Invert the toggle value to match our language logic
            // Toggle ON = English, Toggle OFF = Spanish
            LanguageManager.Language newLang = isSpanish
                ? LanguageManager.Language.English
                : LanguageManager.Language.Spanish;

            LanguageManager.Instance.SetLanguage(newLang);

            UpdateButtonVisuals(newLang);
        }
        #endregion


        #region Visual Updates
        private void UpdateButtonVisuals(LanguageManager.Language currentLang)
        {
            bool isSpanish = currentLang == LanguageManager.Language.Spanish;

            // Update button text
            if (buttonText != null)
                buttonText.text = isSpanish ? "Español" : "English";
        }
        #endregion
    }
}
