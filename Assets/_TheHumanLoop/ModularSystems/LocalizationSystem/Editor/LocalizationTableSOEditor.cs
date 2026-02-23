#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    [CustomEditor(typeof(LocalizationTableSO))]
    public class LocalizationTableSOEditor : Editor
    {
        private string _search = "";
        private bool _searchInSpanish = true;
        private bool _searchInEnglish = true;
        private bool _caseSensitive = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Search text → IDs", EditorStyles.boldLabel);

            _search = EditorGUILayout.TextField("Query", _search);
            using (new EditorGUILayout.HorizontalScope())
            {
                _searchInSpanish = EditorGUILayout.ToggleLeft("Spanish", _searchInSpanish, GUILayout.Width(90));
                _searchInEnglish = EditorGUILayout.ToggleLeft("English", _searchInEnglish, GUILayout.Width(90));
                _caseSensitive = EditorGUILayout.ToggleLeft("Case", _caseSensitive, GUILayout.Width(70));
            }

            if (string.IsNullOrWhiteSpace(_search))
            {
                EditorGUILayout.HelpBox("Type a text fragment to find matching entries and get their IDs.", MessageType.Info);
                return;
            }

            var table = (LocalizationTableSO)target;
            var entries = table.GetEditorEntries();

            var comparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var matches = entries.Where(e =>
            {
                bool ok = false;
                if (_searchInSpanish && !string.IsNullOrEmpty(e.spanish))
                    ok |= e.spanish.IndexOf(_search, comparison) >= 0;

                if (_searchInEnglish && !string.IsNullOrEmpty(e.english))
                    ok |= e.english.IndexOf(_search, comparison) >= 0;

                return ok;
            }).ToList();

            EditorGUILayout.Space(6);

            if (matches.Count == 0)
            {
                EditorGUILayout.HelpBox("No matches found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox($"Found {matches.Count} match(es).", MessageType.None);

            foreach (var m in matches.Take(50)) // límite para no petar el inspector
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("ID", m.id);
                    if (_searchInSpanish) EditorGUILayout.LabelField("ES", Truncate(m.spanish));
                    if (_searchInEnglish) EditorGUILayout.LabelField("EN", Truncate(m.english));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Copy ID"))
                            EditorGUIUtility.systemCopyBuffer = m.id;

                        if (GUILayout.Button("Ping Asset"))
                            EditorGUIUtility.PingObject(table);
                    }
                }
            }

            if (matches.Count > 50)
                EditorGUILayout.HelpBox("Showing first 50 results. Narrow your query to see more.", MessageType.Info);
        }

        private static string Truncate(string s, int max = 120)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max) + "…";
        }
    }
}
#endif
