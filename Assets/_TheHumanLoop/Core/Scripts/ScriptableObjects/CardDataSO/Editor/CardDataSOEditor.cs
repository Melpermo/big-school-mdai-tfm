using UnityEditor;
using UnityEngine;

namespace HumanLoop.Data
{
    /// <summary>
    /// Custom editor for CardDataSO that displays a preview of the card art sprite.
    /// </summary>
    [CustomEditor(typeof(CardDataSO), true)] // 'true' makes it work for derived classes too
    public class CardDataSOEditor : Editor
    {
        private const float PREVIEW_SIZE = 200f;

        public override void OnInspectorGUI()
        {
            // Draw all default inspector fields
            DrawDefaultInspector();

            // Add spacing
            EditorGUILayout.Space(10);

            // Get reference to the target CardDataSO
            CardDataSO card = (CardDataSO)target;

            // Display card art preview if available
            if (card.cardArt != null)
            {
                EditorGUILayout.LabelField("Card Art Preview", EditorStyles.boldLabel);
                
                Texture2D preview = AssetPreview.GetAssetPreview(card.cardArt);
                if (preview != null)
                {
                    // Center the preview
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(preview, GUILayout.MaxWidth(PREVIEW_SIZE), GUILayout.MaxHeight(PREVIEW_SIZE));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    // Display sprite dimensions
                    EditorGUILayout.LabelField($"Dimensions: {card.cardArt.texture.width} x {card.cardArt.texture.height}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Card Art assigned. Assign a sprite to see preview.", MessageType.Info);
            }
        }
    }
}