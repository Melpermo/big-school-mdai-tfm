using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheHumanLoop.Tools.PlayerPrefsEditor
{
#if UNITY_EDITOR
    public class PlayerPrefsEditorWindow : EditorWindow
    {
        private const string k_IndexKey = "__pp_index_v1";

        private readonly List<Entry> m_entries = new();
        private Vector2 m_scroll;
        private string m_search = string.Empty;

        private int m_selectedIndex = -1;

        private string m_key = string.Empty;
        private PrefType m_type = PrefType.String;

        private string m_stringValue = string.Empty;
        private int m_intValue;
        private float m_floatValue;

        [MenuItem("The Human Loop/Tools/PlayerPrefs Editor (Indexed)")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerPrefsEditorWindow>("PlayerPrefs");
            window.minSize = new Vector2(560, 360);
            window.RefreshIndex();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftList();
                EditorGUILayout.Space(8);
                DrawRightEditor();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    RefreshIndex();
                }

                if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    AddOrUpdateEntry(m_key, m_type);
                    SaveIndex();
                    RefreshIndex();
                }

                GUILayout.FlexibleSpace();

                m_search = GUILayout.TextField(m_search, GUI.skin.FindStyle("ToolbarSearchTextField"), GUILayout.Width(220));
                if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                {
                    m_search = string.Empty;
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawLeftList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(260)))
            {
                EditorGUILayout.LabelField("Keys", EditorStyles.boldLabel);

                using (var scroll = new EditorGUILayout.ScrollViewScope(m_scroll, GUILayout.ExpandHeight(true)))
                {
                    m_scroll = scroll.scrollPosition;

                    IEnumerable<Entry> filtered = m_entries;

                    if (!string.IsNullOrWhiteSpace(m_search))
                    {
                        filtered = filtered.Where(e =>
                            e.Key.IndexOf(m_search, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    int visibleIndex = 0;
                    foreach (Entry entry in filtered)
                    {
                        bool isSelected = IsSelected(entry);
                        var label = $"{entry.Key}  [{entry.Type}]";

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Toggle(isSelected, label, "Button"))
                            {
                                if (!isSelected)
                                {
                                    SelectEntry(entry);
                                }
                            }
                        }

                        visibleIndex++;
                    }

                    if (visibleIndex == 0)
                    {
                        EditorGUILayout.HelpBox("No keys found (or search filtered all).", MessageType.Info);
                    }
                }

                EditorGUILayout.Space(6);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Delete Key"))
                    {
                        DeleteSelectedKey();
                    }

                    if (GUILayout.Button("Delete All"))
                    {
                        DeleteAllPrefs();
                    }
                }

                EditorGUILayout.HelpBox(
                    $"Index key reserved: {k_IndexKey}\n" +
                    "This tool can only list keys that are registered in the index.",
                    MessageType.None);
            }
        }

        private void DrawRightEditor()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                EditorGUILayout.LabelField("Selected / Edit", EditorStyles.boldLabel);

                m_key = EditorGUILayout.TextField("Key", m_key);

                using (new EditorGUILayout.HorizontalScope())
                {
                    m_type = (PrefType)EditorGUILayout.EnumPopup("Type", m_type);

                    if (GUILayout.Button("Load", GUILayout.Width(70)))
                    {
                        LoadValue(m_key, m_type);
                    }

                    if (GUILayout.Button("Save", GUILayout.Width(70)))
                    {
                        SaveValue(m_key, m_type);
                        AddOrUpdateEntry(m_key, m_type);
                        SaveIndex();
                        RefreshIndex();
                    }
                }

                EditorGUILayout.Space(10);

                DrawValueField(m_type);

                EditorGUILayout.Space(10);

                EditorGUILayout.HelpBox(
                    "Tip: If you change Type, Load again (or Save to update the index).",
                    MessageType.Info);
            }
        }

        private void DrawValueField(PrefType type)
        {
            switch (type)
            {
                case PrefType.String:
                    m_stringValue = EditorGUILayout.TextField("Value", m_stringValue);
                    break;

                case PrefType.Int:
                    m_intValue = EditorGUILayout.IntField("Value", m_intValue);
                    break;

                case PrefType.Float:
                    m_floatValue = EditorGUILayout.FloatField("Value", m_floatValue);
                    break;

                default:
                    EditorGUILayout.HelpBox("Unknown type.", MessageType.Warning);
                    break;
            }
        }

        private bool IsSelected(Entry entry)
        {
            if (m_selectedIndex < 0 || m_selectedIndex >= m_entries.Count) return false;
            return m_entries[m_selectedIndex].Key == entry.Key;
        }

        private void SelectEntry(Entry entry)
        {
            m_selectedIndex = m_entries.FindIndex(e => e.Key == entry.Key);
            m_key = entry.Key;
            m_type = entry.Type;
            LoadValue(m_key, m_type);
        }

        private void LoadValue(string key, PrefType type)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            switch (type)
            {
                case PrefType.String:
                    m_stringValue = PlayerPrefs.GetString(key, string.Empty);
                    break;

                case PrefType.Int:
                    m_intValue = PlayerPrefs.GetInt(key, 0);
                    break;

                case PrefType.Float:
                    m_floatValue = PlayerPrefs.GetFloat(key, 0f);
                    break;
            }
        }

        private void SaveValue(string key, PrefType type)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            switch (type)
            {
                case PrefType.String:
                    PlayerPrefs.SetString(key, m_stringValue);
                    break;

                case PrefType.Int:
                    PlayerPrefs.SetInt(key, m_intValue);
                    break;

                case PrefType.Float:
                    PlayerPrefs.SetFloat(key, m_floatValue);
                    break;
            }

            PlayerPrefs.Save();
        }

        private void DeleteSelectedKey()
        {
            if (m_selectedIndex < 0 || m_selectedIndex >= m_entries.Count) return;

            string key = m_entries[m_selectedIndex].Key;

            if (!EditorUtility.DisplayDialog("Confirm", $"Delete key?\n\n{key}", "Delete", "Cancel"))
            {
                return;
            }

            PlayerPrefs.DeleteKey(key);

            RemoveEntry(key);
            SaveIndex();
            RefreshIndex();

            if (m_key == key)
            {
                m_key = string.Empty;
            }
        }

        private void DeleteAllPrefs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Confirm",
                    "Delete ALL PlayerPrefs (including the index)?",
                    "Delete All",
                    "Cancel"))
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            m_entries.Clear();
            m_selectedIndex = -1;
            m_key = string.Empty;
        }

        private void RefreshIndex()
        {
            m_entries.Clear();

            string json = PlayerPrefs.GetString(k_IndexKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                m_selectedIndex = -1;
                return;
            }

            IndexData data = JsonUtility.FromJson<IndexData>(json);
            if (data?.Entries == null)
            {
                m_selectedIndex = -1;
                return;
            }

            m_entries.AddRange(data.Entries
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Key))
                .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase));

            m_selectedIndex = Mathf.Clamp(m_selectedIndex, -1, m_entries.Count - 1);
        }

        private void SaveIndex()
        {
            var data = new IndexData
            {
                Entries = m_entries
                    .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Key))
                    .DistinctBy(e => e.Key)
                    .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(k_IndexKey, json);
            PlayerPrefs.Save();
        }

        private void AddOrUpdateEntry(string key, PrefType type)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (key == k_IndexKey) return;

            int index = m_entries.FindIndex(e => e.Key == key);
            if (index >= 0)
            {
                m_entries[index] = new Entry { Key = key, Type = type };
            }
            else
            {
                m_entries.Add(new Entry { Key = key, Type = type });
            }
        }

        private void RemoveEntry(string key)
        {
            m_entries.RemoveAll(e => e.Key == key);
        }

        [Serializable]
        private class IndexData
        {
            public List<Entry> Entries = new();
        }

        [Serializable]
        private class Entry
        {
            public string Key;
            public PrefType Type;
        }

        private enum PrefType
        {
            String = 0,
            Int = 1,
            Float = 2
        }
    }

    internal static class LinqExtensions
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        {
            var set = new HashSet<TKey>();
            foreach (T item in source)
            {
                if (set.Add(selector(item)))
                {
                    yield return item;
                }
            }

        }
    }
}
#endif