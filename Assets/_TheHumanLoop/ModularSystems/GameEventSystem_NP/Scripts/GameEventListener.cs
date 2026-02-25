using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HumanLoop.Events
{
    public class GameEventListener : MonoBehaviour
    {
        /// <summary>
        /// Component that listens to a specific GameEvent ScriptableObject and triggers a UnityEvent.
        /// </summary>

        [SerializeField] private GameEventSO gameEvent;
        [SerializeField] private UnityEvent onEventRaised;
        [SerializeField] private float delayBeforeInvoke = 0f; // Optional delay before invoking the UnityEvent

        [Header("FOR WEB DEVELOMENT ISSUES")]
        [Tooltip("This is a workaround for web builds where coroutines can behave unpredictably. " +
                 "If you experience issues with delayed events in web builds, try enabling this option.")]
        //[SerializeField] private bool useWebBuildWorkaround = false;
        [SerializeField] private bool usingAsyncTaskMethods = false; // Optional: If you want to use async/await instead of coroutines for the delay

        private void OnEnable() => gameEvent.RegisterListener(this);
        private void OnDisable() => gameEvent.UnregisterListener(this);

        public void OnEventRaised()
        {
            EventResponseHandler();
        }

        private void EventResponseHandler()
        {
            if (gameEvent != null)
            {
                if (!usingAsyncTaskMethods)
                {
                    StartCoroutine(InvokeWithDelay(delayBeforeInvoke));
                }
                else
                {
                    using var _ = InvokeWithDelayAsync(delayBeforeInvoke);
                }                
            }
            else
            {
                //Debug.LogWarning("GameEventListener: No GameEvent assigned to " + gameObject.name);
                return;
            }
        }

        private async Task InvokeWithDelayAsync(float delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            onEventRaised?.Invoke();
            await Task.Yield(); // Ensure we yield back to the main thread after invoking
        }

        IEnumerator InvokeWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            onEventRaised?.Invoke();
            StopAllCoroutines(); // Ensure we don't have multiple pending invokes if the event is raised multiple times
        }
    }
}
