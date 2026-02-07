using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace HumanLoop.Events
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "The Human Loop/GameEvent/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> listeners = new List<GameEventListener>();

        public void Raise()
        {
            //In case in events response or a listeners response includes removing form the list 
            // that way you're removing items behaind you instead of removing items in front of you
            // so you can iterate throught the list without an out of range exception
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();                
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            listeners.Add(listener);
        }
        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }

    }
}
