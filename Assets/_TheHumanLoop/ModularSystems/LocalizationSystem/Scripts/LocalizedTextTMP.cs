using HumanLoop.LocalizationSystem;
using TMPro;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextTMP : MonoBehaviour
    {
        [SerializeField] private string m_id;

        private TMP_Text m_text;

        // Referencia al servicio (elige tu forma de exponerlo: bootstrap, locator, etc.)
        public static LocalizationService Service { get; set; }

        private void Awake()
        {
            m_text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (Service == null) return;
            Service.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (Service == null) return;
            Service.LanguageChanged -= Refresh;
        }

        public void SetId(string id)
        {
            m_id = id;
            Refresh();
        }

        private void Refresh()
        {
            if (Service == null) return;
            m_text.text = Service.Get(m_id);
        }
    }
}