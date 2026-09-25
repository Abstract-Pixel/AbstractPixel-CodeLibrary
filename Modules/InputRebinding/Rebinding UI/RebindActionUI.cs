using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using RebindOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace AbstractPixel.InputRebinding
{
    public class RebindActionUI : MonoBehaviour
    {
        public static Func<InputActionAsset> RuntimeInputAssetProvider { get; set; }

        public InputActionReference ActionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateBlankLabels();
                UpdateBindingDisplay();
            }
        }

        public string BindingId
        {
            get => m_BindingId;
            set
            {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions DisplayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        public TMP_Text ActionLabel
        {
            get => m_ActionLabel;
            set => m_ActionLabel = value;
        }

        public TMP_Text BindingText
        {
            get => m_BindingText;
            set => m_BindingText = value;
        }

        public TMP_Text RebindText
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        public TMP_Text rebindInfo
        {
            get => m_RebindInfo;
            set => m_RebindInfo = value;
        }

        public Button rebindCancelButton
        {
            get => m_RebindCancelButton;
            set => m_RebindCancelButton = value;
        }

        public float rebindTimeout
        {
            get => m_RebindTimeout;
            set => m_RebindTimeout = value;
        }

        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        public UpdateBindingUIEvent updateBindingUIEvent => m_UpdateBindingUIEvent;
        public InteractiveRebindEvent startRebindEvent => m_RebindStartEvent;
        public InteractiveRebindEvent stopRebindEvent => m_RebindStopEvent;

        public RebindOperation ongoingRebind => m_RebindOperation;

        [Header("Action & Binding References")]
        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField] private InputActionReference m_Action;

        [SerializeField] private string m_BindingId;

        [SerializeField] private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Header("UI Controls (TextMeshPro)")]
        [Tooltip("Text component that will receive the name of the action.")]
        [SerializeField] private TMP_Text m_ActionLabel;

        [Tooltip("Text component that will receive the current, formatted binding string.")]
        [SerializeField] private TMP_Text m_BindingText;

        [Tooltip("Optional text component that receives a prompt when waiting for a control to be actuated.")]
        [SerializeField] private TMP_Text m_RebindText;

        [Tooltip("Optional text component that receives additional info (e.g. 'Press Escape to cancel').")]
        [SerializeField] private TMP_Text m_RebindInfo;

        [Tooltip("Optional button that can be clicked to cancel a rebind.")]
        [SerializeField] private Button m_RebindCancelButton;

        [Tooltip("Optional timeout in seconds for rebind operations.")]
        [SerializeField] private float m_RebindTimeout = 0f;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField] private GameObject m_RebindOverlay;

        [Header("Events")]
        [SerializeField] private UpdateBindingUIEvent m_UpdateBindingUIEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStartEvent;
        [SerializeField] private InteractiveRebindEvent m_RebindStopEvent;

        private RebindOperation m_RebindOperation;

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string> { }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, RebindOperation> { }

        private void OnEnable()
        {
            UpdateBlankLabels();
            UpdateBindingDisplay();
        }

        private void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
        }

        private InputAction ResolveActionAndBinding(out int bindingIndex)
        {
            bindingIndex = -1;

            if (m_Action == null || m_Action.action == null)
                return null;

            InputActionAsset liveAsset = RuntimeInputAssetProvider?.Invoke();

            InputAction action = liveAsset != null
                ? liveAsset.FindAction(m_Action.action.id)
                : m_Action.action;

            if (action == null)
                return null;

            if (!string.IsNullOrEmpty(m_BindingId))
            {
                var id = new Guid(m_BindingId);
                bindingIndex = action.bindings.IndexOf(x => x.id == id);
            }

            return action;
        }

        public void ResetToDefault()
        {
            InputAction action = ResolveActionAndBinding(out var bindingIndex);
            if (action == null)
                return;

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }

            UpdateBindingDisplay();
        }

        public void StartInteractiveRebind()
        {
            InputAction action = ResolveActionAndBinding(out var bindingIndex);
            if (action == null)
                return;

            if (bindingIndex < 0)
            {
                Debug.LogError($"[RebindActionUI] Cannot find binding with ID '{m_BindingId}' on '{action}'.", this);
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebinding(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebinding(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebinding(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation and call CleanUp() via OnCancel callback.
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            action.Disable();

            if (m_RebindOverlay != null)
                m_RebindOverlay.SetActive(true);

            if (m_RebindText != null)
            {
                string partName = default(string);
                if (action.bindings[bindingIndex].isPartOfComposite)
                    partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

                m_RebindText.text = $"{partName}Waiting for input...";
            }

            if (m_RebindInfo != null)
            {
                m_RebindInfo.text = "Press Escape to cancel";
            }

            if (m_RebindCancelButton != null)
            {
                m_RebindCancelButton.onClick.AddListener(CancelRebind);
            }

            RebindOperation rebindConfig = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f);

            if (m_RebindTimeout > 0f)
            {
                rebindConfig.WithTimeout(m_RebindTimeout);
            }

            m_RebindOperation = rebindConfig
                .OnCancel(operation =>
                {
                    action.Enable();
                    m_RebindStopEvent?.Invoke(this, operation);
                    if (m_RebindOverlay != null)
                        m_RebindOverlay.SetActive(false);
                    UpdateBindingDisplay();
                    CleanUp();
                })
                .OnComplete(operation =>
                {
                    action.Enable();
                    m_RebindStopEvent?.Invoke(this, operation);
                    if (m_RebindOverlay != null)
                        m_RebindOverlay.SetActive(false);

                    if(CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                    {
                        action.RemoveBindingOverride(bindingIndex);
                        CleanUp();
                        PerformInteractiveRebinding(action, bindingIndex, allCompositeParts);
                        return;
                    }

                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                        {
                            PerformInteractiveRebinding(action, nextBindingIndex, true);
                            return;
                        }
                    }

                    UpdateBindingDisplay();
                    CleanUp();
                });

            m_RebindStartEvent?.Invoke(this, m_RebindOperation);
            m_RebindOperation.Start();
        }

        private bool CheckDuplicateBindings(InputAction _action, int _bindingIndex, bool _allCompositeParts = false)
        {
            InputBinding newBinding = _action.bindings[_bindingIndex];

            for (int i = 0; i < _action.actionMap.bindings.Count; i++)
            {
                if (_action.actionMap.bindings[i].action == newBinding.action)
                {
                    continue;
                }
    
                InputBinding existingBinding = _action.actionMap.bindings[i];
                if (existingBinding.effectivePath == newBinding.effectivePath)
                {
                    Debug.LogWarning($"Duplicate binding detected: {_action.name} has multiple bindings for {newBinding.effectivePath}");
                    return true;
                }
            }

            if(_allCompositeParts)
            {
                for(int i =1; i < _bindingIndex; i++)
                {
                    if (_action.bindings[i].effectivePath == newBinding.overridePath)
                    {
                        Debug.LogWarning($"Duplicate binding detected: {_action.name} has multiple bindings for {newBinding.overridePath}");
                        return true;
                    }
                }
            }
            return false;
        }

        private void CancelRebind()
        {
            m_RebindOperation?.Cancel();
        }

        public void UpdateBindingDisplay()
        {
            string displayString = string.Empty;
            string deviceLayoutName = default(string);
            string controlPath = default(string);

            InputAction action = ResolveActionAndBinding(out var bindingIndex);
            if (action != null && bindingIndex >= 0)
            {
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, m_DisplayStringOptions);
            }

            if (m_BindingText != null)
                m_BindingText.text = displayString;

            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        private void UpdateBlankLabels()
        {
            if (m_ActionLabel != null)
            {
                InputAction  action = ResolveActionAndBinding(out _);
                m_ActionLabel.text = action != null ? action.name : string.Empty;
            }
        }

        private void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
        }
    }
}