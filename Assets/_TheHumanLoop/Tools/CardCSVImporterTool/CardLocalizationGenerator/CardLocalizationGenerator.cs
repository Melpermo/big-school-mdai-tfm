#if UNITY_EDITOR
using System.Collections.Generic;
using HumanLoop.Data;
using HumanLoop.LocalizationSystem;
using UnityEditor;
using UnityEngine;

public static class CardLocalizationGenerator
{
    [MenuItem("Tools/Localization/Generate EN Cards From Selected (SimpleCardData)")]
    public static void GenerateEnglishCardsFromSelected()
    {
        // 1) Requiere tabla seleccionada o asignada manualmente aquí.
        // Opción cómoda: selecciona también la LocalizationTableSO en el Project.
        LocalizationTableSO table = GetSelectedAsset<LocalizationTableSO>();
        if (table == null)
        {
            EditorUtility.DisplayDialog(
                "Missing LocalizationTableSO",
                "Select a LocalizationTableSO asset in the Project window (can be selected together with the cards) and run again.",
                "OK");
            return;
        }

        // 2) Recoge cartas seleccionadas
        SimpleCardData[] sourceCards = GetSelectedAssets<SimpleCardData>();
        if (sourceCards.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No cards selected",
                "Select one or more SimpleCardData assets and run again.",
                "OK");
            return;
        }

        // 3) Carpeta destino
        string outputFolder = EditorUtility.SaveFolderPanel("Select output folder for EN cards", "Assets", "");
        if (string.IsNullOrEmpty(outputFolder))
        {
            return;
        }

        outputFolder = ToProjectRelativePath(outputFolder);
        if (string.IsNullOrEmpty(outputFolder))
        {
            EditorUtility.DisplayDialog("Invalid folder", "Please choose a folder inside this Unity project (under Assets).", "OK");
            return;
        }

        table.BuildLookup();

        // 4) Primera pasada: crear copias y mapear ES->EN
        var map = new Dictionary<CardDataSO, CardDataSO>(sourceCards.Length);

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (SimpleCardData src in sourceCards)
            {
                if (src == null) continue;

                string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{outputFolder}/{src.name}_EN.asset");

                SimpleCardData dst = ScriptableObject.CreateInstance<SimpleCardData>();

                CopyNonTextFields(src, dst);

                // Textos EN materializados (fallback al texto actual si falta key)
                dst.cardName = Translate(table, src.TitleID, src.cardName);
                dst.description = Translate(table, src.DescriptionID, src.description);
                dst.leftChoiceText = Translate(table, src.LeftChoiceID, src.leftChoiceText);
                dst.rightChoiceText = Translate(table, src.RightChoiceID, src.rightChoiceText);

                // Opcional: copiar IDs al asset EN (no es necesario si no los usarás)
                // CopyLocalizationIds(src, dst);

                AssetDatabase.CreateAsset(dst, newAssetPath);
                map[src] = dst;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        // 5) Segunda pasada: re-enlazar secuelas forzadas para que apunten a EN si existe
        foreach (var kv in map)
        {
            CardDataSO src = kv.Key;
            CardDataSO dst = kv.Value;

            if (src.nextCardLeft != null && map.TryGetValue(src.nextCardLeft, out CardDataSO nextLeftEn))
            {
                dst.nextCardLeft = nextLeftEn;
                EditorUtility.SetDirty(dst);
            }

            if (src.nextCardRight != null && map.TryGetValue(src.nextCardRight, out CardDataSO nextRightEn))
            {
                dst.nextCardRight = nextRightEn;
                EditorUtility.SetDirty(dst);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Done",
            $"Generated {map.Count} EN cards into:\n{outputFolder}\n\nNote: Forced sequels were re-linked when the target card was also generated.",
            "OK");
    }

    private static void CopyNonTextFields(SimpleCardData src, SimpleCardData dst)
    {
        // General
        dst.category = src.category;

        // Visual
        dst.cardArt = src.cardArt;

        // Impacts
        dst.leftSwipeImpact = src.leftSwipeImpact;
        dst.rightSwipeImpact = src.rightSwipeImpact;

        // Conditions
        dst.useConditions = src.useConditions;
        if (src.conditions != null)
        {
            dst.conditions = new List<CardDataSO.SpawnCondition>(src.conditions);
        }
        else
        {
            dst.conditions = new List<CardDataSO.SpawnCondition>();
        }

        // Forced sequels (se relinkeán en segunda pasada si se genera también el target)
        dst.nextCardLeft = src.nextCardLeft;
        dst.nextCardRight = src.nextCardRight;
    }

    private static string Translate(LocalizationTableSO table, string id, string fallback)
    {
        if (string.IsNullOrWhiteSpace(id) || table == null)
        {
            return fallback ?? string.Empty;
        }

        return table.TryGet(id, out string value, LanguageId.English)
            ? value
            : (fallback ?? string.Empty);
    }

    // --- Helpers ---

    private static T GetSelectedAsset<T>() where T : Object
    {
        foreach (Object o in Selection.objects)
        {
            if (o is T t) return t;
        }
        return null;
    }

    private static T[] GetSelectedAssets<T>() where T : Object
    {
        var list = new List<T>();
        foreach (Object o in Selection.objects)
        {
            if (o is T t) list.Add(t);
        }
        return list.ToArray();
    }

    private static string ToProjectRelativePath(string absolutePath)
    {
        absolutePath = absolutePath.Replace("\\", "/");
        string dataPath = Application.dataPath.Replace("\\", "/"); // .../Project/Assets
        if (!absolutePath.StartsWith(dataPath))
        {
            return null;
        }
        return "Assets" + absolutePath.Substring(dataPath.Length);
    }
}
#endif