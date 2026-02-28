#if UNITY_EDITOR
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.LocalizationSystem;
using UnityEditor;
using UnityEngine;

public sealed class SimpleCardLocalizationWindow : EditorWindow
{
    [Header("Inputs")]
    [SerializeField] private LocalizationTableSO _table;

    [SerializeField] private DefaultAsset _sourceFolder; // carpeta en Project
    [SerializeField] private DefaultAsset _outputFolder; // carpeta en Project
    [SerializeField] private bool _includeSubfolders = true;

    [MenuItem("Tools/Localization/Simple Card Generator (EN)")]
    public static void Open()
    {
        var window = GetWindow<SimpleCardLocalizationWindow>("Card EN Generator");
        window.minSize = new Vector2(420, 220);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generate EN SimpleCardData from ES folder", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        _table = (LocalizationTableSO)EditorGUILayout.ObjectField(
            "Localization Table", _table, typeof(LocalizationTableSO), false);

        _sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Source Folder (ES)", _sourceFolder, typeof(DefaultAsset), false);

        _outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Output Folder (EN)", _outputFolder, typeof(DefaultAsset), false);

        _includeSubfolders = EditorGUILayout.ToggleLeft("Include subfolders", _includeSubfolders);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(!CanRun()))
        {
            if (GUILayout.Button("Generate EN Cards", GUILayout.Height(32)))
            {
                Generate();
            }
        }

        EditorGUILayout.Space(8);

        EditorGUILayout.HelpBox(
            "Tip: Source/Output must be folders under Assets/. The tool will create one EN asset per ES card and re-link forced sequels when possible.",
            MessageType.Info);
    }

    private bool CanRun()
    {
        return _table != null && IsFolder(_sourceFolder) && IsFolder(_outputFolder);
    }

    private static bool IsFolder(DefaultAsset asset)
    {
        if (asset == null) return false;
        string path = AssetDatabase.GetAssetPath(asset);
        return AssetDatabase.IsValidFolder(path);
    }

    private void Generate()
    {
        string sourcePath = AssetDatabase.GetAssetPath(_sourceFolder);
        string outputPath = AssetDatabase.GetAssetPath(_outputFolder);

        _table.BuildLookup();

        // 1) Find all SimpleCardData in source folder
        string[] searchIn = _includeSubfolders ? new[] { sourcePath } : new[] { sourcePath };
        string[] guids = AssetDatabase.FindAssets("t:SimpleCardData", searchIn);

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No cards found", $"No SimpleCardData found in:\n{sourcePath}", "OK");
            return;
        }

        // Load sources
        var sources = new List<SimpleCardData>(guids.Length);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<SimpleCardData>(path);
            if (card != null) sources.Add(card);
        }

        // 2) Create EN assets (first pass) and map ES->EN
        var map = new Dictionary<CardDataSO, CardDataSO>(sources.Count);

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (SimpleCardData src in sources)
            {
                if (src == null) continue;

                string enName = MakeEnName(src.name);
                string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{outputPath}/{enName}.asset");

                SimpleCardData dst = ScriptableObject.CreateInstance<SimpleCardData>();

                CopyNonTextFields(src, dst);

                // Materialize EN texts
                dst.cardName = Translate(_table, src.TitleID, src.cardName);
                dst.description = Translate(_table, src.DescriptionID, src.description);
                dst.leftChoiceText = Translate(_table, src.LeftChoiceID, src.leftChoiceText);
                dst.rightChoiceText = Translate(_table, src.RightChoiceID, src.rightChoiceText);

                // (Opcional) NO copiamos IDs: EN no necesita traducir en runtime.
                // Si quieres copiarlos igualmente, dímelo y te lo añado vía SerializedObject.

                AssetDatabase.CreateAsset(dst, assetPath);
                map[src] = dst;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        // 3) Relink forced sequels to EN if generated
        foreach (var kv in map)
        {
            CardDataSO src = kv.Key;
            CardDataSO dst = kv.Value;

            if (src.nextCardLeft != null && map.TryGetValue(src.nextCardLeft, out CardDataSO leftEn))
            {
                dst.nextCardLeft = leftEn;
                EditorUtility.SetDirty(dst);
            }

            if (src.nextCardRight != null && map.TryGetValue(src.nextCardRight, out CardDataSO rightEn))
            {
                dst.nextCardRight = rightEn;
                EditorUtility.SetDirty(dst);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Done",
            $"Generated {map.Count} EN cards.\n\nSource: {sourcePath}\nOutput: {outputPath}",
            "OK");
    }

    private static string MakeEnName(string srcName)
    {
        // Ajusta a tu convención. Ejemplos:
        // "Card_001_ES" -> "Card_001_EN"
        // si no contiene _ES, se añade sufijo _EN
        if (srcName.Contains("_ES"))
        {
            return srcName.Replace("_ES", "_EN");
        }

        return $"{srcName}_EN";
    }

    private static void CopyNonTextFields(SimpleCardData src, SimpleCardData dst)
    {
        dst.category = src.category;
        dst.cardArt = src.cardArt;

        dst.leftSwipeImpact = src.leftSwipeImpact;
        dst.rightSwipeImpact = src.rightSwipeImpact;

        dst.useConditions = src.useConditions;
        dst.conditions = src.conditions != null
            ? new List<CardDataSO.SpawnCondition>(src.conditions)
            : new List<CardDataSO.SpawnCondition>();

        // temporal: se re-enlaza en segunda pasada si existe EN
        dst.nextCardLeft = src.nextCardLeft;
        dst.nextCardRight = src.nextCardRight;
    }

    private static string Translate(LocalizationTableSO table, string id, string fallback)
    {
        if (table == null) return fallback ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id)) return fallback ?? string.Empty;

        return table.TryGet(id, out string value, LanguageId.English)
            ? value
            : (fallback ?? string.Empty);
    }
}
#endif