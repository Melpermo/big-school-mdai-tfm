using System;

namespace HumanLoop.LocalizationSystem
{
    public sealed class LocalizationService
    {
        public event Action LanguageChanged;

        private readonly LocalizationTableSO m_table;

        public LanguageId CurrentLanguage { get; private set; }

        public LocalizationService(LocalizationTableSO table, LanguageId initialLanguage)
        {
            m_table = table;
            CurrentLanguage = initialLanguage;
            m_table.BuildLookup();
        }

        public void SetLanguage(LanguageId language)
        {
            if (CurrentLanguage == language) return;

            CurrentLanguage = language;
            LanguageChanged?.Invoke();
        }

        public string Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            if (m_table.TryGet(id, out var value, CurrentLanguage))
            {
                return value;
            }

            // Fallback visible para detectar faltantes:
            return $"#{id}#";
        }
    }
}