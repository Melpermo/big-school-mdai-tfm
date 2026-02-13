using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace TheHumanLoop.UI
{
    public class FloatingMenuManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FloatingMenu floatingMenu;
        [SerializeField] private List<UIWindow> windows;

        [Header("Background Dimmer")]
        [SerializeField] private CanvasGroup dimmerCanvasGroup;
        [SerializeField] private float dimmerAlpha = 0.6f;
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Global Settings")]
        [SerializeField] private bool closeMenuOnWindowOpen = true;

        /// <summary>
        /// Opens a specific window by index and manages the menu state.
        /// This method should be called by the OnClick event of your sub-buttons.
        /// </summary>
        public void OpenWindow(int index)
        {
            if (index < 0 || index >= windows.Count)
            {
                Debug.LogWarning($"FloatingMenuManager: Window index {index} out of bounds.");
                return;
            }

            // 1. Close the floating menu if configured
            if (closeMenuOnWindowOpen)
            {
                floatingMenu.ToggleMenu(); // This triggers the Close animation
            }

            // 2. Dimmer Animation
            dimmerCanvasGroup.gameObject.SetActive(true);
            dimmerCanvasGroup.DOKill();
            dimmerCanvasGroup.DOFade(dimmerAlpha, fadeDuration);

            // 3. Play a subtle global 'hit' effect or sound here if desired

            // 4. Open the target window
            windows[index].Open();
        }

        /// <summary>
        /// Close all active windows. Useful for a 'Back' or 'Home' functionality.
        /// </summary>
        public void CloseAllWindows()
        {
            CloseDrimmer();

            foreach (var window in windows)
            {
                if (window.gameObject.activeSelf)
                {
                    window.Close();
                }
            }
        }

        public void CloseDrimmer()
        {
            // --- Dimmer Fade Out ---
            dimmerCanvasGroup.DOKill();
            dimmerCanvasGroup.DOFade(0f, fadeDuration).OnComplete(() => {
                dimmerCanvasGroup.gameObject.SetActive(false);
            });
        }
    }
}
