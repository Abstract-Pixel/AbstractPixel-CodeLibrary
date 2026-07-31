#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Tooltip.Editor
{
    [InitializeOnLoad]
    public static class TooltipPreviewUtility
    {
        public static bool IsPreviewActive { get; private set; }
        public static TooltipConfig CurrentConfig { get; private set; }

        // Live Overrides
        public static GameObject PreviewTargetObject;
        public static int PreviewSortingOrder = 3000;
        public static string PreviewHeader = "Header Preview";
        public static string PreviewBody = "This is sample body text to test scaling, text wrapping, and offsets in real-time.";

        private static GameObject previewCanvas;
        private static GameObject dummyTarget;
        private static TooltipView previewInstance;

        private static bool lastKnownWorldSpaceState;
        private static TooltipView lastKnownPrefab;

        static TooltipPreviewUtility()
        {
            EditorApplication.update += UpdatePreview;
            AssemblyReloadEvents.beforeAssemblyReload += ForceCleanup;
            EditorApplication.playModeStateChanged += HandlePlayModeChange;
            Undo.undoRedoPerformed += OnUndoRedo;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public static void TogglePreview(TooltipConfig config)
        {
            if (IsPreviewActive && CurrentConfig == config)
            {
                ForceCleanup();
            }
            else
            {
                ForceCleanup();
                CurrentConfig = config;
                IsPreviewActive = true;
            }
        }

        private static void HandlePlayModeChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                ForceCleanup();
            }
        }

        private static void OnUndoRedo()
        {
            if (IsPreviewActive)
            {
                ForceCleanup();
                IsPreviewActive = true;
            }
        }

        private static void OnSelectionChanged()
        {
            if (IsPreviewActive) SceneView.RepaintAll();
        }

        private static void UpdatePreview()
        {
            if (!IsPreviewActive || Application.isPlaying || CurrentConfig == null || CurrentConfig.PositioningStrategy == null || CurrentConfig.TooltipPrefab == null)
            {
                return;
            }

            if (previewCanvas != null && (lastKnownWorldSpaceState != CurrentConfig.isWorldSpace || lastKnownPrefab != CurrentConfig.TooltipPrefab))
            {
                ForceCleanup();
                IsPreviewActive = true;
                return;
            }

            // 1. Construct Canvas and Dummy Target
            if (previewCanvas == null)
            {
                previewCanvas = new GameObject("[Tooltip_Preview_Canvas]");
                previewCanvas.hideFlags = HideFlags.DontSave;

                Canvas canvas = previewCanvas.AddComponent<Canvas>();
                canvas.sortingOrder = PreviewSortingOrder;

                if (CurrentConfig.isWorldSpace)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    previewCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }

                dummyTarget = new GameObject("[Tooltip_Preview_Target]");
                dummyTarget.hideFlags = HideFlags.DontSave;

                previewInstance = Object.Instantiate(CurrentConfig.TooltipPrefab, previewCanvas.transform);
                previewInstance.gameObject.hideFlags = HideFlags.None;
                previewInstance.gameObject.SetActive(true);

                lastKnownWorldSpaceState = CurrentConfig.isWorldSpace;
                lastKnownPrefab = CurrentConfig.TooltipPrefab;
            }

            // Update live canvas sorting order
            Canvas liveCanvas = previewCanvas.GetComponent<Canvas>();
            if (liveCanvas != null)
            {
                liveCanvas.sortingOrder = PreviewSortingOrder;
            }

            // 2. Target Alignment Priority: Explicit Picked Object -> Active Hierarchy Selection -> World Origin
            Transform activeTargetTransform = GetActiveTargetTransform();
            dummyTarget.transform.position = activeTargetTransform.position;
            dummyTarget.transform.rotation = activeTargetTransform.rotation;

            // 3. Inject Live Text Data & Initialize Layout
            TooltipData dummyData = new TooltipData(
                PreviewHeader,
                PreviewBody,
                null,
                CurrentConfig,
                dummyTarget.transform
            );

            // Re-runs layout rebuilding and pivot setup live every frame
            previewInstance.Initialize(dummyData);

            // 4. Execute Positioning Strategy
            CurrentConfig.PositioningStrategy.ExecutePositioning(previewInstance.tooltipHolder, dummyTarget.transform, CurrentConfig.isWorldSpace);

            SceneView.RepaintAll();
        }

        private static Transform GetActiveTargetTransform()
        {
            // Priority 1: Explicitly assigned target object in the preview box
            if (PreviewTargetObject != null)
            {
                return PreviewTargetObject.transform;
            }

            // Priority 2: Currently selected object in hierarchy (excluding preview objects)
            GameObject currentSelection = Selection.activeGameObject;
            if (currentSelection != null && currentSelection != previewCanvas && currentSelection != dummyTarget && (previewInstance == null || currentSelection != previewInstance.gameObject))
            {
                return currentSelection.transform;
            }

            // Priority 3: Fallback to dummy target self
            return dummyTarget.transform;
        }

        public static void ForceCleanup()
        {
            if (previewCanvas != null) Object.DestroyImmediate(previewCanvas);
            if (dummyTarget != null) Object.DestroyImmediate(dummyTarget);

            previewCanvas = null;
            dummyTarget = null;
            previewInstance = null;
            PreviewTargetObject = null;
            IsPreviewActive = false;
            CurrentConfig = null;

            SceneView.RepaintAll();
        }
    }
}
#endif