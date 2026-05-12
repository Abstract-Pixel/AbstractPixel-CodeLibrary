// --- START OF FILE GameStateMenu.cs ---
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// A reactive UI component that automatically shows or hides its content 
    /// based on the currently active GameState in the GameStateRegistry.
    /// </summary>
    public class GameStateMenu : MonoBehaviour
    {
        [Header("Menu Configuration")]
        [Tooltip("The specific state event that should cause this menu to appear.")]
        [SerializeField] private GameStateEvent targetStateEvent;

        [Tooltip("The actual UI GameObject to toggle. Do NOT assign the object this script is attached to, or it will stop listening when disabled!")]
        [SerializeField] private GameObject menuContent;

        private void OnValidate()
        {
            // Designer-Proofing: If the designer forgets to assign the content, 
            // try to grab the first child object automatically.
            if (menuContent == null && transform.childCount > 0)
            {
                menuContent = transform.GetChild(0).gameObject;
            }
        }

        private void OnEnable()
        {
            // C# Standard for preventing double-subscription: 
            // We strictly subscribe in OnEnable and unsubscribe in OnDisable.
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
            // Initialization Sync: Check the registry the moment the scene starts.
            // Because StateComponents register in Awake(), the Registry is already populated.
            bool isTargetStateActive = GameStateRegistry.IsStateActive(targetStateEvent);
            ToggleMenu(isTargetStateActive);
        }

        private void HandleStateRegistered(StateSO _registeredState)
        {
            // ShowMenu
            if (_registeredState.GameStateEvent == targetStateEvent)
            {
                ToggleMenu(true);
            }
        }

        private void HandleStateUnregistered(StateSO _unregisteredState)
        {
            // HideMenu
            if (_unregisteredState.GameStateEvent == targetStateEvent)
            {
                ToggleMenu(false);
            }
        }

        private void ToggleMenu(bool _shouldShow)
        {
            if (menuContent != null)
            {
                // Only apply the change if it's different, to save slight performance overhead
                if (menuContent.activeSelf != _shouldShow)
                {
                    menuContent.SetActive(_shouldShow);
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] GameStateMenu cannot toggle because MenuContent is not assigned!");
            }
        }
    }
}