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
        [SerializeField] private StateSO targetState;

        [Tooltip("The actual UI GameObject to toggle. Do NOT assign the object this script is attached to, or it will stop listening when disabled!")]
        [SerializeField] private GameObject menuContent;

        [Tooltip("OnStateRegistered shows the menu at once. OnConfigurationsApplied waits for the state's execution delay.")]
        [SerializeField] private MenuRevealTiming revealTiming = MenuRevealTiming.OnStateRegistered;

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
            GameStateRegistry.OnStateRegistered += HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
            GameStateRegistry.OnStateConfigurationsApplied += HandleStateConfigurationsApplied;
        }

        private void OnDisable()
        {
            GameStateRegistry.OnStateRegistered -= HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
            GameStateRegistry.OnStateConfigurationsApplied -= HandleStateConfigurationsApplied;
        }

        private void Start()
        {
            // Initialization Sync: Check the registry the moment the scene starts.
            // Because StateComponents register in Awake(), the Registry is already populated.
            bool isTargetStateActive = GameStateRegistry.IsStateActive(targetState);
            if (revealTiming == MenuRevealTiming.OnConfigurationsApplied)
            {
                bool areConfigurationsApplied = GameStateRegistry.AreConfigurationsApplied(targetState);
                ToggleMenu(isTargetStateActive && areConfigurationsApplied);
                return;
            }
            ToggleMenu(isTargetStateActive);
        }

        private void HandleStateRegistered(StateSO _registeredState)
        {
            // ShowMenu
            if (revealTiming != MenuRevealTiming.OnStateRegistered)
            {
                return;
            }
            if (_registeredState == targetState)
            {
                ToggleMenu(true);
            }
        }

        private void HandleStateConfigurationsApplied(StateSO _appliedState)
        {
            if (revealTiming != MenuRevealTiming.OnConfigurationsApplied)
            {
                return;
            }
            if (_appliedState == targetState)
            {
                ToggleMenu(true);
            }
        }

        private void HandleStateUnregistered(StateSO _unregisteredState)
        {
            // HideMenu
            if (_unregisteredState== targetState)
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