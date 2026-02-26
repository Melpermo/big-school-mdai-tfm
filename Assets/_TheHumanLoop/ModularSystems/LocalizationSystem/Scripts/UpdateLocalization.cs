using TMPro;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    public class UpdateLocalization : MonoBehaviour
    {
        // This is an example of how to update the language from a TMP_Dropdown.
        // You can call SetLanguageFromDropdown directly from the dropdown's OnValueChanged event,
        // or use UpdateLanguage as an intermediary if you need to do additional processing.
        private TMP_Dropdown languageDropdown;
        private int languageIndex;

        void Start()
        {
            languageDropdown = GetComponent<TMP_Dropdown>();            
            SetDropdownInitialValue();
        }

        private void SetDropdownInitialValue()
        {
            if (languageDropdown == null) return;

            languageDropdown.value = LocalizationBootstrap.Instance.GetCurrentLanguaje(); //_service.CurrentLanguage == LanguageId.Spanish ? 1 : 0;
            languageDropdown.RefreshShownValue();            
        }

        // This method can be called directly from the dropdown's OnValueChanged event, passing the selected index.
        public void SetLanguage(int languageIndex)
        {
            languageDropdown = GetComponent<TMP_Dropdown>();
            LanguageId language = (LanguageId)languageIndex;
            LocalizationBootstrap.Instance.SetLanguage(languageIndex);            
        }

        // This method can be called from the dropdown's OnValueChanged event without parameters,
        // and it will read the selected index from the dropdown.
        public void UpdateLanguage()
        { 
            languageIndex = languageDropdown.value;
            SetLanguage(languageIndex);            
        }
    }
}
