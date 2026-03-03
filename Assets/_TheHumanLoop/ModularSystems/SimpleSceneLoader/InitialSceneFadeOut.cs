using System;
using System.Collections;
using UnityEngine;

namespace TheHumanLoop.LoadingScreenSystem
{
    public class InitialSceneFadeOut : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup; // Reference to the CanvasGroup component used for fading
        [SerializeField] private float fadeDuration = 1f; // Duration of the fade-out effect in seconds

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
           StartCoroutine(FadeOut());
            
        }

        private void SetInitFadeOutElement()
        {
            fadeCanvasGroup.alpha = 1f; // Start fully opaque
            fadeCanvasGroup.interactable = true; // Allow interaction during fade-out
            fadeCanvasGroup.blocksRaycasts = true; // Block raycasts to prevent interaction with underlying UI during fade-out
        }

        private IEnumerator FadeOut()
        {
            SetInitFadeOutElement(); // Initialize the fade-out element before starting the fade-out process
            float elapsedTime = 0f;
            // Gradually decrease the alpha of the CanvasGroup to create a fade-out effect
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                fadeCanvasGroup.alpha = alpha;
                fadeCanvasGroup.interactable = false; // Disable interaction during fade-out
                fadeCanvasGroup.blocksRaycasts = false; // Prevent blocking raycasts during fade-out                
                yield return null; // Wait for the next frame
            }
            // Ensure the alpha is set to 0 at the end of the fade-out
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false); // Optionally disable the GameObject after fading out
        }
    }
}
