using HumanLoop.AudioSystem;
using UnityEngine;

#if (UNITY_EDITOR) 
public class MusicEventCroossFade : MonoBehaviour
{
    [SerializeField] private SoundEventSO _newMusicEvent;

    // Example: Changing from Exploration music to Suspense music
    [ContextMenu("EnterInNewMusicEvent")]
    public void OnMysteryDiscovered()
    {
        AudioManager.Instance.PlayMusic(_newMusicEvent, 3.0f); // 3 seconds fade
    }
}
#endif