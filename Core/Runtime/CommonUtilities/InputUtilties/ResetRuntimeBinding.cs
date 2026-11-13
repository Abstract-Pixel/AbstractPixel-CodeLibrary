using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    [DisallowMultipleComponent]
    public class ResetRuntimeBinding : MonoBehaviour
    {
        [Header("Asset Configuration")]
        [Tooltip("Optional direct reference to the InputActionAsset. If unassigned, the live asset provided by GlobalInput is used.")]
        [SerializeField] private InputActionAsset fallbackInputAsset;

        [Header("Button Auto-Hook")]
        [Tooltip("If true and a Button component is present on this GameObject, automatically hooks ResetToDefault to onClick.")]
        [SerializeField] private bool autoHookButtonOnClick = true;

        [SerializeField] private Button targetButton;

        public static InputActionAsset RuntimeAsset { get; private set; }
        public static event Action OnResetCompleted;

        private static string factoryDefaultOverridesJson = string.Empty;
        private static bool hasCapturedFactoryDefaults = false;

        public static void SetRuntimeAsset(InputActionAsset _asset)
        {
            RuntimeAsset = _asset;

            if (_asset != null && !hasCapturedFactoryDefaults)
            {
                factoryDefaultOverridesJson = _asset.SaveBindingOverridesAsJson();
                hasCapturedFactoryDefaults = true;
            }
        }

        private void Awake()
        {
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (autoHookButtonOnClick && targetButton != null)
            {
                targetButton.onClick.RemoveListener(ResetToDefault);
                targetButton.onClick.AddListener(ResetToDefault);
            }
        }

        private void OnDisable()
        {
            if (autoHookButtonOnClick && targetButton != null)
            {
                targetButton.onClick.RemoveListener(ResetToDefault);
            }
        }

        public void ResetToDefault()
        {
            InputActionAsset activeAsset = RuntimeAsset != null ? RuntimeAsset : fallbackInputAsset;

            if (activeAsset == null)
            {
                return;
            }

            activeAsset.RemoveAllBindingOverrides();

            if (!string.IsNullOrEmpty(factoryDefaultOverridesJson))
            {
                activeAsset.LoadBindingOverridesFromJson(factoryDefaultOverridesJson);
            }

            foreach (InputActionMap actionMap in activeAsset.actionMaps)
            {
                if (actionMap != null && actionMap.enabled)
                {
                    actionMap.Disable();
                    actionMap.Enable();
                }
            }

            OnResetCompleted?.Invoke();
        }
    }
}