using HumanLoop.Data;
using TMPro;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    public sealed class CardLocalizedView : MonoBehaviour
    {
        [Header("TMP refs")]
        [SerializeField] private TMP_Text m_titleText;
        [SerializeField] private TMP_Text m_descriptionText;
        [SerializeField] private TMP_Text m_leftChoiceText;
        [SerializeField] private TMP_Text m_rightChoiceText;

        // Guardamos la última carta pintada para refrescar si cambia el idioma.
        private ScriptableObject m_currentCardData;

        // Cache de textos legacy (por si el ID está vacío o falta en tabla)
        private string m_legacyTitle;
        private string m_legacyDescription;
        private string m_legacyLeft;
        private string m_legacyRight;

        private void OnEnable()
        {
            if (LocalizedTextTMP.Service == null) return;
            LocalizedTextTMP.Service.LanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            if (LocalizedTextTMP.Service == null) return;
            LocalizedTextTMP.Service.LanguageChanged -= Refresh;
        }

        /// <summary>
        /// Llama a esto cuando una carta pasa al frente.
        /// </summary>
        public void Bind(SimpleCardData cardData)
        {
            m_currentCardData = cardData;

            // Captura textos actuales como fallback (si ya los estabas rellenando antes).
            m_legacyTitle = m_titleText != null ? m_titleText.text : string.Empty;
            m_legacyDescription = m_descriptionText != null ? m_descriptionText.text : string.Empty;
            m_legacyLeft = m_leftChoiceText != null ? m_leftChoiceText.text : string.Empty;
            m_legacyRight = m_rightChoiceText != null ? m_rightChoiceText.text : string.Empty;

            Apply(cardData);
        }

        private void Refresh()
        {
            if (m_currentCardData == null) return;
            if (m_currentCardData is SimpleCardData simple)
            {
                Apply(simple);
            }
        }

        private void Apply(SimpleCardData data)
        {
            var loc = LocalizedTextTMP.Service;
            if (loc == null) return;

            if (m_titleText != null)
                m_titleText.text = Resolve(loc, data.TitleID, m_legacyTitle);

            if (m_descriptionText != null)
                m_descriptionText.text = Resolve(loc, data.DescriptionID, m_legacyDescription);

            if (m_leftChoiceText != null)
                m_leftChoiceText.text = Resolve(loc, data.LeftChoiceID, m_legacyLeft);

            if (m_rightChoiceText != null)
                m_rightChoiceText.text = Resolve(loc, data.RightChoiceID, m_legacyRight);
        }

        private static string Resolve(LocalizationService loc, string id, string fallback)
        {
            if (string.IsNullOrWhiteSpace(id)) return fallback;

            var translated = loc.Get(id);
            // Si Get devuelve "#id#" cuando falta, caemos a fallback:
            if (translated.Length >= 2 && translated[0] == '#' && translated[^1] == '#')
                return fallback;

            return translated;
        }
    }
}