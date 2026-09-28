using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public static class GameStateRegistry
    {
        private static HashSet<StateSO> activeStates = new HashSet<StateSO>();
        private static Stack<StateSO> stateHistory = new Stack<StateSO>();
        private static bool isProcessingStateChange = false;

        public static event Action<StateSO> OnStateRegistered = delegate { };
        public static event Action<StateSO> OnStateUnregistered = delegate { };
        public static event Action<StateSO> OnStateRestored = delegate { };

        public static bool TryRegisterAsActiveState(StateSO _stateData)
        {
            if (_stateData == null)
            {
                return false;
            }

            // [MODIFIED]: Guard against recursive event loops during eviction
            if (isProcessingStateChange)
            {
                return false;
            }

            if (activeStates.Contains(_stateData))
            {
                return true;
            }

            int incomingPriority = _stateData.Priority;
            int highestActivePriority = GetHighestActivePriority();

            if (incomingPriority < highestActivePriority)
            {
                return false;
            }

            isProcessingStateChange = true;
            try
            {
                if (_stateData.IsSubState)
                {
                    StateSO currentHighest = GetCurrentHighestState();
                    if (currentHighest != null)
                    {
                        stateHistory.Push(currentHighest);
                    }
                }
                else
                {
                    stateHistory.Clear();
                }

                List<StateSO> statesToEvict = new List<StateSO>();
                foreach (StateSO activeState in activeStates)
                {
                    if (activeState.Priority < incomingPriority)
                    {
                        statesToEvict.Add(activeState);
                    }
                }

                activeStates.Add(_stateData);

                foreach (StateSO evictedState in statesToEvict)
                {
                    activeStates.Remove(evictedState);
                    OnStateUnregistered?.Invoke(evictedState);
                }
            }
            finally
            {
                // [MODIFIED]: Release lock BEFORE notifying registered so valid downstream sub-states can enter
                isProcessingStateChange = false;
            }

            OnStateRegistered?.Invoke(_stateData);
            return true;
        }

        public static void UnregisterState(StateSO _stateData)
        {
            if (_stateData == null)
            {
                return;
            }

            if (activeStates.Remove(_stateData))
            {
                OnStateUnregistered?.Invoke(_stateData);

                if (_stateData.IsSubState && stateHistory.Count > 0)
                {
                    StateSO previousState = stateHistory.Pop();
                    OnStateRestored?.Invoke(previousState);
                }
            }
        }

        public static bool IsStateActive(StateSO _stateData)
        {
            if (_stateData == null)
            {
                return false;
            }
            return activeStates.Contains(_stateData);
        }

        public static StateSO GetCurrentHighestState()
        {
            StateSO highestState = null;
            int highestPriority = -1;

            foreach (StateSO activeState in activeStates)
            {
                if (activeState.Priority > highestPriority)
                {
                    highestPriority = activeState.Priority;
                    highestState = activeState;
                }
            }
            return highestState;
        }

        private static int GetHighestActivePriority()
        {
            int highestPriority = -1;

            foreach (StateSO activeState in activeStates)
            {
                if (activeState.Priority > highestPriority)
                {
                    highestPriority = activeState.Priority;
                }
            }
            return highestPriority;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            activeStates = new HashSet<StateSO>();
            stateHistory = new Stack<StateSO>();
            isProcessingStateChange = false;
            OnStateRegistered = delegate { };
            OnStateUnregistered = delegate { };
            OnStateRestored = delegate { };
        }
    }
}