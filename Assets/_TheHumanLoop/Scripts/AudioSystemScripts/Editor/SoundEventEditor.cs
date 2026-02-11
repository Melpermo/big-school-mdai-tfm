using UnityEngine;
using UnityEditor;

namespace HumanLoop.AudioSystem
{
    [CustomEditor(typeof(SoundEventSO))]
    public class SoundEventEditor : Editor
    {
        private AudioSource _previewSource;

        private void OnEnable()
        {
            // Create an invisible AudioSource for previewing sounds in the Editor
            _previewSource = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            // Cleanup the preview source
            if (_previewSource != null)
            {
                DestroyImmediate(_previewSource.gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector fields
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            GUI.enabled = ((SoundEventSO)target)._clips != null && ((SoundEventSO)target)._clips.Length > 0;

            if (GUILayout.Button("Preview Sound", GUILayout.Height(30)))
            {
                SoundEventSO soundEvent = (SoundEventSO)target;

                // We use the Play method we already wrote!
                soundEvent.Play(_previewSource);
            }

            if (GUILayout.Button("Stop Preview"))
            {
                _previewSource.Stop();
            }

            GUI.enabled = true;
        }
    }
}