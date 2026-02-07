using HumanLoop.Events;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace HumanLoop.EditorTools
{
    /// <summary>
    /// Adds a custom button to the GameEvent ScriptableObject inspector.
    /// </summary>
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector (so we can still see the listeners list)
            base.OnInspectorGUI();

            // Only allow clicking the button if the game is in Play Mode
            GUI.enabled = Application.isPlaying;

            GameEvent e = (GameEvent)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Raise Event (Debug)"))
            {
                e.Raise();
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You can only raise events in Play Mode.", MessageType.Info);
            }
        }
    }
}
