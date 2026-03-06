using HumanLoop.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheHumanLoop.Debugging
{
    public class BugTestings : MonoBehaviour
    {
        public GameObject objectToShow;
        public bool act_or_deact;
        public float afterTime;

        bool hasBeenCalled = false;

        [SerializeField] private SceneStateManager _sceneStateMamager;

        public void DirectCall()
        {
            Invoke(nameof(InvokeActDeact), afterTime);
        }

        public void ResetGameAfterTime()
        {
            Invoke(nameof(ResetGameAfterTimeCoroutine), afterTime);
        }

        public void ActDeactGameObgectAfter(bool act_or_deact,GameObject objectToShow, float afterTime)
        { 
            Invoke(nameof(ActDeactGameObgectAfterCoroutine), afterTime);
        }

        private void ActDeactGameObgectAfterCoroutine(bool act_or_deact, GameObject objectToShow, float afterTime)
        {
            objectToShow.SetActive(act_or_deact);
        }

        private void InvokeActDeact()
        {
            objectToShow.SetActive(act_or_deact);
        }

        private void ResetGameAfterTimeCoroutine()
        {
            _sceneStateMamager.RestartGame();
        }

        public void ToggleHasBeingnCalled()
        { 
            hasBeenCalled = !hasBeenCalled; 
            if (hasBeenCalled)
            {
                _sceneStateMamager.RestartGame();
            }
        }
        
        
        public void CallResetGameOnce()
        {
            hasBeenCalled = !hasBeenCalled;

            if (!hasBeenCalled)
            {
                _sceneStateMamager.RestartGame();
                hasBeenCalled = true;
            }
        }


    }
}
