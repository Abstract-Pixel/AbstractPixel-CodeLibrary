using System.Collections;
using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    // Updates before the EventSystem (order 0), so navigation rebuilt and selection made this frame
    // are already in place when the EventSystem processes this frame's D-pad press.
    [DefaultExecutionOrder(-1100)]
    [DisallowMultipleComponent]
    public class AutoSpatialNavigation : MonoBehaviour
    {
        [Header("Auto Selection")]
        [Tooltip("If true, selects the first button on enable while a controller is the last used device. " +
                 "The selection waits (with no time limit) until that button is active and interactable, " +
                 "so entrance animations that disable buttons can take as long as they need.")]
        [SerializeField] private bool autoSelectOnEnable = true;

        [Tooltip("Optional explicit button to highlight first when using a controller. If unassigned, selects the top-leftmost button.")]
        [SerializeField] private Selectable firstSelectedOverride;

        [Header("Navigation Rules")]
        [Tooltip("Both: free 2D search. VerticalList: Up/Down only, ordered top to bottom. HorizontalList: Left/Right only, ordered left to right.")]
        [SerializeField] private SpatialNavigationAxis navigationAxis = SpatialNavigationAxis.Both;

        [Tooltip("If true, pressing Right on the rightmost element loops to the left, and pressing Down on the bottom element loops to the top.")]
        [SerializeField] private bool loopNavigation = true;

        [Tooltip("Maximum angle cone in degrees (from 45 to 85) to search for adjacent buttons.")]
        [Range(45.0f, 85.0f)]
        [SerializeField] private float directionalConeAngle = 75.0f;

        [Tooltip("Polling frequency in seconds to detect UI elements being enabled, disabled, or destroyed dynamically.")]
        [SerializeField] private float evaluationRate = 0.25f;

        private struct UIElementInfo
        {
            public Selectable selectable;
            public Rect screenRect;
            public Vector2 center => screenRect.center;
        }

        private List<Selectable> trackedSelectables = new List<Selectable>();
        private List<bool> trackedInteractableStates = new List<bool>();
        private List<bool> trackedActiveStates = new List<bool>();
        private List<Vector2> trackedPositions = new List<Vector2>();
        private int cachedSelectableCount = -1;

        private Canvas cachedCanvas;
        private Camera targetCamera;
        private Coroutine evaluationCoroutine;
        private Coroutine deferredInitCoroutine;

        // True from enable until the first button has been selected for a controller.
        private bool isInitialSelectionPending = false;

        private static readonly Vector2 NO_POINTER_POSITION = new Vector2(-1f, -1f);
        private Vector2 lastPointerScreenPosition = NO_POINTER_POSITION;

        private const float ALIGNMENT_EPSILON = 1.0f;
        private const float PERPENDICULAR_WEIGHT = 2.5f;
        private const float STICK_NAV_THRESHOLD_SQR = 0.16f;
        private const float MOUSE_MOVE_THRESHOLD_SQR = 4.0f;

        private void Awake()
        {
            ResolveCanvasAndCamera();
        }

        private void OnEnable()
        {
            ResolveCanvasAndCamera();
            ForceRebuildAllLayouts();
            InitializeStateTracking();
            BuildNavigation();

            InputDeviceTracker.OnCurrentInputDeviceChanged -= HandleDeviceChanged;
            InputDeviceTracker.OnCurrentInputDeviceChanged += HandleDeviceChanged;

            // A pointer position from a previous time this menu was open must not pick the first button.
            lastPointerScreenPosition = NO_POINTER_POSITION;

            // Whatever is selected now belongs to an earlier screen, or to this menu's last visit (and is hidden).
            // Leaving it would block the first selection and send the first D-pad presses to a hidden button.
            ClearSelection();

            isInitialSelectionPending = false;
            if (autoSelectOnEnable)
            {
                TriggerAutoSelect();
            }

            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
            }
            evaluationCoroutine = StartCoroutine(StateEvaluationRoutine());

            if (deferredInitCoroutine != null)
            {
                StopCoroutine(deferredInitCoroutine);
            }
            deferredInitCoroutine = StartCoroutine(DeferredInitialBuildRoutine());
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnCurrentInputDeviceChanged -= HandleDeviceChanged;

            isInitialSelectionPending = false;
            ClearSelectionIfInsideThisMenu();

            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
                evaluationCoroutine = null;
            }

            if (deferredInitCoroutine != null)
            {
                StopCoroutine(deferredInitCoroutine);
                deferredInitCoroutine = null;
            }
        }

        private void Update()
        {
            RebuildNavigationIfInteractabilityChanged();
            DetectMouseMovementAndDeselect();
            DetectControllerInputAndAdaptSelection();
            TryCompleteInitialSelection();
        }

        // Entrance animations switch buttons interactable one by one. Rebuilding as soon as that happens
        // (instead of on the next poll) means the D-pad reaches each button the frame it becomes usable.
        // Reads cached state only, so it does not allocate.
        private void RebuildNavigationIfInteractabilityChanged()
        {
            bool statesChanged = false;

            for (int index = 0; index < trackedSelectables.Count; ++index)
            {
                Selectable selectable = trackedSelectables[index];
                if (selectable == null)
                {
                    continue;
                }

                bool currentInteractable = selectable.IsInteractable();
                bool currentActive = selectable.gameObject.activeInHierarchy;

                if (currentInteractable != trackedInteractableStates[index] || currentActive != trackedActiveStates[index])
                {
                    trackedInteractableStates[index] = currentInteractable;
                    trackedActiveStates[index] = currentActive;
                    statesChanged = true;
                }
            }

            if (statesChanged)
            {
                BuildNavigation();
            }
        }

        // Keeps the first button selected until the player navigates with the controller. It has no time
        // limit: an entrance animation can keep the buttons non-interactable for as long as it needs.
        // It also reselects when Unity drops the selection, which happens whenever an animation sets the
        // selected button's interactable to false (for example when its OnEnable runs after this one).
        private void TryCompleteInitialSelection()
        {
            if (!isInitialSelectionPending)
            {
                return;
            }

            bool isControllerInUse = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            if (!isControllerInUse)
            {
                // Stay pending: if the player picks up the controller during the entrance, the first button is still chosen.
                return;
            }

            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            if (HasUsableSelection(currentEventSystem))
            {
                return;
            }

            Selectable initialTarget = FindInitialSelectionTarget();
            if (initialTarget == null)
            {
                return;
            }

            SelectNow(initialTarget);
        }

        // The player pressed a controller button while something in this menu is selected: from now on
        // the selection is theirs, so the initial selection stops reselecting the first button.
        private void EndInitialSelectionIfPlayerTookOver()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            GameObject currentSelection = currentEventSystem.currentSelectedGameObject;
            if (currentSelection == null)
            {
                return;
            }

            bool isSelectionInsideThisMenu = currentSelection.transform.IsChildOf(transform);
            if (isSelectionInsideThisMenu)
            {
                isInitialSelectionPending = false;
            }
        }

        // The configured first button when it is usable, otherwise the top-left usable button
        // (only when no first button is configured). Returns null when nothing is usable yet.
        private Selectable FindInitialSelectionTarget()
        {
            if (firstSelectedOverride != null)
            {
                bool isOverrideUsable = firstSelectedOverride.gameObject.activeInHierarchy && firstSelectedOverride.IsInteractable();
                if (isOverrideUsable)
                {
                    return firstSelectedOverride;
                }

                return null;
            }

            List<Selectable> validSelectables = GetValidSelectables();
            if (validSelectables.Count == 0)
            {
                return null;
            }

            SortTopLeftFirst(validSelectables);
            return validSelectables[0];
        }

        private void SelectNow(Selectable _target)
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null || _target == null)
            {
                return;
            }

            // Make sure the explicit navigation already includes the target before the next D-pad press.
            BuildNavigation();

            currentEventSystem.SetSelectedGameObject(null);
            currentEventSystem.SetSelectedGameObject(_target.gameObject);
        }

        private void ClearSelection()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            if (currentEventSystem.currentSelectedGameObject != null)
            {
                currentEventSystem.SetSelectedGameObject(null);
            }
        }

        // Called when the menu closes: the EventSystem would otherwise keep pointing at a hidden button.
        private void ClearSelectionIfInsideThisMenu()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            GameObject currentSelection = currentEventSystem.currentSelectedGameObject;
            if (currentSelection == null)
            {
                return;
            }

            bool isSelectionInsideThisMenu = currentSelection.transform.IsChildOf(transform);
            if (isSelectionInsideThisMenu)
            {
                currentEventSystem.SetSelectedGameObject(null);
            }
        }

        // A selection counts only while its object is visible and usable. A hidden or disabled one
        // receives the D-pad presses but nothing visible happens.
        private bool HasUsableSelection(EventSystem _eventSystem)
        {
            GameObject currentSelection = _eventSystem.currentSelectedGameObject;
            if (currentSelection == null)
            {
                return false;
            }

            if (!currentSelection.activeInHierarchy)
            {
                return false;
            }

            Selectable selectedSelectable = currentSelection.GetComponent<Selectable>();
            if (selectedSelectable != null && !selectedSelectable.IsInteractable())
            {
                return false;
            }

            return true;
        }

        private void DetectMouseMovementAndDeselect()
        {
            Mouse currentMouse = Mouse.current;
            if (currentMouse == null)
            {
                return;
            }

            Vector2 mouseDelta = currentMouse.delta.ReadValue();
            if (mouseDelta.sqrMagnitude > MOUSE_MOVE_THRESHOLD_SQR)
            {
                lastPointerScreenPosition = currentMouse.position.ReadValue();

                if (InputDeviceTracker.LastUsedDevice != currentMouse)
                {
                    InputDeviceTracker.SetActiveDevice(currentMouse);
                }

                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null && currentEventSystem.currentSelectedGameObject != null)
                {
                    currentEventSystem.SetSelectedGameObject(null);
                }
            }
        }

        private void DetectControllerInputAndAdaptSelection()
        {
            Gamepad navigatingGamepad = FindGamepadThatNavigatedThisFrame();
            if (navigatingGamepad == null)
            {
                return;
            }

            if (InputDeviceTracker.LastUsedDevice != navigatingGamepad)
            {
                InputDeviceTracker.SetActiveDevice(navigatingGamepad);
            }

            if (isInitialSelectionPending)
            {
                // The first button is selected by TryCompleteInitialSelection as soon as it is usable.
                EndInitialSelectionIfPlayerTookOver();
                return;
            }

            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem != null && !HasUsableSelection(currentEventSystem))
            {
                SelectClosestButtonToLastPointerPosition();
            }
        }

        // Checks every connected gamepad, not only Gamepad.current: with two pads connected (or a Steam Deck
        // plus a controller) Gamepad.current can be the idle one and the presses would go unnoticed.
        private Gamepad FindGamepadThatNavigatedThisFrame()
        {
            ReadOnlyArray<Gamepad> connectedGamepads = Gamepad.all;

            for (int index = 0; index < connectedGamepads.Count; ++index)
            {
                Gamepad gamepad = connectedGamepads[index];
                if (WasNavigationTriggered(gamepad))
                {
                    return gamepad;
                }
            }

            return null;
        }

        private bool WasNavigationTriggered(Gamepad _gamepad)
        {
            if (_gamepad.dpad.up.wasPressedThisFrame ||
                _gamepad.dpad.down.wasPressedThisFrame ||
                _gamepad.dpad.left.wasPressedThisFrame ||
                _gamepad.dpad.right.wasPressedThisFrame ||
                _gamepad.buttonSouth.wasPressedThisFrame ||
                _gamepad.buttonEast.wasPressedThisFrame)
            {
                return true;
            }

            Vector2 leftStick = _gamepad.leftStick.ReadValue();
            if (leftStick.sqrMagnitude >= STICK_NAV_THRESHOLD_SQR)
            {
                return true;
            }

            return false;
        }

        public void SelectClosestButtonToLastPointerPosition()
        {
            List<Selectable> validSelectables = GetValidSelectables();
            if (validSelectables.Count == 0)
            {
                return;
            }

            Selectable targetToSelect = null;

            if (lastPointerScreenPosition.x >= 0f && lastPointerScreenPosition.y >= 0f)
            {
                for (int index = 0; index < validSelectables.Count; ++index)
                {
                    RectTransform rectTransform = validSelectables[index].transform as RectTransform;
                    if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, lastPointerScreenPosition, targetCamera))
                    {
                        targetToSelect = validSelectables[index];
                        break;
                    }
                }

                if (targetToSelect == null)
                {
                    float shortestDistanceSqr = float.MaxValue;
                    for (int index = 0; index < validSelectables.Count; ++index)
                    {
                        Vector2 buttonCenter = GetScreenCenter(validSelectables[index].transform as RectTransform);
                        float distanceSqr = (buttonCenter - lastPointerScreenPosition).sqrMagnitude;
                        if (distanceSqr < shortestDistanceSqr)
                        {
                            shortestDistanceSqr = distanceSqr;
                            targetToSelect = validSelectables[index];
                        }
                    }
                }
            }

            if (targetToSelect == null)
            {
                if (firstSelectedOverride != null && firstSelectedOverride.gameObject.activeInHierarchy && firstSelectedOverride.IsInteractable())
                {
                    targetToSelect = firstSelectedOverride;
                }
                else
                {
                    SortTopLeftFirst(validSelectables);
                    targetToSelect = validSelectables[0];
                }
            }

            SelectNow(targetToSelect);
        }

        private void SortTopLeftFirst(List<Selectable> _selectables)
        {
            _selectables.Sort((_first, _second) =>
            {
                Vector2 firstCenter = GetScreenCenter(_first.transform as RectTransform);
                Vector2 secondCenter = GetScreenCenter(_second.transform as RectTransform);

                bool isSameRow = Mathf.Abs(firstCenter.y - secondCenter.y) <= 20.0f;
                if (isSameRow)
                {
                    return firstCenter.x.CompareTo(secondCenter.x);
                }

                return secondCenter.y.CompareTo(firstCenter.y);
            });
        }

        private IEnumerator DeferredInitialBuildRoutine()
        {
            yield return null;

            ForceRebuildAllLayouts();
            InitializeStateTracking();
            BuildNavigation();

            deferredInitCoroutine = null;
        }

        private void ResolveCanvasAndCamera()
        {
            if (cachedCanvas == null)
            {
                cachedCanvas = GetComponentInParent<Canvas>();
            }

            if (cachedCanvas != null && cachedCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetCamera = cachedCanvas.worldCamera != null ? cachedCanvas.worldCamera : Camera.main;
            }
            else
            {
                targetCamera = null;
            }
        }

        private void ForceRebuildAllLayouts()
        {
            LayoutGroup[] childLayoutGroups = GetComponentsInChildren<LayoutGroup>(false);
            for (int layoutIndex = 0; layoutIndex < childLayoutGroups.Length; ++layoutIndex)
            {
                RectTransform layoutRect = childLayoutGroups[layoutIndex].transform as RectTransform;
                if (layoutRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
                }
            }

            Canvas.ForceUpdateCanvases();
        }

        private void HandleDeviceChanged(InputDevice _device)
        {
            if (_device is Gamepad || _device is Joystick)
            {
                if (isInitialSelectionPending)
                {
                    // TryCompleteInitialSelection picks the configured first button this frame or once it is usable.
                    return;
                }

                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null && !HasUsableSelection(currentEventSystem))
                {
                    SelectClosestButtonToLastPointerPosition();
                }
            }
            else if (_device is Mouse)
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null && currentEventSystem.currentSelectedGameObject != null)
                {
                    currentEventSystem.SetSelectedGameObject(null);
                }
            }
        }

        // Selects the first button for a controller as soon as it is usable (see TryCompleteInitialSelection).
        public void TriggerAutoSelect()
        {
            isInitialSelectionPending = true;
            TryCompleteInitialSelection();
        }

        private void InitializeStateTracking()
        {
            Selectable[] allChildSelectables = GetComponentsInChildren<Selectable>(true);
            cachedSelectableCount = allChildSelectables.Length;

            trackedSelectables.Clear();
            trackedInteractableStates.Clear();
            trackedActiveStates.Clear();
            trackedPositions.Clear();

            for (int index = 0; index < allChildSelectables.Length; ++index)
            {
                Selectable selectable = allChildSelectables[index];
                if (selectable != null)
                {
                    trackedSelectables.Add(selectable);
                    trackedInteractableStates.Add(selectable.IsInteractable());
                    trackedActiveStates.Add(selectable.gameObject.activeInHierarchy);
                    trackedPositions.Add(GetScreenCenter(selectable.transform as RectTransform));
                }
            }
        }

        private IEnumerator StateEvaluationRoutine()
        {
            WaitForSecondsRealtime waitInstruction = new WaitForSecondsRealtime(evaluationRate);

            while (true)
            {
                yield return waitInstruction;

                Selectable[] currentChildSelectables = GetComponentsInChildren<Selectable>(true);
                bool structureChanged = currentChildSelectables.Length != cachedSelectableCount;

                if (structureChanged)
                {
                    ForceRebuildAllLayouts();
                    InitializeStateTracking();
                    BuildNavigation();
                    continue;
                }

                bool statesChanged = false;
                for (int index = 0; index < trackedSelectables.Count; ++index)
                {
                    Selectable selectable = trackedSelectables[index];
                    if (selectable == null)
                    {
                        statesChanged = true;
                        break;
                    }

                    bool currentInteractable = selectable.IsInteractable();
                    bool currentActive = selectable.gameObject.activeInHierarchy;
                    Vector2 currentPos = GetScreenCenter(selectable.transform as RectTransform);

                    if (currentInteractable != trackedInteractableStates[index] ||
                        currentActive != trackedActiveStates[index] ||
                        Vector2.Distance(currentPos, trackedPositions[index]) > ALIGNMENT_EPSILON)
                    {
                        trackedInteractableStates[index] = currentInteractable;
                        trackedActiveStates[index] = currentActive;
                        trackedPositions[index] = currentPos;
                        statesChanged = true;
                    }
                }

                if (statesChanged)
                {
                    ForceRebuildAllLayouts();
                    BuildNavigation();
                }
            }
        }

        public void BuildNavigation()
        {
            ResolveCanvasAndCamera();

            List<Selectable> validSelectables = GetValidSelectables();
            int count = validSelectables.Count;

            if (count == 1)
            {
                // Clear navigation left from a previous build, which may point at buttons that are not usable yet.
                ClearExplicitNavigation(validSelectables[0]);
                return;
            }

            if (count == 0)
            {
                return;
            }

            if (navigationAxis == SpatialNavigationAxis.VerticalList)
            {
                BuildListNavigation(validSelectables, true);
                return;
            }

            if (navigationAxis == SpatialNavigationAxis.HorizontalList)
            {
                BuildListNavigation(validSelectables, false);
                return;
            }

            List<UIElementInfo> elementInfos = new List<UIElementInfo>(count);
            Vector3[] corners = new Vector3[4];

            for (int index = 0; index < count; ++index)
            {
                Selectable currentSelectable = validSelectables[index];
                RectTransform rectTransform = currentSelectable.transform as RectTransform;
                if (rectTransform == null) continue;

                rectTransform.GetWorldCorners(corners);
                Vector2 p0 = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[0]);
                Vector2 p2 = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[2]);

                float xMin = Mathf.Min(p0.x, p2.x);
                float xMax = Mathf.Max(p0.x, p2.x);
                float yMin = Mathf.Min(p0.y, p2.y);
                float yMax = Mathf.Max(p0.y, p2.y);

                elementInfos.Add(new UIElementInfo
                {
                    selectable = currentSelectable,
                    screenRect = new Rect(xMin, yMin, Mathf.Max(1.0f, xMax - xMin), Mathf.Max(1.0f, yMax - yMin))
                });
            }

            float maxConeTangent = Mathf.Tan(directionalConeAngle * Mathf.Deg2Rad);

            for (int index = 0; index < elementInfos.Count; ++index)
            {
                UIElementInfo current = elementInfos[index];

                Navigation customNavigation = new Navigation();
                customNavigation.mode = Navigation.Mode.Explicit;

                customNavigation.selectOnRight = FindBestCandidate(current, Vector2.right, elementInfos, maxConeTangent);
                customNavigation.selectOnLeft = FindBestCandidate(current, Vector2.left, elementInfos, maxConeTangent);
                customNavigation.selectOnUp = FindBestCandidate(current, Vector2.up, elementInfos, maxConeTangent);
                customNavigation.selectOnDown = FindBestCandidate(current, Vector2.down, elementInfos, maxConeTangent);
                KeepSideAxisForSliderValue(current.selectable, ref customNavigation);

                current.selectable.navigation = customNavigation;
            }
        }

        // A Unity slider only changes its value on Left/Right when it has no neighbour on that side.
        // A spatial search can find a button diagonally beside it, and then the D-pad moves the focus
        // instead of the value. So a horizontal slider keeps Left/Right for itself (Up/Down still move on).
        private static void KeepSideAxisForSliderValue(Selectable _selectable, ref Navigation _navigation)
        {
            Slider slider = _selectable as Slider;
            if (slider == null)
            {
                return;
            }

            bool isHorizontal = slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft;
            if (isHorizontal == false)
            {
                return;
            }

            _navigation.selectOnLeft = null;
            _navigation.selectOnRight = null;
        }

        // Links the buttons as one ordered list on a single axis. Staggered or diagonal layouts still step
        // one button at a time, and the other axis stays unlinked.
        private void BuildListNavigation(List<Selectable> _selectables, bool _isVertical)
        {
            if (_isVertical)
            {
                // Top of the screen first.
                _selectables.Sort((_first, _second) =>
                {
                    float firstY = GetScreenCenter(_first.transform as RectTransform).y;
                    float secondY = GetScreenCenter(_second.transform as RectTransform).y;
                    return secondY.CompareTo(firstY);
                });
            }
            else
            {
                // Left of the screen first.
                _selectables.Sort((_first, _second) =>
                {
                    float firstX = GetScreenCenter(_first.transform as RectTransform).x;
                    float secondX = GetScreenCenter(_second.transform as RectTransform).x;
                    return firstX.CompareTo(secondX);
                });
            }

            int count = _selectables.Count;
            for (int index = 0; index < count; ++index)
            {
                Selectable previous = GetListNeighbour(_selectables, index - 1);
                Selectable next = GetListNeighbour(_selectables, index + 1);

                Navigation listNavigation = new Navigation();
                listNavigation.mode = Navigation.Mode.Explicit;

                if (_isVertical)
                {
                    listNavigation.selectOnUp = previous;
                    listNavigation.selectOnDown = next;
                }
                else
                {
                    listNavigation.selectOnLeft = previous;
                    listNavigation.selectOnRight = next;
                }

                _selectables[index].navigation = listNavigation;
            }
        }

        // The list entry at _index, wrapping around the ends when loopNavigation is on.
        private Selectable GetListNeighbour(List<Selectable> _selectables, int _index)
        {
            int count = _selectables.Count;

            if (_index >= 0 && _index < count)
            {
                return _selectables[_index];
            }

            if (!loopNavigation)
            {
                return null;
            }

            int wrappedIndex = (_index + count) % count;
            return _selectables[wrappedIndex];
        }

        private void ClearExplicitNavigation(Selectable _selectable)
        {
            Navigation emptyNavigation = new Navigation();
            emptyNavigation.mode = Navigation.Mode.Explicit;
            _selectable.navigation = emptyNavigation;
        }

        private Selectable FindBestCandidate(
            UIElementInfo _source,
            Vector2 _direction,
            List<UIElementInfo> _candidates,
            float _maxConeTangent)
        {
            bool isHorizontal = Mathf.Abs(_direction.x) > 0.5f;

            if (isHorizontal)
            {
                List<UIElementInfo> sameRowElements = new List<UIElementInfo>();
                for (int index = 0; index < _candidates.Count; ++index)
                {
                    if (_candidates[index].selectable == _source.selectable) continue;

                    if (IsSameRow(_source.screenRect, _candidates[index].screenRect))
                    {
                        sameRowElements.Add(_candidates[index]);
                    }
                }

                if (sameRowElements.Count > 0)
                {
                    UIElementInfo bestRowCandidate = default;
                    float bestDistance = float.MaxValue;
                    bool foundInDirection = false;

                    for (int index = 0; index < sameRowElements.Count; ++index)
                    {
                        UIElementInfo candidateInfo = sameRowElements[index];
                        float deltaX = candidateInfo.center.x - _source.center.x;

                        if (_direction.x > 0 && deltaX > ALIGNMENT_EPSILON)
                        {
                            if (deltaX < bestDistance)
                            {
                                bestDistance = deltaX;
                                bestRowCandidate = candidateInfo;
                                foundInDirection = true;
                            }
                        }
                        else if (_direction.x < 0 && deltaX < -ALIGNMENT_EPSILON)
                        {
                            float distance = -deltaX;
                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                bestRowCandidate = candidateInfo;
                                foundInDirection = true;
                            }
                        }
                    }

                    if (foundInDirection)
                    {
                        return bestRowCandidate.selectable;
                    }

                    if (loopNavigation)
                    {
                        UIElementInfo wrapCandidate = default;
                        float extremeX = _direction.x > 0 ? float.MaxValue : float.MinValue;

                        for (int index = 0; index < sameRowElements.Count; ++index)
                        {
                            UIElementInfo candidateInfo = sameRowElements[index];
                            if (_direction.x > 0)
                            {
                                if (candidateInfo.center.x < extremeX)
                                {
                                    extremeX = candidateInfo.center.x;
                                    wrapCandidate = candidateInfo;
                                }
                            }
                            else
                            {
                                if (candidateInfo.center.x > extremeX)
                                {
                                    extremeX = candidateInfo.center.x;
                                    wrapCandidate = candidateInfo;
                                }
                            }
                        }

                        if (wrapCandidate.selectable != null)
                        {
                            return wrapCandidate.selectable;
                        }
                    }

                    return null;
                }
            }

            Selectable bestCandidate = null;
            float bestScore = float.MaxValue;

            for (int index = 0; index < _candidates.Count; ++index)
            {
                UIElementInfo candidate = _candidates[index];
                if (candidate.selectable == _source.selectable)
                {
                    continue;
                }

                // Left / Right only reach elements really beside the source. One that overlaps it horizontally
                // sits above or below (a stacked settings row), and moving there on Left/Right would leave the
                // row the player is changing.
                if (isHorizontal && OverlapsHorizontally(_source.screenRect, candidate.screenRect))
                {
                    continue;
                }

                Vector2 delta = candidate.center - _source.center;

                float primaryDistance = Vector2.Dot(delta, _direction);
                if (primaryDistance <= ALIGNMENT_EPSILON)
                {
                    continue;
                }

                float perpendicularDistance = Mathf.Abs(delta.x * _direction.y - delta.y * _direction.x);

                if (perpendicularDistance > primaryDistance * _maxConeTangent)
                {
                    continue;
                }

                float score = primaryDistance + (perpendicularDistance * PERPENDICULAR_WEIGHT);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate.selectable;
                }
            }

            // Up / Down move one row at a time: a well-aligned element two rows away must not beat the row in
            // between just because that row's element sits off to the side (e.g. a settings slider under a
            // two-column list of rebind buttons).
            bool isVertical = isHorizontal == false;
            if (bestCandidate != null && isVertical)
            {
                bestCandidate = FindBestInNearestRow(_source, _direction, _candidates);
            }

            // Left / Right wrap only inside the source's row (handled above); wrapping here would jump to
            // another row.
            if (bestCandidate == null && loopNavigation && isVertical)
            {
                bestCandidate = FindWrapCandidate(_source, _direction, _candidates);
            }

            return bestCandidate;
        }

        private static bool OverlapsHorizontally(Rect _a, Rect _b)
        {
            float horizontalOverlap = Mathf.Min(_a.xMax, _b.xMax) - Mathf.Max(_a.xMin, _b.xMin);
            return horizontalOverlap > ALIGNMENT_EPSILON;
        }

        /// <summary>
        /// Vertical step: the nearest row in _direction (by distance along it, inside the cone), then the
        /// element in that row with the best score (closest, least sideways).
        /// </summary>
        private Selectable FindBestInNearestRow(UIElementInfo _source, Vector2 _direction, List<UIElementInfo> _candidates)
        {
            bool hasNearest = false;
            UIElementInfo nearest = default;
            float nearestDistance = float.MaxValue;
            for (int index = 0; index < _candidates.Count; ++index)
            {
                UIElementInfo candidate = _candidates[index];
                bool isInCone = IsInDirectionalCone(_source, candidate, _direction);
                if (isInCone == false)
                {
                    continue;
                }

                float primaryDistance = Vector2.Dot(candidate.center - _source.center, _direction);
                if (primaryDistance < nearestDistance)
                {
                    nearestDistance = primaryDistance;
                    nearest = candidate;
                    hasNearest = true;
                }
            }

            if (hasNearest == false)
            {
                return null;
            }

            Selectable bestInRow = null;
            float bestScore = float.MaxValue;
            for (int index = 0; index < _candidates.Count; ++index)
            {
                UIElementInfo candidate = _candidates[index];
                bool isInNearestRow = IsSameRow(nearest.screenRect, candidate.screenRect);
                bool isInCone = IsInDirectionalCone(_source, candidate, _direction);
                if (isInNearestRow == false || isInCone == false)
                {
                    continue;
                }

                Vector2 delta = candidate.center - _source.center;
                float primaryDistance = Vector2.Dot(delta, _direction);
                float perpendicularDistance = Mathf.Abs(delta.x * _direction.y - delta.y * _direction.x);
                float score = primaryDistance + (perpendicularDistance * PERPENDICULAR_WEIGHT);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestInRow = candidate.selectable;
                }
            }

            return bestInRow;
        }

        /// <summary>True when _candidate lies ahead of _source in _direction, within the search cone.</summary>
        private bool IsInDirectionalCone(UIElementInfo _source, UIElementInfo _candidate, Vector2 _direction)
        {
            if (_candidate.selectable == _source.selectable)
            {
                return false;
            }

            Vector2 delta = _candidate.center - _source.center;
            float primaryDistance = Vector2.Dot(delta, _direction);
            if (primaryDistance <= ALIGNMENT_EPSILON)
            {
                return false;
            }

            float perpendicularDistance = Mathf.Abs(delta.x * _direction.y - delta.y * _direction.x);
            float maxConeTangent = Mathf.Tan(directionalConeAngle * Mathf.Deg2Rad);
            return perpendicularDistance <= primaryDistance * maxConeTangent;
        }

        private Selectable FindWrapCandidate(
            UIElementInfo _source,
            Vector2 _direction,
            List<UIElementInfo> _candidates)
        {
            Selectable wrapCandidate = null;
            float bestScore = float.MinValue;

            Vector2 oppositeDirection = -_direction;

            for (int index = 0; index < _candidates.Count; ++index)
            {
                UIElementInfo candidate = _candidates[index];
                if (candidate.selectable == _source.selectable)
                {
                    continue;
                }

                Vector2 delta = candidate.center - _source.center;

                float oppositeProjection = Vector2.Dot(delta, oppositeDirection);
                if (oppositeProjection <= ALIGNMENT_EPSILON)
                {
                    continue;
                }

                float perpendicularDistance = Mathf.Abs(delta.x * _direction.y - delta.y * _direction.x);
                float score = oppositeProjection - (perpendicularDistance * PERPENDICULAR_WEIGHT);

                if (score > bestScore)
                {
                    bestScore = score;
                    wrapCandidate = candidate.selectable;
                }
            }

            return wrapCandidate;
        }

        private bool IsSameRow(Rect _a, Rect _b)
        {
            float verticalOverlap = Mathf.Min(_a.yMax, _b.yMax) - Mathf.Max(_a.yMin, _b.yMin);
            if (verticalOverlap > 0.0f)
            {
                return true;
            }

            float centerDiffY = Mathf.Abs(_a.center.y - _b.center.y);
            float minHeight = Mathf.Min(_a.height, _b.height);
            float tolerance = Mathf.Max(15.0f, minHeight * 0.4f);
            return centerDiffY <= tolerance;
        }

        private List<Selectable> GetValidSelectables()
        {
            List<Selectable> validList = new List<Selectable>();
            Selectable[] allSelectables = GetComponentsInChildren<Selectable>(false);

            for (int index = 0; index < allSelectables.Length; ++index)
            {
                Selectable selectable = allSelectables[index];
                if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.IsInteractable())
                {
                    validList.Add(selectable);
                }
            }

            return validList;
        }

        private Vector2 GetScreenCenter(RectTransform _rectTransform)
        {
            if (_rectTransform == null)
            {
                return Vector2.zero;
            }

            Vector3 worldCenter = _rectTransform.TransformPoint(_rectTransform.rect.center);
            return RectTransformUtility.WorldToScreenPoint(targetCamera, worldCenter);
        }
    }
}