using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// The local executor for a Game State. Listens to conditions and communicates with the GameStateRegistry.
    /// </summary>
    public class GameStateComponent : MonoBehaviour
    {
        [Header("State Configuration")]
        [Tooltip("The ScriptableObject defining the rules and priority of this state.")]
        [SerializeField] private StateSO stateConfig;

        [Header("Triggers")]
        [Tooltip("The condition that triggers this state. Automatically found if attached to this object or its children.")]
        [SerializeField] private BaseCondition stateCondition;

        private bool isActive = false;

        private void OnValidate()
        {
            if (stateCondition == null)
            {
                stateCondition = GetComponentInChildren<BaseCondition>();
            }
        }

        private void OnEnable()
        {
            if (stateCondition != null)
            {
                stateCondition.OnConditionMet += HandleConditionMet;
            }
        }

        private void OnDisable()
        {
            if (stateCondition != null)
            {
                stateCondition.OnConditionMet -= HandleConditionMet;
            }
        }

        public void ActivateState()
        {
            if (isActive) return;

            if (stateConfig == null)
            {
                Debug.LogError($"[{gameObject.name}] GameStateComponent cannot activate because StateSO is missing!");
                return;
            }

            bool isPermissionGranted = GameStateRegistry.TryRegisterAsActiveState(stateConfig);

            if (isPermissionGranted)
            {
                isActive = true;
                stateConfig.ApplyConfigurations();
            }
        }

        public void DeactivateState()
        {
            if (!isActive) return;

            isActive = false;
            stateConfig.RevertConfigurations();
            GameStateRegistry.UnregisterState(stateConfig);
        }

        private void HandleConditionMet(bool _isActivationTrigger)
        {
            if (_isActivationTrigger)
            {
                ActivateState();
            }
            else
            {
                DeactivateState();
            }
        }
    }
}