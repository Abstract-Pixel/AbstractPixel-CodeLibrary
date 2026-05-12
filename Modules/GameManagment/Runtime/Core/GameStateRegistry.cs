// --- START OF FILE GameStateRegistry.cs ---
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public static class GameStateRegistry
    {
        private static Dictionary<GameStateEvent, StateSO> activeStateDict = new Dictionary<GameStateEvent, StateSO>();

        public static event Action<StateSO> OnStateRegistered = delegate { };
        public static event Action<StateSO> OnStateUnregistered = delegate { };

        /// <summary>
        /// Attempts to register a state using its data. Automatically evicts lower priority states.
        /// </summary>
        public static bool TryRegisterAsActiveState(StateSO _stateData)
        {
            GameStateEvent incomingEvent = _stateData.GameStateEvent;
            int incomingPriority = _stateData.Priority;
            int highestActivePriority = GetHighestActivePriority();

            // Deny Entry: A higher or equal priority state is already running.
            if (incomingPriority < highestActivePriority)
            {
                return false;
            }

            // Prepare Eviction
            List<GameStateEvent> keysToRemove = new List<GameStateEvent>();
            List<StateSO> statesToEvict = new List<StateSO>();

            foreach (KeyValuePair<GameStateEvent, StateSO> kvp in activeStateDict)
            {
                if (kvp.Value.Priority < incomingPriority)
                {
                    keysToRemove.Add(kvp.Key);
                    statesToEvict.Add(kvp.Value);
                }
            }

            // Execute Eviction
            foreach (GameStateEvent key in keysToRemove)
            {
                activeStateDict.Remove(key);
            }

            // Broadcast unregistration so local MonoBehaviours can shut themselves down if they were evicted
            foreach (StateSO evictedState in statesToEvict)
            {
                OnStateUnregistered?.Invoke(evictedState);
            }

            // Register new state
            activeStateDict[incomingEvent] = _stateData;
            OnStateRegistered?.Invoke(_stateData);

            return true;
        }

        public static void UnregisterState(StateSO _stateData)
        {
            if (activeStateDict.ContainsKey(_stateData.GameStateEvent))
            {
                activeStateDict.Remove(_stateData.GameStateEvent);
                OnStateUnregistered?.Invoke(_stateData);
            }
        }

        public static bool IsStateActive(GameStateEvent _eventType)
        {
            return activeStateDict.ContainsKey(_eventType);
        }
        public static StateSO GetCurrentHighestState()
        {
            StateSO highestState = null;
            int highestPriority = -1;

            foreach (KeyValuePair<GameStateEvent, StateSO> kvp in activeStateDict)
            {
                if (kvp.Value.Priority > highestPriority)
                {
                    highestPriority = kvp.Value.Priority;
                    highestState = kvp.Value;
                }
            }
            return highestState;
        }

        private static int GetHighestActivePriority()
        {
            int highestPriority = -1;
            foreach (KeyValuePair<GameStateEvent, StateSO> kvp in activeStateDict)
            {
                if (kvp.Value.Priority > highestPriority)
                {
                    highestPriority = kvp.Value.Priority;
                }
            }
            return highestPriority;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            activeStateDict = new Dictionary<GameStateEvent, StateSO>();
            OnStateRegistered = delegate { };
            OnStateUnregistered = delegate { };
        }
    }
}