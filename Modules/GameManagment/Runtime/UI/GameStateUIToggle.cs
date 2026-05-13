using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// A reactive UI component that toggles general Game UI (like HUDs, Health Bars) 
    /// based on the ShowGameUIOnExecution boolean of the currently active StateSO.
    /// </summary>
    public class GameStateUIToggle : MonoBehaviour
    {
        [Header("UI Configuration")]
        [Tooltip("The actual UI GameObject to toggle. Do NOT assign the object this script is attached to!")]
        [SerializeField] private GameObject uiContent;


        private void OnEnable()
        {
            GameStateRegistry.OnStateRegistered += HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
        }

        private void OnDisable()
        {
            GameStateRegistry.OnStateRegistered -= HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
        }

        private void Start()
        {
            StateSO currentState = GameStateRegistry.GetCurrentHighestState();

            if (currentState != null)
            {
                ToggleUI(currentState.ShowGameUIOnExecution);
            }
            else
            {
                // Fallback if no state is active yet (e.g., loading screen)
                ToggleUI(false);
            }
        }

        private void HandleStateRegistered(StateSO _registeredState)
        {
            ToggleUI(_registeredState.ShowGameUIOnExecution);
        }

        private void HandleStateUnregistered(StateSO _unregisteredState)
        {
            // Inverse
            ToggleUI(_unregisteredState.ShowGameUIOnDeactivation);
        }

        private void ToggleUI(bool _shouldShow)
        {
            if (uiContent != null)
            {
                if (uiContent.activeSelf != _shouldShow)
                {
                    uiContent.SetActive(_shouldShow);
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] GameStateUIToggle cannot toggle because uiContent is not assigned!");
            }
        }
    }
}