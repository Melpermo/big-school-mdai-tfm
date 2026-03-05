using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HumanLoop.UI
{
    //[RequireComponent(typeof(CanvasGroup))]
    public class PopupManager : MonoBehaviour
    {
        [System.Serializable]
        public class PopupEntry
        {
            public string popupID;
            public GameObject panel;
        }

        [Header("References")]
        public List<PopupEntry> popups = new List<PopupEntry>();
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        public float duration = 0.3f;
        public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve closeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private Stack<GameObject> history = new Stack<GameObject>();
        private Coroutine currentAnimation;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            CanvasGroupIsActive(canvasGroup, false);

            foreach (var entry in popups)
            {
                if (entry.panel != null)
                {
                    entry.panel.transform.localScale = Vector3.zero;
                    entry.panel.SetActive(false);
                }
            }
        }       

        public void OpenPopup(string id)
        {
            CanvasGroupIsActive(canvasGroup, true);

            PopupEntry entry = popups.Find(p => p.popupID == id);

            if (entry != null && entry.panel != null)
            {
                if (currentAnimation != null) StopCoroutine(currentAnimation);

                GameObject newPanel = entry.panel;
                newPanel.SetActive(true);
                history.Push(newPanel);

                currentAnimation = StartCoroutine(AnimatePopUp(newPanel, 1f, Vector3.one, openCurve));
            }           
        }

        public void ClosePopup(string id)
        {
            if (history.Count == 0) return;
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            PopupEntry entry = popups.Find(p => p.popupID == id);
            if (entry != null && entry.panel != null)
            {
                GameObject panelToClose = entry.panel;
                currentAnimation = StartCoroutine(AnimatePopUp(panelToClose, 0f, Vector3.zero, closeCurve, true));
                history.Pop(); // Remove from history after closing
            }
            if (history.Count == 0)
            {
                CanvasGroupIsActive(canvasGroup, false);
            }
        }

        private void CanvasGroupIsActive(CanvasGroup canvasGroupToControl, bool isActive)
        {
            if (isActive)
            {   
                canvasGroupToControl.alpha = 1;
                canvasGroupToControl.interactable = true;
                canvasGroupToControl.blocksRaycasts = true;
            }
            else
            {
                canvasGroupToControl.alpha = 0;
                canvasGroupToControl.interactable = false;
                canvasGroupToControl.blocksRaycasts = false;
            }           
        }

        // Closes only the most recent popup
        public void CloseLast()
        {
            if (history.Count == 0) return;
            if (currentAnimation != null) StopCoroutine(currentAnimation);

            GameObject panelToClose = history.Pop();
            currentAnimation = StartCoroutine(AnimatePopUp(panelToClose, 0f, Vector3.zero, closeCurve, true));

            CanvasGroupIsActive(canvasGroup, false);
        }

        // Closes everything and clears the history
        public void CloseAll()
        {
            if (history.Count == 0) return;
            if (currentAnimation != null) StopCoroutine(currentAnimation);

            // We close the entire history stack
            while (history.Count > 0)
            {
                GameObject p = history.Pop();
                // We use a separate simple routine for bulk closing to avoid animation conflicts
                StartCoroutine(SimpleFadeOut(p));
            }
        }

        private IEnumerator AnimatePopUp(GameObject targetPanel, float targetAlpha, Vector3 targetScale, AnimationCurve curve, bool deactivateAtEnd = false)
        {
            float time = 0;
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = targetPanel.transform.localScale;

            while (time < duration)
            {
                time += Time.deltaTime;
                float lerpFactor = time / duration;
                float curveValue = curve.Evaluate(lerpFactor);

                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerpFactor);
                targetPanel.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            targetPanel.transform.localScale = targetScale;

            if (deactivateAtEnd) targetPanel.SetActive(false);
            currentAnimation = null;
        }

        // Helper for CloseAll to handle multiple panels at once
        private IEnumerator SimpleFadeOut(GameObject p)
        {
            // Quick visual close without blocking the main animator
            p.transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0;
            p.SetActive(false);
            yield return null;
        }
    }
}