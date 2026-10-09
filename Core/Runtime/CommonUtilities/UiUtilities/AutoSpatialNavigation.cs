using System.Collections;
using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    [DisallowMultipleComponent]
    public class AutoSpatialNavigation : MonoBehaviour
    {
        [Header("Auto Selection")]
        [Tooltip("If true, automatically selects the primary or override button when enabled.")]
        [SerializeField] private bool autoSelectOnEnable = true;

        [Tooltip("If true, auto-selection on enable only occurs if the active device is a Gamepad or Joystick. If false, always selects on enable so controller navigation is immediately available.")]
        [SerializeField] private bool requireGamepadForAutoSelect = false;

        [Tooltip("Optional explicit button to highlight first. If unassigned, automatically finds the top-leftmost button.")]
        [SerializeField] private Selectable firstSelectedOverride;

        [Tooltip("Maximum time in seconds to retry selecting the target button if it is waiting on layout calculation or an entrance animation.")]
        [SerializeField] private float selectionTimeout = 1.0f;

        [Header("Navigation Rules")]
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
        private Coroutine deferredSelectCoroutine;
        private Coroutine deferredInitCoroutine;

        private const float ALIGNMENT_EPSILON = 1.0f;
        private const float PERPENDICULAR_WEIGHT = 2.5f;
        private const float STICK_NAV_THRESHOLD = 0.35f;

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

            if (autoSelectOnEnable && ShouldAutoSelectOnEnable())
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

            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
                evaluationCoroutine = null;
            }

            if (deferredSelectCoroutine != null)
            {
                StopCoroutine(deferredSelectCoroutine);
                deferredSelectCoroutine = null;
            }

            if (deferredInitCoroutine != null)
            {
                StopCoroutine(deferredInitCoroutine);
                deferredInitCoroutine = null;
            }
        }

        private void Update()
        {
            EventSystem currentEventSystem = EventSystem.current;
            if (currentEventSystem == null)
            {
                return;
            }

            if (currentEventSystem.currentSelectedGameObject == null)
            {
                if (WasNavigationInputTriggered())
                {
                    TriggerAutoSelect();
                }
            }
        }

        private bool ShouldAutoSelectOnEnable()
        {
            if (!requireGamepadForAutoSelect)
            {
                return true;
            }

            if (InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick())
            {
                return true;
            }

            return Gamepad.current != null;
        }

        private bool WasNavigationInputTriggered()
        {
            Gamepad currentGamepad = Gamepad.current;
            if (currentGamepad != null)
            {
                if (currentGamepad.dpad.up.wasPressedThisFrame ||
                    currentGamepad.dpad.down.wasPressedThisFrame ||
                    currentGamepad.dpad.left.wasPressedThisFrame ||
                    currentGamepad.dpad.right.wasPressedThisFrame ||
                    currentGamepad.buttonSouth.wasPressedThisFrame)
                {
                    return true;
                }

                Vector2 leftStickValue = currentGamepad.leftStick.ReadValue();
                if (leftStickValue.sqrMagnitude >= STICK_NAV_THRESHOLD * STICK_NAV_THRESHOLD)
                {
                    return true;
                }
            }

            Keyboard currentKeyboard = Keyboard.current;
            if (currentKeyboard != null)
            {
                if (currentKeyboard.upArrowKey.wasPressedThisFrame ||
                    currentKeyboard.downArrowKey.wasPressedThisFrame ||
                    currentKeyboard.leftArrowKey.wasPressedThisFrame ||
                    currentKeyboard.rightArrowKey.wasPressedThisFrame ||
                    currentKeyboard.wKey.wasPressedThisFrame ||
                    currentKeyboard.sKey.wasPressedThisFrame ||
                    currentKeyboard.aKey.wasPressedThisFrame ||
                    currentKeyboard.dKey.wasPressedThisFrame ||
                    currentKeyboard.enterKey.wasPressedThisFrame ||
                    currentKeyboard.spaceKey.wasPressedThisFrame)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator DeferredInitialBuildRoutine()
        {
            yield return null;

            ForceRebuildAllLayouts();
            InitializeStateTracking();
            BuildNavigation();

            EventSystem currentEventSystem = EventSystem.current;
            if (autoSelectOnEnable && currentEventSystem != null && currentEventSystem.currentSelectedGameObject == null)
            {
                if (ShouldAutoSelectOnEnable())
                {
                    TriggerAutoSelect();
                }
            }

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
            if (!autoSelectOnEnable)
            {
                return;
            }

            if (_device is Gamepad || _device is Joystick)
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null && currentEventSystem.currentSelectedGameObject == null)
                {
                    TriggerAutoSelect();
                }
            }
            else if (_device is Mouse)
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null)
                {
                    currentEventSystem.SetSelectedGameObject(null);
                }
            }
        }

        public void TriggerAutoSelect()
        {
            if (deferredSelectCoroutine != null)
            {
                StopCoroutine(deferredSelectCoroutine);
            }
            deferredSelectCoroutine = StartCoroutine(DeferredSelectRoutine());
        }

        private IEnumerator DeferredSelectRoutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < selectionTimeout)
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null)
                {
                    if (firstSelectedOverride != null &&
                        firstSelectedOverride.gameObject.activeInHierarchy &&
                        firstSelectedOverride.IsInteractable())
                    {
                        currentEventSystem.SetSelectedGameObject(null);
                        currentEventSystem.SetSelectedGameObject(firstSelectedOverride.gameObject);
                        deferredSelectCoroutine = null;
                        yield break;
                    }

                    if (firstSelectedOverride == null)
                    {
                        List<Selectable> validSelectables = GetValidSelectables();
                        if (validSelectables.Count > 0)
                        {
                            validSelectables.Sort((_first, _second) =>
                            {
                                Vector2 posA = GetScreenCenter(_first.transform as RectTransform);
                                Vector2 posB = GetScreenCenter(_second.transform as RectTransform);

                                if (Mathf.Abs(posA.y - posB.y) <= 20.0f)
                                {
                                    return posA.x.CompareTo(posB.x);
                                }

                                return posB.y.CompareTo(posA.y);
                            });

                            currentEventSystem.SetSelectedGameObject(null);
                            currentEventSystem.SetSelectedGameObject(validSelectables[0].gameObject);
                            deferredSelectCoroutine = null;
                            yield break;
                        }
                    }
                }

                yield return null;
                elapsedTime += Time.unscaledDeltaTime;
            }

            deferredSelectCoroutine = null;
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

            if (count <= 1)
            {
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

                current.selectable.navigation = customNavigation;
            }
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

            if (bestCandidate == null && loopNavigation)
            {
                bestCandidate = FindWrapCandidate(_source, _direction, _candidates);
            }

            return bestCandidate;
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