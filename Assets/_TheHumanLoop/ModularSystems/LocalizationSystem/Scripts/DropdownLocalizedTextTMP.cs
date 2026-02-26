using HumanLoop.LocalizationSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TheHumanLoop.LocalizationSystem
{
    public class DropdownLocalizedTextTMP : MonoBehaviour
    {
        [Header("TMPro Elements")]
        [SerializeField] private TMP_Dropdown _dropdownTMP;
        [SerializeField] private TMP_Text _labelTextTMP;

        // This list will hold the localized options for the dropdown. It will be updated whenever the language changes.
        private List<string> newOptions = new List<string>();

        // This method should be called whenever the language changes to update the dropdown options to the current language.
        public void GetCurrentLocalizationLanguage()
        {
            // Clear the existing options and add the localized options based on the current language.
            newOptions.Clear();

            // Here we check the current language from the LocalizationBootstrap and
            // add the appropriate options for English and Spanish.
            if (LocalizationBootstrap.Instance.GetCurrentLanguaje() == 0)
            {
                newOptions.Add("English");
                newOptions.Add("Spanish");
            }
            else
            {
                newOptions.Add("Inglés");
                newOptions.Add("Español");
            }

            Refresh();
        }

        // This method updates the dropdown options and refreshes the displayed value.
        private void Refresh()
        {
            _dropdownTMP.options.Clear();
            _dropdownTMP.AddOptions(newOptions);
            _dropdownTMP.RefreshShownValue();
        }        
    }
}
