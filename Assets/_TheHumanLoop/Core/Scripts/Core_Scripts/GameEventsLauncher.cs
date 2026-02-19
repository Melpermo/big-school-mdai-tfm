using UnityEngine;
using HumanLoop.Events;
using UnityEngine.Rendering;

namespace TheHumanLoop.Core
{

    public class GameEventsLauncher : MonoBehaviour
    {
        public static GameEventsLauncher Instance { get; private set; }

        public GameEventSO[] gameEventsToLaunch;

        public bool launchOnStart = true;

        private void Start()
        {
            if (launchOnStart)
            {
                LaunchAllGameEvents();
            }
            else
            {
                Debug.Log("GameEventsLauncher is set to not launch on start. Use LaunchAllGameEvents() or LaunchGameEvents(GameEventSO) to launch events.");
            }

        }

        public void LaunchAllGameEvents()
        { 
           foreach (var gameEvent in gameEventsToLaunch)
            {
                gameEvent?.Raise();
                Debug.Log($"Launched Game Event: {gameEvent.name}");
            }
        }

        public void LaunchGameEvents(GameEventSO gameEventSO)
        {
            foreach (var gameEvent in gameEventsToLaunch)
            {
                if (gameEvent == gameEventSO)
                {
                    gameEvent?.Raise();
                    Debug.Log($"Launched Game Event: {gameEvent.name}");
                }
                    
            }
        }

    }
}
