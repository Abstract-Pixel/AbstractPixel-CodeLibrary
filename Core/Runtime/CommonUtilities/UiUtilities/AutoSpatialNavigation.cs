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

        private List<Selectable> trackedSelectables = new List<Selectable>();
        private List<bool> trackedInteractableStates = new List<bool>();
        private List<bool> trackedActiveStates = new List<bool>();
        private int cachedChildCount = -1;

        private Canvas cachedCanvas;
        private Camera targetCamera;
        private Coroutine evaluationCoroutine;
        private Coroutine deferredSelectCoroutine;

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
            cachedChildCount = transform.childCount;

            trackedSelectables.Clear();
            trackedInteractableStates.Clear();
            trackedActiveStates.Clear();

            Selectable[] allChildSelectables = GetComponentsInChildren<Selectable>(true);
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

                bool structureChanged = transform.childCount != cachedChildCount;
                if (structureChanged)
                {
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

            float maxConeTangent = Mathf.Tan(directionalConeAngle * Mathf.Deg2Rad);

            for (int index = 0; index < count; ++index)
            {
                Selectable currentSelectable = validSelectables[index];
                Vector2 currentCenter = GetScreenCenter(currentSelectable.transform as RectTransform);

                Navigation customNavigation = new Navigation();
                customNavigation.mode = Navigation.Mode.Explicit;

                customNavigation.selectOnRight = FindBestCandidate(currentSelectable, currentCenter, Vector2.right, validSelectables, maxConeTangent);
                customNavigation.selectOnLeft = FindBestCandidate(currentSelectable, currentCenter, Vector2.left, validSelectables, maxConeTangent);
                customNavigation.selectOnUp = FindBestCandidate(currentSelectable, currentCenter, Vector2.up, validSelectables, maxConeTangent);
                customNavigation.selectOnDown = FindBestCandidate(currentSelectable, currentCenter, Vector2.down, validSelectables, maxConeTangent);

                currentSelectable.navigation = customNavigation;
            }
        }

        private Selectable FindBestCandidate(
            Selectable _source,
            Vector2 _sourceCenter,
            Vector2 _direction,
            List<Selectable> _candidates,
            float _maxConeTangent)
        {
            Selectable bestCandidate = null;
            float bestScore = float.MaxValue;

            for (int index = 0; index < _candidates.Count; ++index)
            {
                Selectable candidate = _candidates[index];
                if (candidate == _source)
                {
                    continue;
                }

                Vector2 candidateCenter = GetScreenCenter(candidate.transform as RectTransform);
                Vector2 delta = candidateCenter - _sourceCenter;

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
                    bestCandidate = candidate;
                }
            }

            if (bestCandidate == null && loopNavigation)
            {
                bestCandidate = FindWrapCandidate(_source, _sourceCenter, _direction, _candidates);
            }

            return bestCandidate;
        }

        private Selectable FindWrapCandidate(
            Selectable _source,
            Vector2 _sourceCenter,
            Vector2 _direction,
            List<Selectable> _candidates)
        {
            Selectable wrapCandidate = null;
            float maxOppositeDistance = float.MinValue;

            Vector2 oppositeDirection = -_direction;

            for (int index = 0; index < _candidates.Count; ++index)
            {
                Selectable candidate = _candidates[index];
                if (candidate == _source)
                {
                    continue;
                }

                Vector2 candidateCenter = GetScreenCenter(candidate.transform as RectTransform);
                Vector2 delta = candidateCenter - _sourceCenter;

                float oppositeProjection = Vector2.Dot(delta, oppositeDirection);
                if (oppositeProjection > maxOppositeDistance)
                {
                    maxOppositeDistance = oppositeProjection;
                    wrapCandidate = candidate;
                }
            }

            return wrapCandidate;
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