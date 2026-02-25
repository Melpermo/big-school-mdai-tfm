using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using HumanLoop.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HumanLoop.Tools.DeckBuilder
{
    public class DeckBuilderWindow : EditorWindow
    {
        [System.Serializable]
        public struct FilterRule
        {
            public string columnName;
            public string value;
        }

        [Header("Data Sources")]
        [SerializeField] private TextAsset csvFile;
        [SerializeField] private string cardsFolder = "Assets/GameData/Cards";

        [Header("Target")]
        [SerializeField] private DeckSO targetDeck;

        [Header("Filter Rules")]
        [SerializeField]
        private List<FilterRule> filterRules = new List<FilterRule>
    {
        new FilterRule { columnName = "Phase", value = "Early" }
    };

        [MenuItem("The Human Loop/Advanced Deck Builder")]
        public static void ShowWindow() => GetWindow<DeckBuilderWindow>("Deck Builder");

        private void OnGUI()
        {
            GUILayout.Label("Advanced Deck Generation Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("All rules must be met (AND logic). If a column name is wrong, no cards will be added.", MessageType.Info);

            EditorGUILayout.Space();
            GUILayout.Label("References", EditorStyles.boldLabel);
            csvFile = (TextAsset)EditorGUILayout.ObjectField("Reference CSV", csvFile, typeof(TextAsset), false);
            cardsFolder = EditorGUILayout.TextField("Cards Folder", cardsFolder);
            targetDeck = (DeckSO)EditorGUILayout.ObjectField("Target Deck SO", targetDeck, typeof(DeckSO), false);

            EditorGUILayout.Space();
            DrawFilterRulesUI();

            EditorGUILayout.Space();

            bool isValid = csvFile != null && targetDeck != null && filterRules.Count > 0;

            EditorGUI.BeginDisabledGroup(!isValid);
            if (GUILayout.Button("BUILD FILTERED DECK", GUILayout.Height(40)))
            {
                BuildDeck();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawFilterRulesUI()
        {
            GUILayout.Label("Filter Rules (AND Logic)", EditorStyles.boldLabel);

            for (int i = 0; i < filterRules.Count; i++)
            {
                EditorGUILayout.BeginVertical("box"); // Agrupamos visualmente
                EditorGUILayout.BeginHorizontal();

                // Etiquetas claras para evitar la confusión entre Columna y Valor
                EditorGUILayout.LabelField("CSV Column:", GUILayout.Width(80));
                var rule = filterRules[i];
                rule.columnName = EditorGUILayout.TextField(rule.columnName);

                EditorGUILayout.LabelField("Value:", GUILayout.Width(50));
                rule.value = EditorGUILayout.TextField(rule.value);

                filterRules[i] = rule;

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    filterRules.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Rule", GUILayout.Width(120)))
            {
                // Sugerimos "Phase" por defecto para ayudar al usuario
                filterRules.Add(new FilterRule { columnName = "Phase", value = "" });
            }
        }

        private void BuildDeck()
        {
            string csvPath = AssetDatabase.GetAssetPath(csvFile);
            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2) return;

            string[] headers = ParseCSVLine(lines[0]);

            // Find the ID column index (Case-Insensitive)
            int idIndex = -1;
            for (int j = 0; j < headers.Length; j++)
            {
                if (headers[j].Equals("ID", StringComparison.OrdinalIgnoreCase))
                {
                    idIndex = j; break;
                }
            }

            if (idIndex == -1)
            {
                //Debug.LogError("Critical Error: 'ID' column not found in CSV header.");
                return;
            }

            // Map filter rules to indices safely
            List<(int index, string targetValue)> activeRules = new List<(int, string)>();
            foreach (var rule in filterRules)
            {
                int foundIdx = -1;
                for (int j = 0; j < headers.Length; j++)
                {
                    if (headers[j].Trim().Equals(rule.columnName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        foundIdx = j; break;
                    }
                }

                if (foundIdx != -1)
                {
                    activeRules.Add((foundIdx, rule.value.Trim()));
                }
                else
                {
                    //Debug.LogError($"Filter Error: Column '{rule.columnName}' not found in CSV. Check for typos!");
                    return; // Stop execution to prevent adding all cards by mistake
                }
            }

            List<CardDataSO> filteredCards = new List<CardDataSO>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] data = ParseCSVLine(lines[i]);
                if (data.Length <= idIndex) continue;

                bool matchesAll = true;
                foreach (var rule in activeRules)
                {
                    // Check if the data exists for this column and matches exactly (Case-Insensitive)
                    if (data.Length <= rule.index || !data[rule.index].Equals(rule.targetValue, StringComparison.OrdinalIgnoreCase))
                    {
                        matchesAll = false;
                        break;
                    }
                }

                if (matchesAll)
                {
                    string cardID = data[idIndex].Trim();
                    string assetPath = $"{cardsFolder}/{cardID}.asset";
                    CardDataSO cardAsset = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);

                    if (cardAsset != null) filteredCards.Add(cardAsset);
                }
            }

            // Apply changes
            Undo.RecordObject(targetDeck, "Build Advanced Deck");
            targetDeck.cards = filteredCards;
            EditorUtility.SetDirty(targetDeck);
            AssetDatabase.SaveAssets();

            //Debug.Log($"SUCCESS: {filteredCards.Count} cards added to '{targetDeck.name}'. Filter: {string.Join(", ", filterRules.Select(r => r.columnName + "=" + r.value))}");
            Selection.activeObject = targetDeck;
        }

        private string[] ParseCSVLine(string line)
        {
            return Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                .Select(s => s.Trim().Trim('"'))
                .ToArray();
        }
    }
}
