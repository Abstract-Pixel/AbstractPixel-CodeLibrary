using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using RebindOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace AbstractPixel.InputRebinding
{
    [DisallowMultipleComponent]
    public class RebindAction : MonoBehaviour
    {
        [Header("Action Configuration")]
        [Tooltip("Reference to the InputAction from your project asset.")]
        [SerializeField] private InputActionReference actionReference;

        [Tooltip("Formatting options for the display string.")]
        [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

        [Tooltip("Optional timeout in seconds for interactive rebinding.")]
        [SerializeField] private float rebindTimeout = 0.0f;

        public static Func<InputActionAsset> RuntimeInputAssetProvider { get; set; }
        public static Func<InputActionAsset> RuntimeAssetProvider
        {
            get => RuntimeInputAssetProvider;
            set => RuntimeInputAssetProvider = value;
        }

        public static event Action OnAnyBindingChanged;

        public static void BroadcastBindingChanged()
        {
            OnAnyBindingChanged?.Invoke();
        }

        public event Action<BindingDisplayPayload> OnDisplayUpdated;
        public event Action<RebindOverlayPayload> OnOverlayUpdated;

        public InputActionReference ActionReference => actionReference;

        private int currentActiveBindingIndex = -1;
        private DeviceFamily currentLockedDeviceFamily = DeviceFamily.Unknown;
        private InputDevice currentLockedInputDevice;
        private RebindOperation ongoingRebindOperation;
        private float rebindCooldownEndTime = 0.0f;
        private Coroutine activeRebindRoutine;

        private Dictionary<int, string> preRebindBackupPaths = new Dictionary<int, string>();
        private Dictionary<string, string> crossActionSwappedBackups = new Dictionary<string, string>();

        private bool hasActiveSwapNotification = false;
        private string activeSwapHeader = string.Empty;
        private string activeSwapDetails = string.Empty;

        private static InputActionAsset cachedDiscoveredRuntimeAsset;
        private List<InputActionMap> silencedUiActionMaps = new List<InputActionMap>();

        private const string KEYBOARD_MATCH_PATH = "<Keyboard>";
        private const string MOUSE_MATCH_PATH = "<Mouse>";
        private const string GAMEPAD_MATCH_PATH = "<Gamepad>";
        private const string KEYBOARD_CANCEL_PATH = "<Keyboard>/escape";
        private const string GAMEPAD_CANCEL_PATH = "<Gamepad>/start";

        private const string EXCLUDE_POINTER_POSITION = "<Pointer>/position";
        private const string EXCLUDE_MOUSE_DELTA = "<Mouse>/delta";
        private const string EXCLUDE_MOUSE_SCROLL = "<Mouse>/scroll";

        private const string TRIGGER_BUTTON_SUFFIX = "Button";
        private const float TRIGGER_ACTUATION_THRESHOLD = 0.5f;
        private const float CONTROL_RELEASE_THRESHOLD = 0.15f;
        private const float REBIND_COOLDOWN_DURATION = 0.25f;
        private const float COMPOSITE_SETTLE_SAFETY_DELAY = 0.2f;
        private const float CONFIRMATION_NORMAL_DISPLAY_DURATION = 0.35f;
        private const float CONFIRMATION_SWAP_DISPLAY_DURATION = 0.85f;
        private const float RELEASE_WAIT_TIMEOUT_SECONDS = 5.0f;

        private void OnEnable()
        {
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;
            ResolveAndRefreshForActiveDevice();
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            StopActiveRebindRoutine();
            CleanUpRebindOperation();
            RestoreGlobalInputAndUi();
        }

        public void ResolveAndRefreshForActiveDevice()
        {
            InputAction resolvedAction = ResolveRuntimeAction();
            if (resolvedAction == null)
            {
                return;
            }

            bool isGamepadActive = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            currentActiveBindingIndex = FindMatchingBindingIndex(resolvedAction, isGamepadActive);

            PublishDisplayPayload(resolvedAction, currentActiveBindingIndex);
        }

        public void StartInteractiveRebind()
        {
            if (Time.unscaledTime < rebindCooldownEndTime)
            {
                return;
            }

            InputAction resolvedAction = ResolveRuntimeAction();
            if (resolvedAction == null)
            {
                return;
            }

            bool isGamepadActive = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            currentActiveBindingIndex = FindMatchingBindingIndex(resolvedAction, isGamepadActive);

            if (currentActiveBindingIndex < 0 || currentActiveBindingIndex >= resolvedAction.bindings.Count)
            {
                return;
            }

            currentLockedDeviceFamily = InputDeviceTracker.CurrentDeviceFamily;
            currentLockedInputDevice = InputDeviceTracker.LastUsedDevice;

            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            SilenceAllUiMapsAcrossEngine();

            hasActiveSwapNotification = false;
            activeSwapHeader = string.Empty;
            activeSwapDetails = string.Empty;
            preRebindBackupPaths.Clear();
            crossActionSwappedBackups.Clear();

            InputBinding targetBinding = resolvedAction.bindings[currentActiveBindingIndex];
            if (targetBinding.isComposite)
            {
                for (int i = currentActiveBindingIndex + 1; i < resolvedAction.bindings.Count && resolvedAction.bindings[i].isPartOfComposite; ++i)
                {
                    preRebindBackupPaths[i] = resolvedAction.bindings[i].effectivePath;
                }

                int firstPartIndex = currentActiveBindingIndex + 1;
                if (firstPartIndex < resolvedAction.bindings.Count && resolvedAction.bindings[firstPartIndex].isPartOfComposite)
                {
                    ExecuteRebindPipeline(resolvedAction, firstPartIndex, true, currentActiveBindingIndex);
                    return;
                }
            }
            else
            {
                preRebindBackupPaths[currentActiveBindingIndex] = resolvedAction.bindings[currentActiveBindingIndex].effectivePath;
            }

            ExecuteRebindPipeline(resolvedAction, currentActiveBindingIndex, false, -1);
        }

        public void ResetToDefault()
        {
            if (Time.unscaledTime < rebindCooldownEndTime)
            {
                return;
            }

            InputAction resolvedAction = ResolveRuntimeAction();
            if (resolvedAction == null)
            {
                return;
            }

            bool isGamepadActive = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            int bindingIndexToReset = FindMatchingBindingIndex(resolvedAction, isGamepadActive);

            if (bindingIndexToReset < 0 || bindingIndexToReset >= resolvedAction.bindings.Count)
            {
                return;
            }

            InputBinding targetBinding = resolvedAction.bindings[bindingIndexToReset];
            if (targetBinding.isComposite)
            {
                for (int index = bindingIndexToReset + 1; index < resolvedAction.bindings.Count && resolvedAction.bindings[index].isPartOfComposite; ++index)
                {
                    ResolveResetConflicts(resolvedAction, index);
                    ApplyOverrideSafelyToAllInstances(resolvedAction, index, string.Empty, true);
                }
            }
            else
            {
                ResolveResetConflicts(resolvedAction, bindingIndexToReset);
                ApplyOverrideSafelyToAllInstances(resolvedAction, bindingIndexToReset, string.Empty, true);
            }

            CommitActionMapReboot(resolvedAction);
            ResolveAndRefreshForActiveDevice();
            OnAnyBindingChanged?.Invoke();
        }

        private void ExecuteRebindPipeline(
            InputAction _action,
            int _bindingIndex,
            bool _allCompositeParts,
            int _compositeRootIndex)
        {
            CleanUpRebindOperation();
            _action.Disable();

            bool isGamepadLocked = currentLockedDeviceFamily != DeviceFamily.KeyboardMouse;
            string matchPath = isGamepadLocked ? GAMEPAD_MATCH_PATH : KEYBOARD_MATCH_PATH;
            string cancelPath = isGamepadLocked ? GAMEPAD_CANCEL_PATH : KEYBOARD_CANCEL_PATH;

            string friendlyDeviceName = ResolveDeviceFamilyFriendlyName(currentLockedDeviceFamily);
            string actionDisplayName = BuildActionDisplayName(_action, _bindingIndex);
            string statusPrompt = BuildStatusPrompt(_action, _bindingIndex, _allCompositeParts, _compositeRootIndex);
            string initialProgressPreview = _allCompositeParts ? BuildCompositeProgressString(_action, _compositeRootIndex, _bindingIndex, string.Empty) : string.Empty;

            PublishOverlayPayload(
                true,
                friendlyDeviceName,
                actionDisplayName,
                statusPrompt,
                initialProgressPreview,
                hasActiveSwapNotification,
                activeSwapHeader,
                activeSwapDetails);

            RebindOperation operationBuilder = _action.PerformInteractiveRebinding(_bindingIndex)
                .WithMatchingEventsBeingSuppressed(false)
                .WithCancelingThrough(cancelPath)
                .WithControlsHavingToMatchPath(matchPath)
                .WithMagnitudeHavingToBeGreaterThan(isGamepadLocked ? TRIGGER_ACTUATION_THRESHOLD : 0.2f)
                .OnMatchWaitForAnother(0.1f);

            operationBuilder.OnApplyBinding((_operation, _path) => { });

            if (!isGamepadLocked)
            {
                operationBuilder.WithControlsHavingToMatchPath(MOUSE_MATCH_PATH);
                operationBuilder.WithControlsExcluding(EXCLUDE_POINTER_POSITION);
                operationBuilder.WithControlsExcluding(EXCLUDE_MOUSE_DELTA);
                operationBuilder.WithControlsExcluding(EXCLUDE_MOUSE_SCROLL);
            }

            if (rebindTimeout > 0.0f)
            {
                operationBuilder.WithTimeout(rebindTimeout);
            }

            ongoingRebindOperation = operationBuilder
                .OnPotentialMatch(_operation =>
                {
                    if (_operation.selectedControl == null)
                    {
                        return;
                    }

                    InputControl candidateRawControl = _operation.selectedControl;
                    InputDevice candidateDevice = candidateRawControl.device;

                    if (isGamepadLocked)
                    {
                        if (!(candidateDevice is Gamepad || candidateDevice is Joystick))
                        {
                            _operation.RemoveCandidate(candidateRawControl);
                            return;
                        }

                        if (currentLockedInputDevice != null && candidateDevice != currentLockedInputDevice)
                        {
                            _operation.RemoveCandidate(candidateRawControl);
                            return;
                        }
                    }
                    else
                    {
                        if (!(candidateDevice is Keyboard || candidateDevice is Mouse))
                        {
                            _operation.RemoveCandidate(candidateRawControl);
                            return;
                        }
                    }

                    InputControl candidateSpecificControl = ResolveSpecificControl(candidateRawControl);
                    if (candidateSpecificControl == null)
                    {
                        return;
                    }

                    hasActiveSwapNotification = false;
                    activeSwapHeader = string.Empty;
                    activeSwapDetails = string.Empty;

                    string canonicalPreviewPath = ConvertToCanonicalPath(candidateSpecificControl);
                    string humanControlName = InputControlPath.ToHumanReadableString(canonicalPreviewPath, InputControlPath.HumanReadableStringOptions.OmitDevice);

                    string progressPreview = _allCompositeParts
                        ? BuildCompositeProgressString(_action, _compositeRootIndex, _bindingIndex, canonicalPreviewPath)
                        : humanControlName;

                    bool hasConflictWarning = false;
                    string previewHeader = string.Empty;
                    string previewDetails = string.Empty;

                    if (_allCompositeParts && CheckInternalCompositeConflict(_action, _bindingIndex, _compositeRootIndex, candidateSpecificControl, canonicalPreviewPath, out string internalPartName))
                    {
                        hasConflictWarning = true;
                        previewHeader = "<color=#FF5555>DUPLICATE BLOCKED</color>";
                        previewDetails = $"[{humanControlName}] is already assigned to {internalPartName}!";
                    }
                    else if (DetectCrossActionConflict(_action, candidateSpecificControl, canonicalPreviewPath, out string conflictingActionName))
                    {
                        hasConflictWarning = true;
                        previewHeader = "<color=#FFCC00>CONFLICT DETECTED</color>";
                        string preRebindPath = GetPreRebindPathForPart(_action, _bindingIndex);
                        string oldKeyHuman = InputControlPath.ToHumanReadableString(preRebindPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        previewDetails = $"[{humanControlName}] is currently used by '{conflictingActionName}'. Release to swap '{conflictingActionName}' to [{oldKeyHuman}].";
                    }

                    PublishOverlayPayload(
                        true,
                        friendlyDeviceName,
                        actionDisplayName,
                        "Hold to Preview, Release to Confirm",
                        progressPreview,
                        hasConflictWarning,
                        previewHeader,
                        previewDetails);
                })
                .OnCancel(_operation =>
                {
                    InputControl cancelControl = _operation.selectedControl;
                    CleanUpRebindOperation();
                    _action.Enable();
                    StopActiveRebindRoutine();
                    RevertAllBindingsToPreRebindBackup(_action);
                    activeRebindRoutine = StartCoroutine(FinalizeRebindRoutine(_action, cancelControl, false));
                })
                .OnComplete(_operation =>
                {
                    InputControl rawCapturedControl = _operation.selectedControl;
                    if (rawCapturedControl == null)
                    {
                        CleanUpRebindOperation();
                        _action.Enable();
                        RevertAllBindingsToPreRebindBackup(_action);
                        RestoreDeviceTrackingAndCloseOverlay();
                        return;
                    }

                    InputControl specificCapturedControl = ResolveSpecificControl(rawCapturedControl);
                    string resolvedCanonicalPath = ConvertToCanonicalPath(specificCapturedControl);

                    CleanUpRebindOperation();
                    StopActiveRebindRoutine();

                    activeRebindRoutine = StartCoroutine(WaitForReleaseAndConfirm(
                        _action,
                        _bindingIndex,
                        _allCompositeParts,
                        _compositeRootIndex,
                        specificCapturedControl,
                        resolvedCanonicalPath));
                });

            ongoingRebindOperation.Start();
        }

        private IEnumerator WaitForReleaseAndConfirm(
            InputAction _action,
            int _bindingIndex,
            bool _allCompositeParts,
            int _compositeRootIndex,
            InputControl _capturedControl,
            string _canonicalPath)
        {
            if (_capturedControl != null)
            {
                float timeoutTimestamp = Time.unscaledTime + RELEASE_WAIT_TIMEOUT_SECONDS;
                while (IsControlActuated(_capturedControl) && Time.unscaledTime < timeoutTimestamp)
                {
                    yield return null;
                }
            }

            string humanControlName = InputControlPath.ToHumanReadableString(_canonicalPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            string friendlyDeviceName = ResolveDeviceFamilyFriendlyName(currentLockedDeviceFamily);
            string actionDisplayName = BuildActionDisplayName(_action, _bindingIndex);

            if (_allCompositeParts && CheckInternalCompositeConflict(_action, _bindingIndex, _compositeRootIndex, _capturedControl, _canonicalPath, out string conflictingPartName))
            {
                string currentBreadcrumb = BuildCompositeProgressString(_action, _compositeRootIndex, _bindingIndex, string.Empty);
                hasActiveSwapNotification = true;
                activeSwapHeader = "<color=#FF5555>DUPLICATE BLOCKED</color>";
                activeSwapDetails = $"[{humanControlName}] is already assigned to {conflictingPartName}! Choose a different input.";

                PublishOverlayPayload(
                    true,
                    friendlyDeviceName,
                    actionDisplayName,
                    "Duplicate Blocked!",
                    currentBreadcrumb,
                    true,
                    activeSwapHeader,
                    activeSwapDetails);

                yield return new WaitForSecondsRealtime(1.0f);
                ExecuteRebindPipeline(_action, _bindingIndex, true, _compositeRootIndex);
                yield break;
            }

            ApplyOverrideSafelyToAllInstances(_action, _bindingIndex, _canonicalPath, false);

            string preRebindPath = GetPreRebindPathForPart(_action, _bindingIndex);
            bool hasSwapped = CheckCrossActionConflictAndSwap(_action, _bindingIndex, _capturedControl, preRebindPath, _canonicalPath, out string conflictDescription);

            if (hasSwapped)
            {
                hasActiveSwapNotification = true;
                activeSwapHeader = "<color=#00FF88>SWAP APPLIED</color>";
                activeSwapDetails = conflictDescription;

                PublishOverlayPayload(
                    true,
                    friendlyDeviceName,
                    actionDisplayName,
                    "Binding Confirmed",
                    humanControlName,
                    true,
                    activeSwapHeader,
                    activeSwapDetails);
            }
            else
            {
                PublishOverlayPayload(
                    true,
                    friendlyDeviceName,
                    actionDisplayName,
                    "Binding Confirmed",
                    humanControlName,
                    false,
                    string.Empty,
                    string.Empty);
            }

            if (_allCompositeParts)
            {
                int nextPartIndex = _bindingIndex + 1;
                if (nextPartIndex < _action.bindings.Count && _action.bindings[nextPartIndex].isPartOfComposite)
                {
                    yield return new WaitForSecondsRealtime(COMPOSITE_SETTLE_SAFETY_DELAY);
                    ExecuteRebindPipeline(_action, nextPartIndex, true, _compositeRootIndex);
                    yield break;
                }

                if (!ValidateCompleteCompositeIntegrity(_action, _compositeRootIndex, out int duplicatePartIndex))
                {
                    ApplyOverrideSafelyToAllInstances(_action, duplicatePartIndex, string.Empty, true);
                    yield return new WaitForSecondsRealtime(COMPOSITE_SETTLE_SAFETY_DELAY);
                    ExecuteRebindPipeline(_action, duplicatePartIndex, true, _compositeRootIndex);
                    yield break;
                }
            }

            float settleDwellDuration = hasSwapped ? CONFIRMATION_SWAP_DISPLAY_DURATION : CONFIRMATION_NORMAL_DISPLAY_DURATION;
            yield return new WaitForSecondsRealtime(settleDwellDuration);

            _action.Enable();
            CommitActionMapReboot(_action);
            activeRebindRoutine = StartCoroutine(FinalizeRebindRoutine(_action, _capturedControl, true));
        }

        private InputControl ResolveSpecificControl(InputControl _control)
        {
            if (_control == null)
            {
                return null;
            }

            if (_control is DpadControl dpadControl)
            {
                Vector2 dpadVector = dpadControl.ReadValue();
                if (Mathf.Abs(dpadVector.x) > Mathf.Abs(dpadVector.y) && Mathf.Abs(dpadVector.x) > CONTROL_RELEASE_THRESHOLD)
                {
                    return dpadVector.x > 0.0f ? dpadControl.right : dpadControl.left;
                }
                if (Mathf.Abs(dpadVector.y) > CONTROL_RELEASE_THRESHOLD)
                {
                    return dpadVector.y > 0.0f ? dpadControl.up : dpadControl.down;
                }
            }

            if (string.Equals(_control.name, "x", StringComparison.OrdinalIgnoreCase) && _control.parent is DpadControl parentDpadX)
            {
                float xVal = parentDpadX.x.ReadValue();
                return xVal > 0.0f ? parentDpadX.right : parentDpadX.left;
            }

            if (string.Equals(_control.name, "y", StringComparison.OrdinalIgnoreCase) && _control.parent is DpadControl parentDpadY)
            {
                float yVal = parentDpadY.y.ReadValue();
                return yVal > 0.0f ? parentDpadY.up : parentDpadY.down;
            }

            if (_control is StickControl stickControl)
            {
                Vector2 stickVector = stickControl.ReadValue();
                if (Mathf.Abs(stickVector.x) > Mathf.Abs(stickVector.y) && Mathf.Abs(stickVector.x) > CONTROL_RELEASE_THRESHOLD)
                {
                    return stickVector.x > 0.0f ? stickControl.right : stickControl.left;
                }
                if (Mathf.Abs(stickVector.y) > CONTROL_RELEASE_THRESHOLD)
                {
                    return stickVector.y > 0.0f ? stickControl.up : stickControl.down;
                }
            }

            if (string.Equals(_control.name, "x", StringComparison.OrdinalIgnoreCase) && _control.parent is StickControl parentStickX)
            {
                float xVal = parentStickX.x.ReadValue();
                return xVal > 0.0f ? parentStickX.right : parentStickX.left;
            }

            if (string.Equals(_control.name, "y", StringComparison.OrdinalIgnoreCase) && _control.parent is StickControl parentStickY)
            {
                float yVal = parentStickY.y.ReadValue();
                return yVal > 0.0f ? parentStickY.up : parentStickY.down;
            }

            return _control;
        }

        private bool IsControlActuated(InputControl _control)
        {
            if (_control == null)
            {
                return false;
            }

            try
            {
                if (_control is ButtonControl buttonControl)
                {
                    if (buttonControl.isPressed || buttonControl.ReadValue() > CONTROL_RELEASE_THRESHOLD)
                    {
                        return true;
                    }
                }

                if (_control is AxisControl axisControl)
                {
                    if (Mathf.Abs(axisControl.ReadValue()) > CONTROL_RELEASE_THRESHOLD)
                    {
                        return true;
                    }
                }

                if (_control is Vector2Control vector2Control)
                {
                    if (vector2Control.ReadValue().sqrMagnitude > CONTROL_RELEASE_THRESHOLD * CONTROL_RELEASE_THRESHOLD)
                    {
                        return true;
                    }
                }

                if (_control.EvaluateMagnitude() > CONTROL_RELEASE_THRESHOLD)
                {
                    return true;
                }

                if (_control.IsPressed(CONTROL_RELEASE_THRESHOLD))
                {
                    return true;
                }

                object valueObject = _control.ReadValueAsObject();
                if (valueObject is float floatValue && Mathf.Abs(floatValue) > CONTROL_RELEASE_THRESHOLD)
                {
                    return true;
                }

                if (valueObject is Vector2 vectorValue && vectorValue.sqrMagnitude > CONTROL_RELEASE_THRESHOLD * CONTROL_RELEASE_THRESHOLD)
                {
                    return true;
                }

                if (_control.parent != null && !(_control.parent is InputDevice))
                {
                    if (_control.parent.EvaluateMagnitude() > CONTROL_RELEASE_THRESHOLD)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void RevertAllBindingsToPreRebindBackup(InputAction _action)
        {
            foreach (KeyValuePair<int, string> pair in preRebindBackupPaths)
            {
                ApplyOverrideSafelyToAllInstances(_action, pair.Key, pair.Value, false);
            }

            foreach (KeyValuePair<string, string> swapPair in crossActionSwappedBackups)
            {
                InputAction target = _action.actionMap?.FindAction(swapPair.Key);
                if (target != null)
                {
                    bool isGamepad = currentLockedDeviceFamily != DeviceFamily.KeyboardMouse;
                    int targetIndex = FindMatchingBindingIndex(target, isGamepad);
                    if (targetIndex >= 0)
                    {
                        ApplyOverrideSafelyToAllInstances(target, targetIndex, swapPair.Value, false);
                    }
                }
            }

            preRebindBackupPaths.Clear();
            crossActionSwappedBackups.Clear();
            hasActiveSwapNotification = false;
            activeSwapHeader = string.Empty;
            activeSwapDetails = string.Empty;

            CommitActionMapReboot(_action);
            ResolveAndRefreshForActiveDevice();
        }

        private string GetPreRebindPathForPart(InputAction _action, int _bindingIndex)
        {
            if (preRebindBackupPaths.TryGetValue(_bindingIndex, out string path))
            {
                return path;
            }

            return _action.bindings[_bindingIndex].effectivePath;
        }

        private void ApplyOverrideSafelyToAllInstances(
            InputAction _sourceAction,
            int _sourceBindingIndex,
            string _overridePath,
            bool _isReset)
        {
            if (_sourceBindingIndex < 0 || _sourceBindingIndex >= _sourceAction.bindings.Count)
            {
                return;
            }

            InputBinding sourceBinding = _sourceAction.bindings[_sourceBindingIndex];
            if (sourceBinding.isComposite)
            {
                return;
            }

            Guid targetBindingId = sourceBinding.id;
            Guid actionId = _sourceAction.id;

            InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            for (int i = 0; i < allAssets.Length; ++i)
            {
                InputAction targetAction = allAssets[i].FindAction(actionId);
                if (targetAction != null)
                {
                    int matchIndex = -1;
                    for (int b = 0; b < targetAction.bindings.Count; ++b)
                    {
                        if (targetAction.bindings[b].id == targetBindingId)
                        {
                            matchIndex = b;
                            break;
                        }
                    }

                    if (matchIndex < 0 && _sourceBindingIndex < targetAction.bindings.Count)
                    {
                        if (!targetAction.bindings[_sourceBindingIndex].isComposite)
                        {
                            matchIndex = _sourceBindingIndex;
                        }
                    }

                    if (matchIndex >= 0 && !targetAction.bindings[matchIndex].isComposite)
                    {
                        if (_isReset)
                        {
                            targetAction.RemoveBindingOverride(matchIndex);
                        }
                        else
                        {
                            targetAction.ApplyBindingOverride(matchIndex, _overridePath);
                        }
                    }
                }
            }

            if (actionReference != null && actionReference.action != null)
            {
                int refIndex = -1;
                for (int b = 0; b < actionReference.action.bindings.Count; ++b)
                {
                    if (actionReference.action.bindings[b].id == targetBindingId)
                    {
                        refIndex = b;
                        break;
                    }
                }

                if (refIndex < 0 && _sourceBindingIndex < actionReference.action.bindings.Count)
                {
                    if (!actionReference.action.bindings[_sourceBindingIndex].isComposite)
                    {
                        refIndex = _sourceBindingIndex;
                    }
                }

                if (refIndex >= 0 && !actionReference.action.bindings[refIndex].isComposite)
                {
                    if (_isReset)
                    {
                        actionReference.action.RemoveBindingOverride(refIndex);
                    }
                    else
                    {
                        actionReference.action.ApplyBindingOverride(refIndex, _overridePath);
                    }
                }
            }
        }

        private bool DetectCrossActionConflict(
            InputAction _sourceAction,
            InputControl _candidateControl,
            string _candidatePath,
            out string _conflictingActionName)
        {
            _conflictingActionName = string.Empty;
            if (_sourceAction.actionMap == null)
            {
                return false;
            }

            foreach (InputAction adjacentAction in _sourceAction.actionMap.actions)
            {
                if (adjacentAction == _sourceAction)
                {
                    continue;
                }

                for (int index = 0; index < adjacentAction.bindings.Count; ++index)
                {
                    InputBinding existingBinding = adjacentAction.bindings[index];

                    if (existingBinding.isComposite || string.IsNullOrEmpty(existingBinding.effectivePath))
                    {
                        continue;
                    }

                    bool isConflict = false;
                    if (_candidateControl != null && DoesControlMatchBindingPath(existingBinding.effectivePath, _candidateControl))
                    {
                        isConflict = true;
                    }
                    else if (!string.IsNullOrEmpty(_candidatePath) && AreControlPathsMatching(existingBinding.effectivePath, _candidatePath))
                    {
                        isConflict = true;
                    }

                    if (isConflict)
                    {
                        _conflictingActionName = adjacentAction.name;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckCrossActionConflictAndSwap(
            InputAction _sourceAction,
            int _sourceBindingIndex,
            InputControl _capturedControl,
            string _originalPathBeforeRebind,
            string _canonicalNewPath,
            out string _conflictSummary)
        {
            _conflictSummary = string.Empty;
            if (_sourceAction.actionMap == null)
            {
                return false;
            }

            StringBuilder stringBuilder = new StringBuilder();
            bool hasSwappedAny = false;

            foreach (InputAction adjacentAction in _sourceAction.actionMap.actions)
            {
                if (adjacentAction == _sourceAction)
                {
                    continue;
                }

                for (int index = 0; index < adjacentAction.bindings.Count; ++index)
                {
                    InputBinding existingBinding = adjacentAction.bindings[index];

                    if (existingBinding.isComposite || string.IsNullOrEmpty(existingBinding.effectivePath))
                    {
                        continue;
                    }

                    bool isConflict = false;
                    if (_capturedControl != null && DoesControlMatchBindingPath(existingBinding.effectivePath, _capturedControl))
                    {
                        isConflict = true;
                    }
                    else if (!string.IsNullOrEmpty(_canonicalNewPath) && AreControlPathsMatching(existingBinding.effectivePath, _canonicalNewPath))
                    {
                        isConflict = true;
                    }

                    if (isConflict)
                    {
                        if (!crossActionSwappedBackups.ContainsKey(adjacentAction.name))
                        {
                            crossActionSwappedBackups[adjacentAction.name] = existingBinding.effectivePath;
                        }

                        ApplyOverrideSafelyToAllInstances(adjacentAction, index, _originalPathBeforeRebind, false);
                        hasSwappedAny = true;

                        string newControlDisplay = InputControlPath.ToHumanReadableString(_canonicalNewPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        string oldControlDisplay = InputControlPath.ToHumanReadableString(_originalPathBeforeRebind, InputControlPath.HumanReadableStringOptions.OmitDevice);

                        stringBuilder.AppendLine($"Swapped [{newControlDisplay}] with '{adjacentAction.name}'. '{adjacentAction.name}' is now bound to [{oldControlDisplay}].");
                    }
                }
            }

            _conflictSummary = stringBuilder.ToString();
            return hasSwappedAny;
        }

        private bool CheckInternalCompositeConflict(
            InputAction _action,
            int _targetPartIndex,
            int _compositeRootIndex,
            InputControl _candidateControl,
            string _candidatePath,
            out string _conflictingPartName)
        {
            _conflictingPartName = string.Empty;

            for (int index = _compositeRootIndex + 1; index < _targetPartIndex; ++index)
            {
                InputBinding existingPartBinding = _action.bindings[index];
                string existingPath = existingPartBinding.effectivePath;

                if (string.IsNullOrEmpty(existingPath))
                {
                    continue;
                }

                bool isConflict = false;
                if (_candidateControl != null && DoesControlMatchBindingPath(existingPath, _candidateControl))
                {
                    isConflict = true;
                }
                else if (!string.IsNullOrEmpty(_candidatePath) && AreControlPathsMatching(existingPath, _candidatePath))
                {
                    isConflict = true;
                }

                if (isConflict)
                {
                    _conflictingPartName = existingPartBinding.name.ToUpper();
                    return true;
                }
            }

            return false;
        }

        private bool DoesControlMatchBindingPath(string _bindingPath, InputControl _control)
        {
            if (string.IsNullOrEmpty(_bindingPath) || _control == null)
            {
                return false;
            }

            InputControl specificControl = ResolveSpecificControl(_control);
            if (specificControl == null)
            {
                return false;
            }

            try
            {
                if (InputControlPath.Matches(_bindingPath, specificControl))
                {
                    return true;
                }

                if (specificControl.name.EndsWith(TRIGGER_BUTTON_SUFFIX, StringComparison.OrdinalIgnoreCase) &&
                    specificControl.parent != null && !(specificControl.parent is InputDevice))
                {
                    if (InputControlPath.Matches(_bindingPath, specificControl.parent))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return AreControlPathsMatching(_bindingPath, specificControl.path);
        }

        private bool AreControlPathsMatching(string _pathA, string _pathB)
        {
            if (string.IsNullOrEmpty(_pathA) || string.IsNullOrEmpty(_pathB))
            {
                return false;
            }

            string cleanA = NormalizeControlPath(_pathA).Replace("<", "").Replace(">", "").Trim().TrimStart('/');
            string cleanB = NormalizeControlPath(_pathB).Replace("<", "").Replace(">", "").Trim().TrimStart('/');

            if (string.Equals(cleanA, cleanB, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!ExtractLayoutAndControl(_pathA, out string layoutA, out string controlA))
            {
                return false;
            }

            if (!ExtractLayoutAndControl(_pathB, out string layoutB, out string controlB))
            {
                return false;
            }

            if (!string.Equals(controlA, controlB, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return AreDeviceLayoutsCompatible(layoutA, layoutB);
        }

        private bool ExtractLayoutAndControl(string _rawPath, out string _layout, out string _control)
        {
            _layout = string.Empty;
            _control = string.Empty;

            if (string.IsNullOrEmpty(_rawPath))
            {
                return false;
            }

            string normalized = NormalizeControlPath(_rawPath).Trim();
            string trimmed = normalized.TrimStart('/');

            if (trimmed.StartsWith("<") && trimmed.Contains(">"))
            {
                int closeIndex = trimmed.IndexOf('>');
                _layout = trimmed.Substring(1, closeIndex - 1);
                _control = trimmed.Substring(closeIndex + 1).TrimStart('/');
            }
            else
            {
                int slashIndex = trimmed.IndexOf('/');
                if (slashIndex > 0)
                {
                    _layout = trimmed.Substring(0, slashIndex);
                    _control = trimmed.Substring(slashIndex + 1);
                }
                else
                {
                    _control = trimmed;
                }
            }

            _control = NormalizeControlPath(_control).TrimStart('/');
            return !string.IsNullOrEmpty(_control);
        }

        private bool AreDeviceLayoutsCompatible(string _layoutA, string _layoutB)
        {
            if (string.IsNullOrEmpty(_layoutA) || string.IsNullOrEmpty(_layoutB))
            {
                return true;
            }

            if (string.Equals(_layoutA, _layoutB, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                if (InputSystem.IsFirstLayoutBasedOnSecond(_layoutA, _layoutB) ||
                    InputSystem.IsFirstLayoutBasedOnSecond(_layoutB, _layoutA))
                {
                    return true;
                }
            }
            catch
            {
            }

            bool isGamepadA = IsGamepadLayout(_layoutA);
            bool isGamepadB = IsGamepadLayout(_layoutB);
            if (isGamepadA && isGamepadB)
            {
                return true;
            }

            bool isKeyboardA = _layoutA.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isKeyboardB = _layoutB.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isKeyboardA && isKeyboardB)
            {
                return true;
            }

            bool isMouseA = _layoutA.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMouseB = _layoutB.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isMouseA && isMouseB)
            {
                return true;
            }

            return false;
        }

        private bool IsGamepadLayout(string _layout)
        {
            if (string.IsNullOrEmpty(_layout))
            {
                return false;
            }

            return _layout.IndexOf("Gamepad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   _layout.IndexOf("Joystick", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   _layout.IndexOf("XInput", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   _layout.IndexOf("DualShock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   _layout.IndexOf("DualSense", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   _layout.IndexOf("SwitchPro", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ConvertToCanonicalPath(InputControl _control)
        {
            if (_control == null)
            {
                return string.Empty;
            }

            InputControl specificControl = ResolveSpecificControl(_control);
            if (specificControl == null)
            {
                return string.Empty;
            }

            if (specificControl.device is Gamepad || specificControl.device is Joystick)
            {
                string controlPart = specificControl.path.Substring(specificControl.device.path.Length).TrimStart('/');
                return $"<Gamepad>/{NormalizeControlPath(controlPart)}";
            }

            if (specificControl.device is Keyboard)
            {
                string controlPart = specificControl.path.Substring(specificControl.device.path.Length).TrimStart('/');
                return $"<Keyboard>/{controlPart}";
            }

            if (specificControl.device is Mouse)
            {
                string controlPart = specificControl.path.Substring(specificControl.device.path.Length).TrimStart('/');
                return $"<Mouse>/{controlPart}";
            }

            return specificControl.path;
        }

        private bool ValidateCompleteCompositeIntegrity(InputAction _action, int _compositeRootIndex, out int _firstConflictingPartIndex)
        {
            _firstConflictingPartIndex = -1;

            for (int i = _compositeRootIndex + 1; i < _action.bindings.Count && _action.bindings[i].isPartOfComposite; ++i)
            {
                string pathA = _action.bindings[i].effectivePath;
                for (int j = i + 1; j < _action.bindings.Count && _action.bindings[j].isPartOfComposite; ++j)
                {
                    string pathB = _action.bindings[j].effectivePath;
                    if (AreControlPathsMatching(pathA, pathB))
                    {
                        _firstConflictingPartIndex = j;
                        return false;
                    }
                }
            }

            return true;
        }

        private void CommitActionMapReboot(InputAction _action)
        {
            InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            for (int i = 0; i < allAssets.Length; ++i)
            {
                InputAction liveAction = allAssets[i].FindAction(_action.id);
                if (liveAction != null && liveAction.actionMap != null)
                {
                    liveAction.actionMap.Disable();
                    liveAction.actionMap.Enable();
                }
            }
        }

        private void SilenceAllUiMapsAcrossEngine()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            silencedUiActionMaps.Clear();

            InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            for (int i = 0; i < allAssets.Length; ++i)
            {
                InputActionMap uiMap = allAssets[i].FindActionMap("UI");
                if (uiMap != null && uiMap.enabled)
                {
                    uiMap.Disable();
                    silencedUiActionMaps.Add(uiMap);
                }
            }
        }

        private void RestoreGlobalInputAndUi()
        {
            for (int i = 0; i < silencedUiActionMaps.Count; ++i)
            {
                if (silencedUiActionMaps[i] != null && !silencedUiActionMaps[i].enabled)
                {
                    silencedUiActionMaps[i].Enable();
                }
            }

            silencedUiActionMaps.Clear();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private string NormalizeControlPath(string _rawPath)
        {
            if (string.IsNullOrEmpty(_rawPath))
            {
                return string.Empty;
            }

            string path = _rawPath;
            if (path.EndsWith("/leftTriggerButton", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - TRIGGER_BUTTON_SUFFIX.Length);
            }
            else if (path.EndsWith("/rightTriggerButton", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - TRIGGER_BUTTON_SUFFIX.Length);
            }

            return path;
        }

        private IEnumerator FinalizeRebindRoutine(InputAction _action, InputControl _justPressedControl, bool _broadcastChange)
        {
            if (_justPressedControl != null)
            {
                float finalizeTimeout = Time.unscaledTime + 2.0f;
                while (IsControlActuated(_justPressedControl) && Time.unscaledTime < finalizeTimeout)
                {
                    yield return null;
                }
            }

            rebindCooldownEndTime = Time.unscaledTime + REBIND_COOLDOWN_DURATION;

            preRebindBackupPaths.Clear();
            crossActionSwappedBackups.Clear();
            hasActiveSwapNotification = false;
            activeSwapHeader = string.Empty;
            activeSwapDetails = string.Empty;

            RestoreGlobalInputAndUi();
            RestoreDeviceTrackingAndCloseOverlay();
            ResolveAndRefreshForActiveDevice();

            if (_broadcastChange)
            {
                OnAnyBindingChanged?.Invoke();
            }
        }

        private string BuildCompositeProgressString(
            InputAction _action,
            int _compositeRootIndex,
            int _currentPartIndex,
            string _activePartCandidatePath)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int partIndex = _compositeRootIndex + 1; partIndex < _action.bindings.Count && _action.bindings[partIndex].isPartOfComposite; ++partIndex)
            {
                InputBinding partBinding = _action.bindings[partIndex];
                string partName = partBinding.name.ToUpper();

                if (partIndex < _currentPartIndex)
                {
                    string boundKeyDisplay = _action.GetBindingDisplayString(partIndex, displayStringOptions);
                    stringBuilder.Append($"<b>{partName}:</b> <color=#00FF88>[{boundKeyDisplay}]</color>  ");
                }
                else if (partIndex == _currentPartIndex)
                {
                    if (!string.IsNullOrEmpty(_activePartCandidatePath))
                    {
                        string previewName = InputControlPath.ToHumanReadableString(_activePartCandidatePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        stringBuilder.Append($"<b>{partName}:</b> <color=#00FF88>[{previewName}]</color>  ");
                    }
                    else
                    {
                        stringBuilder.Append($"<b>{partName}:</b> <color=#FFCC00>[Waiting...]</color>  ");
                    }
                }
                else
                {
                    stringBuilder.Append($"<b>{partName}:</b> <color=#777777>[-]</color>  ");
                }
            }

            return stringBuilder.ToString();
        }

        private string BuildStatusPrompt(
            InputAction _action,
            int _bindingIndex,
            bool _allCompositeParts,
            int _compositeRootIndex)
        {
            if (!_allCompositeParts)
            {
                return "Waiting for input...";
            }

            int currentStep = _bindingIndex - _compositeRootIndex;
            int totalParts = 0;
            for (int index = _compositeRootIndex + 1; index < _action.bindings.Count && _action.bindings[index].isPartOfComposite; ++index)
            {
                totalParts++;
            }

            InputBinding binding = _action.bindings[_bindingIndex];
            return $"Rebinding: {binding.name.ToUpper()} (Step {currentStep} of {totalParts})";
        }

        private void ResolveResetConflicts(InputAction _targetAction, int _bindingIndexToReset)
        {
            if (_targetAction.actionMap == null)
            {
                return;
            }

            InputBinding bindingToReset = _targetAction.bindings[_bindingIndexToReset];
            string defaultPath = bindingToReset.path;
            string currentPath = bindingToReset.effectivePath;

            if (string.IsNullOrEmpty(defaultPath))
            {
                return;
            }

            foreach (InputAction adjacentAction in _targetAction.actionMap.actions)
            {
                if (adjacentAction == _targetAction)
                {
                    continue;
                }

                for (int index = 0; index < adjacentAction.bindings.Count; ++index)
                {
                    InputBinding existingBinding = adjacentAction.bindings[index];

                    if (existingBinding.isComposite)
                    {
                        continue;
                    }

                    if (AreControlPathsMatching(existingBinding.overridePath, defaultPath))
                    {
                        ApplyOverrideSafelyToAllInstances(adjacentAction, index, currentPath, false);
                    }
                }
            }
        }

        private int FindMatchingBindingIndex(InputAction _action, bool _isGamepad)
        {
            for (int index = 0; index < _action.bindings.Count; ++index)
            {
                InputBinding binding = _action.bindings[index];

                if (binding.isComposite)
                {
                    bool compositeMatchesDevice = false;
                    for (int partIndex = index + 1; partIndex < _action.bindings.Count && _action.bindings[partIndex].isPartOfComposite; ++partIndex)
                    {
                        if (IsBindingMatchingDevice(_action.bindings[partIndex], _isGamepad))
                        {
                            compositeMatchesDevice = true;
                            break;
                        }
                    }

                    if (compositeMatchesDevice)
                    {
                        return index;
                    }
                }
                else if (!binding.isPartOfComposite)
                {
                    if (IsBindingMatchingDevice(binding, _isGamepad))
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        private bool IsBindingMatchingDevice(InputBinding _binding, bool _isGamepad)
        {
            string path = _binding.effectivePath;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (_isGamepad)
            {
                if (IsGamepadLayout(path))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(_binding.groups) && _binding.groups.IndexOf("Gamepad", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                return false;
            }

            if (path.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(_binding.groups) &&
                (_binding.groups.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 _binding.groups.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            return false;
        }

        private string BuildActionDisplayName(InputAction _action, int _bindingIndex)
        {
            if (_bindingIndex >= 0 && _bindingIndex < _action.bindings.Count)
            {
                InputBinding binding = _action.bindings[_bindingIndex];
                if (binding.isPartOfComposite)
                {
                    return $"{_action.name} - [{binding.name.ToUpper()}]";
                }
            }

            return _action.name;
        }

        private string ResolveDeviceFamilyFriendlyName(DeviceFamily _family)
        {
            switch (_family)
            {
                case DeviceFamily.KeyboardMouse:
                    return "Keyboard & Mouse";
                case DeviceFamily.Xbox:
                    return "Xbox Controller";
                case DeviceFamily.PlayStation:
                    return "PlayStation Controller";
                case DeviceFamily.Nintendo:
                    return "Nintendo Switch Controller";
                case DeviceFamily.SteamDevice:
                    return "Steam Deck";
                case DeviceFamily.GenericGamepad:
                    return "Generic Gamepad";
                default:
                    return "Controller";
            }
        }

        private InputActionAsset GetRuntimeAsset()
        {
            if (RuntimeInputAssetProvider != null)
            {
                InputActionAsset providedAsset = RuntimeInputAssetProvider.Invoke();
                if (providedAsset != null)
                {
                    return providedAsset;
                }
            }

            if (cachedDiscoveredRuntimeAsset != null)
            {
                return cachedDiscoveredRuntimeAsset;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Type[] types = null;
                try
                {
                    types = assemblies[i].GetTypes();
                }
                catch
                {
                    continue;
                }

                for (int t = 0; t < types.Length; ++t)
                {
                    if (string.Equals(types[t].Name, "GlobalInput", StringComparison.OrdinalIgnoreCase))
                    {
                        PropertyInfo property = types[t].GetProperty("PlayerInputActions", BindingFlags.Public | BindingFlags.Static);
                        if (property != null)
                        {
                            object actionsInstance = property.GetValue(null);
                            if (actionsInstance != null)
                            {
                                PropertyInfo assetProperty = actionsInstance.GetType().GetProperty("asset", BindingFlags.Public | BindingFlags.Instance);
                                if (assetProperty != null)
                                {
                                    cachedDiscoveredRuntimeAsset = assetProperty.GetValue(actionsInstance) as InputActionAsset;
                                    if (cachedDiscoveredRuntimeAsset != null)
                                    {
                                        return cachedDiscoveredRuntimeAsset;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            for (int i = 0; i < allAssets.Length; ++i)
            {
                if (allAssets[i].FindActionMap("Player") != null && allAssets[i].FindActionMap("UI") != null)
                {
                    return allAssets[i];
                }
            }

            return null;
        }

        private InputAction ResolveRuntimeAction()
        {
            if (actionReference == null || actionReference.action == null)
            {
                return null;
            }

            InputActionAsset runtimeAsset = GetRuntimeAsset();
            if (runtimeAsset != null)
            {
                InputAction matchedAction = runtimeAsset.FindAction(actionReference.action.id);
                if (matchedAction != null)
                {
                    return matchedAction;
                }
            }

            return actionReference.action;
        }

        private void PublishDisplayPayload(InputAction _action, int _bindingIndex)
        {
            string displayString = string.Empty;
            string deviceLayoutName = string.Empty;
            string controlPath = string.Empty;

            if (_bindingIndex >= 0 && _bindingIndex < _action.bindings.Count)
            {
                InputBinding binding = _action.bindings[_bindingIndex];
                if (binding.isComposite)
                {
                    StringBuilder compositeDisplayBuilder = new StringBuilder();
                    bool firstPart = true;
                    for (int partIndex = _bindingIndex + 1; partIndex < _action.bindings.Count && _action.bindings[partIndex].isPartOfComposite; ++partIndex)
                    {
                        string partString = _action.GetBindingDisplayString(partIndex, displayStringOptions);
                        if (!firstPart)
                        {
                            compositeDisplayBuilder.Append(" / ");
                        }
                        compositeDisplayBuilder.Append(partString);
                        firstPart = false;
                    }
                    displayString = compositeDisplayBuilder.ToString();
                }
                else
                {
                    displayString = _action.GetBindingDisplayString(
                        _bindingIndex,
                        out deviceLayoutName,
                        out controlPath,
                        displayStringOptions);
                }
            }

            BindingDisplayPayload payload = new BindingDisplayPayload(
                _action.name,
                displayString,
                deviceLayoutName,
                controlPath,
                InputDeviceTracker.CurrentDeviceFamily);

            OnDisplayUpdated?.Invoke(payload);
        }

        private void PublishOverlayPayload(
            bool _isOpen,
            string _deviceName,
            string _actionName,
            string _prompt,
            string _currentInput,
            bool _hasConflict,
            string _conflictHeader,
            string _conflictDetails)
        {
            RebindOverlayPayload payload = new RebindOverlayPayload(
                _isOpen,
                _deviceName,
                _actionName,
                _prompt,
                _currentInput,
                _hasConflict,
                _conflictHeader,
                _conflictDetails);

            OnOverlayUpdated?.Invoke(payload);
        }

        private void RestoreDeviceTrackingAndCloseOverlay()
        {
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;

            PublishOverlayPayload(
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                string.Empty,
                string.Empty);

            CleanUpRebindOperation();
        }

        private void StopActiveRebindRoutine()
        {
            if (activeRebindRoutine != null)
            {
                StopCoroutine(activeRebindRoutine);
                activeRebindRoutine = null;
            }
        }

        private void CleanUpRebindOperation()
        {
            if (ongoingRebindOperation != null)
            {
                ongoingRebindOperation.Dispose();
                ongoingRebindOperation = null;
            }
        }

        private void HandleDeviceFamilyChanged(DeviceFamily _newDeviceFamily)
        {
            ResolveAndRefreshForActiveDevice();
        }
    }
}