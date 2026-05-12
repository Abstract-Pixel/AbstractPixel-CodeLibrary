// --- START OF FILE StateConditionRegistry.cs ---
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public static class StateConditionRegistry
    {
        private static Dictionary<StateSO, List<BaseCondition>> conditionDict = new Dictionary<StateSO, List<BaseCondition>>();

        public static event Action<StateSO, BaseCondition> OnConditionAdded = delegate { };
        public static event Action<StateSO, BaseCondition> OnConditionRemoved = delegate { };

        public static void RegisterCondition(StateSO _targetState, BaseCondition _condition)
        {
            if (_targetState == null || _condition == null) return;

            if (!conditionDict.ContainsKey(_targetState))
            {
                conditionDict[_targetState] = new List<BaseCondition>();
            }

            if (!conditionDict[_targetState].Contains(_condition))
            {
                conditionDict[_targetState].Add(_condition);
                OnConditionAdded?.Invoke(_targetState, _condition);
            }
        }

        public static void UnregisterCondition(StateSO _targetState, BaseCondition _condition)
        {
            if (_targetState == null || _condition == null) return;

            if (conditionDict.ContainsKey(_targetState))
            {
                if (conditionDict[_targetState].Remove(_condition))
                {
                    OnConditionRemoved?.Invoke(_targetState, _condition);
                }
            }
        }

        public static List<BaseCondition> GetConditionsForState(StateSO _targetState)
        {
            if (_targetState != null && conditionDict.ContainsKey(_targetState))
            {
                return conditionDict[_targetState];
            }
            return new List<BaseCondition>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            conditionDict = new Dictionary<StateSO, List<BaseCondition>>();
            OnConditionAdded = delegate { };
            OnConditionRemoved = delegate { };
        }
    }
}
// --- END OF FILE ---