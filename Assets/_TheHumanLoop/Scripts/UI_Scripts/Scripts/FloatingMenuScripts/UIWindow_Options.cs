using UnityEngine;

namespace TheHumanLoop.UI
{
    public class UIWindow_Options : MonoBehaviour
    {
        [SerializeField] private UIToggleSwitch masterSwitch;
        [SerializeField] private UIToggleSwitch musicSwitch;
        [SerializeField] private UIToggleSwitch ambientSwitch;
        [SerializeField] private UIToggleSwitch sfxSwitch;        
        

        private void Start()
        {
            // Initialize (Example: loading from PlayerPrefs)
            masterSwitch.Setup(PlayerPrefs.GetInt("Master", 1) == 1);
            musicSwitch.Setup(PlayerPrefs.GetInt("Music", 1) == 1);
            ambientSwitch.Setup(PlayerPrefs.GetInt("Ambient", 1) == 1);            
            sfxSwitch.Setup(PlayerPrefs.GetInt("SFX", 1) == 1);


            // Listen to changes
            musicSwitch.OnToggleChanged += (isOn) =>
            {
                // Your SoundManager logic here
                Debug.Log("Music is now: " + (isOn ? "ON" : "OFF"));
                PlayerPrefs.SetInt("Music", isOn ? 1 : 0);
            };
        }
    }
}
