using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The spawn point tool in the Scene view. Every position always shows its preview. Click a preview to select that
    /// position (its spawn point is selected for you) and move or turn it with Unity's handle; Shift+click a preview adds
    /// it to the selection. With a spawn point selected, holding Shift shows where a new position would go: a line from
    /// the spawn point, a sphere on the ground, the ghost standing there and its slope. Shift+click adds it;
    /// Ctrl+Shift+click adds it only on allowed ground.
    /// </summary>
    [InitializeOnLoad]
    internal static class SpawnPointSceneView
    {
        private static readonly int PreviewHash = "SpawnPointPreview".GetHashCode();
        private static readonly int AddHash = "SpawnPointAdd".GetHashCode();
        private static readonly List<int> previewControls = new List<int>();
        private static bool hidUnityTools;

        /// <summary>For stills only: stands in for the cursor with Shift held. Null in normal use.</summary>
        internal static Vector2? StillsCursor;

        static SpawnPointSceneView()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private static void OnSceneGui(SceneView _view)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            RelocationEditorModel model = RelocationEditorModel.Current;
            SpawnPoint selected = SelectedSpawnPoint();
            Event current = Event.current;
            MakeControls(model);
            int hovered = HoveredPosition();

            bool shiftHeld = current.shift || StillsCursor.HasValue;
            Vector2 cursor = current.mousePosition;
            if (StillsCursor.HasValue)
            {
                cursor = StillsCursor.Value;
            }

            if (current.type == EventType.Layout)
            {
                AddPreviewControls(model);
                if (selected != null && shiftHeld)
                {
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(AddHash, FocusType.Passive));
                }
            }
            if (current.type == EventType.MouseMove)
            {
                _view.Repaint();
            }
            if (current.type == EventType.Repaint)
            {
                DrawPreviews(_view, model, hovered);
                DrawLinkFromSelectedRelocatable(model);
            }

            bool showsAddGhost = selected != null && shiftHeld && hovered < 0;
            AddGhost ghost = null;
            if (showsAddGhost)
            {
                ghost = AddGhost.Under(cursor, selected, model);
                if (current.type == EventType.Repaint && ghost != null)
                {
                    ghost.Draw(selected);
                }
            }

            PublishMode(selected, shiftHeld, current.control || current.command, hovered);
            HandleClick(current, model, hovered, selected, ghost);
            ClearSelectionOnEscape(current);
            DrawPositionHandles(selected);
        }

        private static SpawnPoint SelectedSpawnPoint()
        {
            GameObject active = Selection.activeGameObject;
            if (active == null)
            {
                return null;
            }
            return active.GetComponent<SpawnPoint>();
        }

        // One control per position, made in the same order every event, so Layout and MouseDown agree.
        private static void MakeControls(RelocationEditorModel _model)
        {
            previewControls.Clear();
            for (int index = 0; index < _model.Positions.Count; index++)
            {
                previewControls.Add(GUIUtility.GetControlID(PreviewHash, FocusType.Passive));
            }
        }

        private static void AddPreviewControls(RelocationEditorModel _model)
        {
            for (int index = 0; index < _model.Positions.Count; index++)
            {
                RelocationEditorModel.Position position = _model.Positions[index];
                Bounds local = position.Shape.LocalBounds;
                Vector3 centre = position.Pose.position + position.Pose.rotation * local.center;
                float radius = Mathf.Max(0.25f, local.extents.magnitude * 0.6f);
                float distance = HandleUtility.DistanceToCircle(centre, radius);
                HandleUtility.AddControl(previewControls[index], distance);
            }
        }

        private static int HoveredPosition()
        {
            int nearest = HandleUtility.nearestControl;
            return previewControls.IndexOf(nearest);
        }

        private static void DrawPreviews(SceneView _view, RelocationEditorModel _model, int _hovered)
        {
            SpawnPoint selected = SelectedSpawnPoint();
            SpawnPointPreviewDrawing.BeginTags();
            DrawPriorityTags(_model, _hovered);
            for (int index = 0; index < _model.Positions.Count; index++)
            {
                RelocationEditorModel.Position position = _model.Positions[index];
                bool isSelected = SpawnPointPositionSelection.Contains(position.SpawnPoint, position.Address.Position);
                Color colour = SpawnPointPreviewDrawing.ColourFor(position);
                if (index == _hovered || isSelected)
                {
                    colour.a = Mathf.Min(1f, colour.a + 0.25f);
                }
                SpawnPointPreviewDrawing.DrawShape(position.Shape, position.Pose, colour);
                if (index == _hovered || isSelected)
                {
                    Color outline = colour;
                    outline.a = 1f;
                    SpawnPointPreviewDrawing.DrawOutline(position.Shape, position.Pose, outline);
                }

                bool belongsToSelection = selected != null && position.SpawnPoint == selected;
                if (belongsToSelection || SpawnPointPreviewDrawing.IsNearCamera(_view, position.Pose.position))
                {
                    DrawPositionTag(position, false);
                }
            }
        }

        // The hovered and the selected positions get their tags first, so crowding never hides them.
        private static void DrawPriorityTags(RelocationEditorModel _model, int _hovered)
        {
            for (int index = 0; index < _model.Positions.Count; index++)
            {
                RelocationEditorModel.Position position = _model.Positions[index];
                bool isSelected = SpawnPointPositionSelection.Contains(position.SpawnPoint, position.Address.Position);
                if (index == _hovered || isSelected)
                {
                    DrawPositionTag(position, true);
                }
            }
        }

        private static void DrawPositionTag(RelocationEditorModel.Position _position, bool _force)
        {
            Vector3 above = _position.Pose.position + _position.Pose.rotation * new Vector3(0f, _position.Shape.LocalBounds.max.y + 0.2f, 0f);
            Color tagColour = SpawnPointPreviewDrawing.ColourFor(_position);
            tagColour.a = 1f;
            SpawnPointPreviewDrawing.DrawTag(above, SpawnPointPreviewDrawing.TagFor(_position), tagColour, _force);
        }

        private static void DrawLinkFromSelectedRelocatable(RelocationEditorModel _model)
        {
            GameObject active = Selection.activeGameObject;
            if (active == null)
            {
                return;
            }
            Relocatable relocatable = active.GetComponent<Relocatable>();
            if (relocatable == null || !_model.GoesTo.TryGetValue(relocatable, out RelocationEditorModel.Position position))
            {
                return;
            }
            Handles.color = RelocationLabels.ColourOf(relocatable.Label);
            Handles.DrawDottedLine(relocatable.transform.position, position.Pose.position, 4f);
        }

        private static void HandleClick(Event _event, RelocationEditorModel _model, int _hovered, SpawnPoint _selected, AddGhost _ghost)
        {
            bool isPlainLeftClick = _event.type == EventType.MouseDown && _event.button == 0 && !_event.alt;
            if (!isPlainLeftClick)
            {
                return;
            }
            if (_hovered >= 0)
            {
                RelocationEditorModel.Position position = _model.Positions[_hovered];
                SpawnPointPositionSelection.Select(position.SpawnPoint, position.Address.Position, _event.shift);
                _event.Use();
                return;
            }
            if (_selected == null || !_event.shift)
            {
                return;
            }
            // Shift+click on ground adds; Ctrl makes it strict, so a red preview adds nothing. No hit adds nothing.
            _event.Use();
            if (_ghost == null)
            {
                return;
            }
            bool strict = _event.control;
            if (strict && !_ghost.IsAllowed)
            {
                return;
            }
            int number = SpawnPointEditing.Add(_selected, _ghost.Pose);
            SpawnPointPositionSelection.Set(_selected, new List<int> { number });
        }

        // The control list in the inspector and the Overlay lights up the row for what a click would do now.
        private static void PublishMode(SpawnPoint _selected, bool _shiftHeld, bool _ctrlHeld, int _hovered)
        {
            SpawnPointToolMode mode = SpawnPointToolMode.Ready;
            if (_selected != null && SpawnPointPositionSelection.SpawnPoint == _selected && SpawnPointPositionSelection.HasAny)
            {
                mode = SpawnPointToolMode.Editing;
            }
            if (_selected != null && _shiftHeld)
            {
                mode = SpawnPointToolMode.Adding;
                if (_ctrlHeld)
                {
                    mode = SpawnPointToolMode.AddingStrict;
                }
                if (_hovered >= 0)
                {
                    mode = SpawnPointToolMode.AddingToSelection;
                }
            }
            SpawnPointToolState.Mode = mode;
        }

        // Esc clears the position selection. Unity's Shortcut Manager cannot bind Esc, so it is read here.
        private static void ClearSelectionOnEscape(Event _event)
        {
            bool isEscape = _event.type == EventType.KeyDown && _event.keyCode == KeyCode.Escape;
            if (isEscape && SpawnPointPositionSelection.HasAny)
            {
                SpawnPointPositionSelection.Clear();
                _event.Use();
            }
        }

        private static void DrawPositionHandles(SpawnPoint _selected)
        {
            bool hasPositions = _selected != null && SpawnPointPositionSelection.SpawnPoint == _selected && SpawnPointPositionSelection.HasAny;
            if (!hasPositions)
            {
                RestoreUnityTools();
                return;
            }
            Tools.hidden = true;
            hidUnityTools = true;

            Pose pivot = _selected.GetWorldPose(SpawnPointPositionSelection.Positions[0]);
            EditorGUI.BeginChangeCheck();
            if (Tools.current == Tool.Rotate)
            {
                Quaternion turned = Handles.RotationHandle(pivot.rotation, pivot.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Quaternion turn = turned * Quaternion.Inverse(pivot.rotation);
                    SpawnPointEditing.MoveSelected(Vector3.zero, turn);
                }
                return;
            }

            Quaternion handleRotation = Quaternion.identity;
            if (Tools.pivotRotation == PivotRotation.Local)
            {
                handleRotation = pivot.rotation;
            }
            Vector3 moved = Handles.PositionHandle(pivot.position, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                SpawnPointEditing.MoveSelected(moved - pivot.position, Quaternion.identity);
            }
        }

        private static void RestoreUnityTools()
        {
            if (!hidUnityTools)
            {
                return;
            }
            Tools.hidden = false;
            hidUnityTools = false;
        }

        /// <summary>Where a Shift+click would add a position: on the ground under the cursor, standing on its lowest point.</summary>
        private sealed class AddGhost
        {
            public Pose Pose;
            public Vector3 HitPoint;
            public Vector3 Normal;
            public float Slope;
            public float Steepest;
            public bool IsAllowed;
            public PreviewShape Shape;
            public int NextInstance;

            public static AddGhost Under(Vector2 _mousePosition, SpawnPoint _spawnPoint, RelocationEditorModel _model)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(_mousePosition);
                if (!EditorGround.TryHit(ray, out RaycastHit hit))
                {
                    return null;
                }
                AddGhost ghost = new AddGhost();
                ghost.HitPoint = hit.point;
                ghost.Normal = hit.normal;
                ghost.Slope = EditorGround.SlopeOf(hit.normal);
                ghost.Steepest = RelocationLabels.SteepestGroundOf(_spawnPoint.Label);
                ghost.IsAllowed = ghost.Slope <= ghost.Steepest + 0.05f;
                ghost.Shape = ShapeForLabel(_spawnPoint.Label, _model);
                ghost.NextInstance = _model.NextFreeInstance(_spawnPoint.Label);
                ghost.Pose = StandingPose(hit, _spawnPoint, ghost.Shape);
                return ghost;
            }

            public void Draw(SpawnPoint _spawnPoint)
            {
                Color state = SpawnPointPreviewDrawing.Allowed;
                if (!IsAllowed)
                {
                    state = SpawnPointPreviewDrawing.TooSteep;
                }
                Handles.color = state;
                Handles.DrawLine(_spawnPoint.transform.position, HitPoint, 2f);
                float size = HandleUtility.GetHandleSize(HitPoint) * 0.12f;
                Handles.SphereHandleCap(0, HitPoint, Quaternion.identity, size, EventType.Repaint);
                Handles.DrawLine(HitPoint, HitPoint + Normal * size * 4f, 2f);

                Color ghostColour = state;
                ghostColour.a = 0.5f;
                SpawnPointPreviewDrawing.DrawShape(Shape, Pose, ghostColour);
                SpawnPointPreviewDrawing.DrawOutline(Shape, Pose, state);

                string labelName = RelocationLabels.NameOf(_spawnPoint.Label);
                string text = labelName + " · next: Instance " + NextInstance;
                if (!IsAllowed)
                {
                    text += "\n" + Mathf.RoundToInt(Slope) + "°, steepest allowed for " + labelName + " is " + Mathf.RoundToInt(Steepest) + "°";
                }
                Vector3 above = Pose.position + Pose.rotation * new Vector3(0f, Shape.LocalBounds.max.y + 0.2f, 0f);
                SpawnPointPreviewDrawing.DrawTag(above, text, state, true);
            }

            // Upright and facing the spawn point's forward; tilted to the ground only when the label aligns to surface.
            private static Pose StandingPose(RaycastHit _hit, SpawnPoint _spawnPoint, PreviewShape _shape)
            {
                bool alignToSurface = _spawnPoint.Label != null && _spawnPoint.Label.AlignToSurface;
                return GroundPlacement.StandingPose(_hit, _spawnPoint.transform.forward, _shape, alignToSurface);
            }

            // The first relocatable with this label shows what will land; otherwise the label's prefab or the box.
            private static PreviewShape ShapeForLabel(RelocationLabel _label, RelocationEditorModel _model)
            {
                foreach (Relocatable relocatable in _model.Relocatables)
                {
                    if (relocatable.Label == _label)
                    {
                        return PreviewShapes.For(relocatable, _label);
                    }
                }
                return PreviewShapes.For(null, _label);
            }
        }
    }
}
