using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using HumanLoop.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HumanLoop.Tools.CardCSVImporter
{
    public class CardCSVImporter: EditorWindow
    {
        [Header("Files")]
        [SerializeField] private TextAsset csvFile;
        [SerializeField] private string targetFolder = "Assets/GameData/Cards";

        [MenuItem("The Human Loop/Card Database Sync")]
        public static void ShowWindow() => GetWindow<CardCSVImporter>("Card Sync");

        private void OnGUI()
        {
            GUILayout.Label("Asset Database Configuration", EditorStyles.boldLabel);
            csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
            targetFolder = EditorGUILayout.TextField("Assets Target Folder", targetFolder);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(csvFile == null);
            if (GUILayout.Button("SYNC ALL ASSETS", GUILayout.Height(40)))
            {
                SyncCards();
            }
            EditorGUI.EndDisabledGroup();

            if (csvFile == null)
            {
                EditorGUILayout.HelpBox("Drag the .csv file here to sync the local Asset database.", MessageType.Info);
            }
        }

        private void SyncCards()
        {
            string actualPath = AssetDatabase.GetAssetPath(csvFile);
            if (!File.Exists(actualPath))
            {
                //Debug.LogError("CSV not found at Unity path.");
                return;
            }

            string[] lines = File.ReadAllLines(actualPath);
            if (lines.Length < 2) return;

            string[] headers = ParseCSVLine(lines[0]);
            Dictionary<string, SimpleCardData> allCards = new Dictionary<string, SimpleCardData>();

            try
            {
                // PASS 1: Create/Update all Assets from CSV
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] data = ParseCSVLine(lines[i]);
                    if (data.Length < headers.Length) continue;

                    string id = GetValueByHeader(headers, data, "ID");
                    if (string.IsNullOrEmpty(id)) continue;

                    EditorUtility.DisplayProgressBar("Syncing Cards", $"Processing: {id}", (float)i / lines.Length);

                    SimpleCardData card = CreateOrGetAsset(id.Trim());
                    FillCardData(card, data, headers);

                    EditorUtility.SetDirty(card);
                    if (!allCards.ContainsKey(id)) allCards.Add(id, card);
                }

                // PASS 2: Link References (Next_L / Next_R)
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] data = ParseCSVLine(lines[i]);
                    string id = GetValueByHeader(headers, data, "ID");

                    EditorUtility.DisplayProgressBar("Linking References", $"Connecting: {id}", (float)i / lines.Length);

                    if (allCards.ContainsKey(id))
                    {
                        string nextL = GetValueByHeader(headers, data, "Next_L");
                        string nextR = GetValueByHeader(headers, data, "Next_R");

                        allCards[id].nextCardLeft = (!string.IsNullOrEmpty(nextL) && allCards.ContainsKey(nextL)) ? allCards[nextL] : null;
                        allCards[id].nextCardRight = (!string.IsNullOrEmpty(nextR) && allCards.ContainsKey(nextR)) ? allCards[nextR] : null;
                        EditorUtility.SetDirty(allCards[id]);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //Debug.Log($"Database Sync Complete. {allCards.Count} assets are up to date.");
        }

        private void FillCardData(SimpleCardData card, string[] data, string[] headers)
        {
            card.cardName = GetValueByHeader(headers, data, "Title");
            card.description = GetValueByHeader(headers, data, "Description").Replace("\\n", "\n");
            card.leftChoiceText = GetValueByHeader(headers, data, "Choice_L");
            card.rightChoiceText = GetValueByHeader(headers, data, "Choice_R");

            if (Enum.TryParse(GetValueByHeader(headers, data, "Category"), true, out CardCategory cat))
                card.category = cat;

            card.leftSwipeImpact = ParseStatEffect(headers, data, "L_B", "L_T", "L_M", "L_Q");
            card.rightSwipeImpact = ParseStatEffect(headers, data, "R_B", "R_T", "R_M", "R_Q");

            ParseConditions(card, GetValueByHeader(headers, data, "Use_Cond"), GetValueByHeader(headers, data, "Conditions"));
        }

        private string[] ParseCSVLine(string line)
        {
            return Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                .Select(s => s.Trim().Trim('"'))
                .ToArray();
        }

        private string GetValueByHeader(string[] headers, string[] data, string columnName)
        {
            int index = Array.IndexOf(headers, columnName);
            return (index >= 0 && index < data.Length) ? data[index] : string.Empty;
        }

        private CardDataSO.StatEffect ParseStatEffect(string[] headers, string[] data, string b, string t, string m, string q)
        {
            return new CardDataSO.StatEffect
            {
                budget = ParseFloat(GetValueByHeader(headers, data, b)),
                time = ParseFloat(GetValueByHeader(headers, data, t)),
                morale = ParseFloat(GetValueByHeader(headers, data, m)),
                quality = ParseFloat(GetValueByHeader(headers, data, q))
            };
        }

        private float ParseFloat(string value) =>
            float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float res) ? res : 0;

        private void ParseConditions(SimpleCardData card, string useCondStr, string conditionsStr)
        {
            card.useConditions = useCondStr.ToLower() == "true";
            card.conditions = new List<CardDataSO.SpawnCondition>();
            if (string.IsNullOrEmpty(conditionsStr) || conditionsStr == "NaN") return;

            string[] parts = conditionsStr.Split(':');
            if (parts.Length == 3)
            {
                CardDataSO.SpawnCondition cond = new CardDataSO.SpawnCondition();
                Enum.TryParse(parts[0], true, out cond.stat);
                cond.comparison = parts[1] == ">" ? CardDataSO.SpawnCondition.Comparison.GreaterThan : CardDataSO.SpawnCondition.Comparison.LessThan;
                cond.value = ParseFloat(parts[2]);
                card.conditions.Add(cond);
            }
        }

        private SimpleCardData CreateOrGetAsset(string id)
        {
            if (!AssetDatabase.IsValidFolder(targetFolder)) Directory.CreateDirectory(targetFolder);
            string path = $"{targetFolder}/{id}.asset";
            SimpleCardData asset = AssetDatabase.LoadAssetAtPath<SimpleCardData>(path);
            if (asset == null)
            {
                asset = CreateInstance<SimpleCardData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}
