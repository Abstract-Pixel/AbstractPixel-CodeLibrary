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
        [Tooltip("If true, automatically selects the primary (top-leftmost) button when enabled on a gamepad or joystick.")]
        [SerializeField] private bool autoSelectOnEnable = true;

        [Tooltip("Optional explicit button to highlight first. If unassigned, automatically finds the top-leftmost button.")]
        [SerializeField] private Selectable firstSelectedOverride;

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
        private int cachedSelectableCount = -1;

        private Canvas cachedCanvas;
        private Camera targetCamera;
        private Coroutine evaluationCoroutine;
        private Coroutine deferredSelectCoroutine;
        private Coroutine deferredInitCoroutine;

        private const float ALIGNMENT_EPSILON = 1.0f;
        private const float PERPENDICULAR_WEIGHT = 2.5f;

        private void Awake()
        {
            ResolveCanvasAndCamera();
        }

        private void OnEnable()
        {
            ResolveCanvasAndCamera();
            InitializeStateTracking();

            // 1. Immediate build
            Canvas.ForceUpdateCanvases();
            BuildNavigation();

            InputDeviceTracker.OnCurrentInputDeviceChanged -= HandleDeviceChanged;
            InputDeviceTracker.OnCurrentInputDeviceChanged += HandleDeviceChanged;

            if (autoSelectOnEnable && InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick())
            {
                TriggerAutoSelect();
            }

            if (evaluationCoroutine != null)
            {
                StopCoroutine(evaluationCoroutine);
            }
            evaluationCoroutine = StartCoroutine(StateEvaluationRoutine());

            // 2. Deferred build for elements dynamically created or laid out on Frame 0
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

        private IEnumerator DeferredInitialBuildRoutine()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();
            InitializeStateTracking();
            BuildNavigation();

            if (autoSelectOnEnable && InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick())
            {
                EventSystem eventSystem = EventSystem.current;
                if (eventSystem != null && eventSystem.currentSelectedGameObject == null)
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
            else
            {
                EventSystem currentEventSystem = EventSystem.current;
                if (currentEventSystem != null)
                {
                    currentEventSystem.SetSelectedGameObject(null);
                }
            }
        }

        private void TriggerAutoSelect()
        {
            if (deferredSelectCoroutine != null)
            {
                StopCoroutine(deferredSelectCoroutine);
            }
            deferredSelectCoroutine = StartCoroutine(DeferredSelectRoutine());
        }

        private IEnumerator DeferredSelectRoutine()
        {
            yield return null;

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                yield break;
            }

            if (firstSelectedOverride != null && firstSelectedOverride.gameObject.activeInHierarchy && firstSelectedOverride.interactable)
            {
                eventSystem.SetSelectedGameObject(firstSelectedOverride.gameObject);
                deferredSelectCoroutine = null;
                yield break;
            }

            List<Selectable> validSelectables = GetValidSelectables();
            if (validSelectables.Count == 0)
            {
                deferredSelectCoroutine = null;
                yield break;
            }

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

            eventSystem.SetSelectedGameObject(validSelectables[0].gameObject);
            deferredSelectCoroutine = null;
        }

        private void InitializeStateTracking()
        {
            Selectable[] allChildSelectables = GetComponentsInChildren<Selectable>(true);
            cachedSelectableCount = allChildSelectables.Length;

            trackedSelectables.Clear();
            trackedInteractableStates.Clear();
            trackedActiveStates.Clear();

            for (int index = 0; index < allChildSelectables.Length; ++index)
            {
                Selectable selectable = allChildSelectables[index];
                if (selectable != null)
                {
                    trackedSelectables.Add(selectable);
                    trackedInteractableStates.Add(selectable.interactable);
                    trackedActiveStates.Add(selectable.gameObject.activeInHierarchy);
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
                    Canvas.ForceUpdateCanvases();
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

                    bool currentInteractable = selectable.interactable;
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
                    Canvas.ForceUpdateCanvases();
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

            Canvas.ForceUpdateCanvases();

            // Precompute screen rects for all elements to avoid repeated WorldToScreenPoint calculations
            List<UIElementInfo> elementInfos = new List<UIElementInfo>(count);
            Vector3[] corners = new Vector3[4];

            for (int i = 0; i < count; ++i)
            {
                Selectable sel = validSelectables[i];
                RectTransform rt = sel.transform as RectTransform;
                if (rt == null) continue;

                rt.GetWorldCorners(corners);
                Vector2 p0 = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[0]);
                Vector2 p2 = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[2]);

                float xMin = Mathf.Min(p0.x, p2.x);
                float xMax = Mathf.Max(p0.x, p2.x);
                float yMin = Mathf.Min(p0.y, p2.y);
                float yMax = Mathf.Max(p0.y, p2.y);

                elementInfos.Add(new UIElementInfo
                {
                    selectable = sel,
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

            // -------------------------------------------------------------
            // HORIZONTAL ROW LOGIC (Left / Right)
            // -------------------------------------------------------------
            if (isHorizontal)
            {
                List<UIElementInfo> sameRowElements = new List<UIElementInfo>();
                for (int i = 0; i < _candidates.Count; ++i)
                {
                    if (_candidates[i].selectable == _source.selectable) continue;

                    if (IsSameRow(_source.screenRect, _candidates[i].screenRect))
                    {
                        sameRowElements.Add(_candidates[i]);
                    }
                }

                // If other elements share this row, navigate strictly along this row
                if (sameRowElements.Count > 0)
                {
                    UIElementInfo bestRowCandidate = default;
                    float bestDistance = float.MaxValue;
                    bool foundInDirection = false;

                    for (int i = 0; i < sameRowElements.Count; ++i)
                    {
                        UIElementInfo cand = sameRowElements[i];
                        float deltaX = cand.center.x - _source.center.x;

                        if (_direction.x > 0 && deltaX > ALIGNMENT_EPSILON) // Moving Right
                        {
                            if (deltaX < bestDistance)
                            {
                                bestDistance = deltaX;
                                bestRowCandidate = cand;
                                foundInDirection = true;
                            }
                        }
                        else if (_direction.x < 0 && deltaX < -ALIGNMENT_EPSILON) // Moving Left
                        {
                            float dist = -deltaX;
                            if (dist < bestDistance)
                            {
                                bestDistance = dist;
                                bestRowCandidate = cand;
                                foundInDirection = true;
                            }
                        }
                    }

                    if (foundInDirection)
                    {
                        return bestRowCandidate.selectable;
                    }

                    // At edge of row: Loop to the opposite end of the SAME row
                    if (loopNavigation)
                    {
                        UIElementInfo wrapCandidate = default;
                        float extremeX = _direction.x > 0 ? float.MaxValue : float.MinValue;

                        for (int i = 0; i < sameRowElements.Count; ++i)
                        {
                            UIElementInfo cand = sameRowElements[i];
                            if (_direction.x > 0) // Moving Right wraps to leftmost button on this row
                            {
                                if (cand.center.x < extremeX)
                                {
                                    extremeX = cand.center.x;
                                    wrapCandidate = cand;
                                }
                            }
                            else // Moving Left wraps to rightmost button on this row
                            {
                                if (cand.center.x > extremeX)
                                {
                                    extremeX = cand.center.x;
                                    wrapCandidate = cand;
                                }
                            }
                        }

                        if (wrapCandidate.selectable != null)
                        {
                            return wrapCandidate.selectable;
                        }
                    }

                    // Reached edge of a multi-button row without looping; do not jump to other rows
                    return null;
                }
            }

            // -------------------------------------------------------------
            // GENERAL / VERTICAL CONE SEARCH (Up / Down / Isolated buttons)
            // -------------------------------------------------------------
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

                // Prioritize candidate furthest in the opposite direction while penalizing perpendicular misalignment
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
            // 1. Direct vertical overlap between element rects
            float verticalOverlap = Mathf.Min(_a.yMax, _b.yMax) - Mathf.Max(_a.yMin, _b.yMin);
            if (verticalOverlap > 0.0f)
            {
                return true;
            }

            // 2. Tolerance for slight visual misalignment
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
                if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.interactable)
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