using System.Collections.Generic;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    /// <summary>
    /// ScriptableObject containing localized text strings for UI elements.
    /// Maps translation keys to their Spanish and English values.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationStaticsTable", menuName = "The Human Loop/Localization/StaticsTableSO")]
    public class LocalizationStaticsTableSO : ScriptableObject
    {
        [System.Serializable]
        public class LocalizedEntry
        {
            [Tooltip("Unique key identifier (e.g., 'menu.play', 'button.settings')")]
            public string key;

            [TextArea(1, 3)]
            public string spanish;

            [TextArea(1, 3)]
            public string english;
        }

        [Header("Localized Strings")]
        [SerializeField] private List<LocalizedEntry> entries = new List<LocalizedEntry>();

        // Dictionary for fast lookup
        private Dictionary<string, LocalizedEntry> _entryDictionary;

        private void OnEnable()
        {
            BuildDictionary();
        }

        private void BuildDictionary()
        {
            _entryDictionary = new Dictionary<string, LocalizedEntry>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.key))
                {
                    Debug.LogWarning($"[LocalizationTable] Empty key found in {name}");
                    continue;
                }

                if (_entryDictionary.ContainsKey(entry.key))
                {
                    Debug.LogWarning($"[LocalizationTable] Duplicate key '{entry.key}' in {name}");
                    continue;
                }

                _entryDictionary[entry.key] = entry;
            }
        }

        /// <summary>
        /// Gets the localized text for a given key and language.
        /// </summary>
        public string GetText(string key, LanguageManager.Language language)
        {
            if (_entryDictionary == null || _entryDictionary.Count == 0)
            {
                BuildDictionary();
            }

            if (_entryDictionary.TryGetValue(key, out LocalizedEntry entry))
            {
                return language == LanguageManager.Language.Spanish ? entry.spanish : entry.english;
            }

            Debug.LogWarning($"[LocalizationTable] Key '{key}' not found in {name}");
            return $"[MISSING: {key}]";
        }

        /// <summary>
        /// Checks if a key exists in the table.
        /// </summary>
        public bool HasKey(string key)
        {
            if (_entryDictionary == null || _entryDictionary.Count == 0)
            {
                BuildDictionary();
            }

            return _entryDictionary.ContainsKey(key);
        }

        #region Editor Utilities

#if UNITY_EDITOR
        [ContextMenu("Rebuild Dictionary")]
        private void EditorRebuildDictionary()
        {
            BuildDictionary();
            Debug.Log($"[LocalizationTable] Dictionary rebuilt. {_entryDictionary.Count} entries loaded.");
        }

        [ContextMenu("Sort Entries Alphabetically")]
        private void SortEntries()
        {
            entries.Sort((a, b) => string.Compare(a.key, b.key, System.StringComparison.Ordinal));
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[LocalizationTable] Entries sorted alphabetically.");
        }

        [ContextMenu("Find Duplicate Keys")]
        private void FindDuplicates()
        {
            HashSet<string> seen = new HashSet<string>();
            List<string> duplicates = new List<string>();

            foreach (var entry in entries)
            {
                if (!seen.Add(entry.key))
                {
                    duplicates.Add(entry.key);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"[LocalizationTable] Found {duplicates.Count} duplicate keys: {string.Join(", ", duplicates)}");
            }
            else
            {
                Debug.Log("[LocalizationTable] No duplicate keys found.");
            }
        }
#endif

        #endregion
    }
}

