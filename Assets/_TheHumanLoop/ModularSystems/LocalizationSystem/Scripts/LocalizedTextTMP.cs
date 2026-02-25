using System.Collections;
using TMPro;
using UnityEngine;

namespace HumanLoop.LocalizationSystem
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextTMP : MonoBehaviour
    {
        [SerializeField] private string _id;

        private TMP_Text _text;
        private bool _isBound;

        public static LocalizationService Service { get; set; }

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TryBindOrRetry();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void SetId(string id)
        {
            _id = id;
            Refresh();
        }

        // Try binding to the service, or retry in the next frame if the service is not ready.
        private void TryBindOrRetry()
        {
            if (_isBound) return;

            if (Service == null)
            {
                // Retry in the next frame (avoids race condition by order of Awake/OnEnable)
                StartCoroutine(BindNextFrame());
                return;
            }

            Bind();
            Refresh();
        }

        // Wait a frame before attempting to bind, useful if the service is initialized on Awake from another object
        private IEnumerator BindNextFrame()
        {
            yield return null;

            if (!isActiveAndEnabled) yield break;
            if (Service == null) yield break;

            Bind();
            Refresh();
        }

        // Bind to the service's LanguageChanged event to refresh text when language changes
        private void Bind()
        {
            if (_isBound) return;
            Service.LanguageChanged += Refresh;
            _isBound = true;
        }

        // Unbind from the service to avoid memory leaks or null reference exceptions
        private void Unbind()
        {
            if (!_isBound) return;
            if (Service != null) Service.LanguageChanged -= Refresh;
            _isBound = false;
        }

        // Refresh the displayed text based on the current language and the assigned ID
        private void Refresh()
        {
            if (Service == null || _text == null) return;
            _text.text = Service.Get(_id);
        }
    }
}