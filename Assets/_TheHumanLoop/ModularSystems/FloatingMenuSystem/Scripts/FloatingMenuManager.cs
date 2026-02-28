using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // Cache para evitar allocations
        private Coroutine _currentDimmerAnimation;

        /// <summary>
        /// Opens a specific window by index and manages the menu state.
        /// This method should be called by the OnClick event of your sub-buttons.
        /// </summary>
        public void OpenWindow(int index)
        {
            if (index < 0 || index >= windows.Count)
            {
                return;
            }

            if (closeMenuOnWindowOpen)
            {
                floatingMenu.CloseMenu(); // Usa método específico
            }

            ShowDimmer();
            windows[index].Open();
        }

        /// <summary>
        /// Close all active windows. Useful for a 'Back' or 'Home' functionality.
        /// </summary>
        public void CloseAllWindows()
        {
            HideDimmer();

            // Optimización: Solo cerrar ventanas activas
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].gameObject.activeSelf)
                {
                    windows[i].Close();
                }
            }
        }

        private void ShowDimmer()
        {
            // Stop previous animation
            if (_currentDimmerAnimation != null)
            {
                StopCoroutine(_currentDimmerAnimation);
            }

            dimmerCanvasGroup.gameObject.SetActive(true);
            _currentDimmerAnimation = StartCoroutine(FadeDimmerCoroutine(dimmerAlpha));
        }

        private void HideDimmer()
        {
            if (_currentDimmerAnimation != null)
            {
                StopCoroutine(_currentDimmerAnimation);
            }

            _currentDimmerAnimation = StartCoroutine(FadeDimmerCoroutine(0f, () => {
                dimmerCanvasGroup.gameObject.SetActive(false);
            }));
        }

        private IEnumerator FadeDimmerCoroutine(float targetAlpha, System.Action onComplete = null)
        {
            float startAlpha = dimmerCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Mejor para UI
                float t = elapsed / fadeDuration;
                dimmerCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            dimmerCanvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
            _currentDimmerAnimation = null;
        }

        private void OnDestroy()
        {
            // Cleanup
            StopAllCoroutines();
        }
    }
}
