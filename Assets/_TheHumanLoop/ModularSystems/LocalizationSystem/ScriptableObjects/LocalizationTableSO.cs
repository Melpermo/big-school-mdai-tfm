using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{

    public enum LanguageId
    {
        English = 0,
        Spanish = 1
    }

    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "The Human Loop/Localization/Table")]
    public class LocalizationTableSO : ScriptableObject
    {
        [Serializable]
        private struct Row
        {
            public string Id;
            [TextArea] public string English;
            [TextArea] public string Spanish;
        }

        [Header("CSV source (id,spanish,english)")]
        [SerializeField] private TextAsset m_csv;

        [SerializeField] private List<Row> m_rows = new();

        private Dictionary<string, Row> m_lookup;

        public void BuildLookup()
        {
            m_lookup = new Dictionary<string, Row>(StringComparer.Ordinal);
            foreach (var row in m_rows)
            {
                if (string.IsNullOrWhiteSpace(row.Id)) { continue; }
                m_lookup[row.Id] = row;
            }
        }

        public bool TryGet(string id, out string value, LanguageId language)
        {
            value = string.Empty;

            if (m_lookup == null) { BuildLookup(); }

            if (!m_lookup.TryGetValue(id, out var row))
            {
                return false;
            }

            value = language == LanguageId.Spanish ? row.Spanish : row.English;
            return true;
        }

#if UNITY_EDITOR
        [ContextMenu("Import CSV -> Rows")]
        private void ImportCsv()
        {
            if (m_csv == null) return;

            m_rows.Clear();

            var rows = ParseCsv(m_csv.text);

            // Saltamos la cabecera
            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count < 3) continue;

                m_rows.Add(new Row
                {
                    Id = cols[0],
                    Spanish = cols[1],
                    English = cols[2]
                });
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private static List<List<string>> ParseCsv(string input)
        {
            var result = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();

            bool insideQuotes = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (c == ',' && !insideQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if ((c == '\n' || c == '\r') && !insideQuotes)
                {
                    if (field.Length > 0 || row.Count > 0)
                    {
                        row.Add(field.ToString());
                        result.Add(new List<string>(row));
                        row.Clear();
                        field.Clear();
                    }
                }
                else
                {
                    field.Append(c);
                }
            }

            if (field.Length > 0)
            {
                row.Add(field.ToString());
                result.Add(row);
            }

            return result;
        }

        public IReadOnlyList<(string id, string spanish, string english)> GetEditorEntries()
        {
            var list = new List<(string, string, string)>(m_rows.Count);
            foreach (var r in m_rows)
                list.Add((r.Id, r.Spanish, r.English));
            return list;
        }
#endif
    }
}
