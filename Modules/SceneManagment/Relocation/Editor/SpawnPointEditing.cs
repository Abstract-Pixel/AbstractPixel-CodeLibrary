using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Edits spawn point positions with Undo: add at a pose with the next free instance, duplicate, remove and frame the
    /// selected positions. The Scene view tool and the hotkeys call it.
    /// </summary>
    internal static class SpawnPointEditing
    {
        public static int Add(SpawnPoint _spawnPoint, Pose _pose)
        {
            int instance = RelocationEditorModel.Current.NextFreeInstance(_spawnPoint.Label);
            Undo.RecordObject(_spawnPoint, "Add spawn point position");
            int number = _spawnPoint.AddPosition(_pose, instance);
            Changed(_spawnPoint);
            return number;
        }

        public static void DuplicateSelected()
        {
            SpawnPoint spawnPoint = SpawnPointPositionSelection.SpawnPoint;
            if (spawnPoint == null || !SpawnPointPositionSelection.HasAny)
            {
                return;
            }
            Undo.RecordObject(spawnPoint, "Duplicate spawn point positions");
            float offset = SpawnPointWidth(spawnPoint) * 1.1f;
            int nextInstance = RelocationEditorModel.Current.NextFreeInstance(spawnPoint.Label);
            List<int> copies = new List<int>();
            foreach (int position in SpawnPointPositionSelection.Positions)
            {
                Pose pose = spawnPoint.GetWorldPose(position);
                Pose copy = new Pose(pose.position + pose.rotation * Vector3.right * offset, pose.rotation);
                copies.Add(spawnPoint.AddPosition(copy, nextInstance));
                nextInstance++;
            }
            Changed(spawnPoint);
            SpawnPointPositionSelection.Set(spawnPoint, copies);
        }

        public static void RemoveSelected()
        {
            SpawnPoint spawnPoint = SpawnPointPositionSelection.SpawnPoint;
            if (spawnPoint == null || !SpawnPointPositionSelection.HasAny)
            {
                return;
            }
            List<int> doomed = new List<int>(SpawnPointPositionSelection.Positions);
            doomed.Sort();
            doomed.Reverse();
            Undo.RecordObjects(new Object[] { spawnPoint, spawnPoint.transform }, "Remove spawn point positions");
            foreach (int position in doomed)
            {
                bool removed = spawnPoint.RemovePosition(position);
                if (!removed)
                {
                    Debug.LogWarning("[Relocation] '" + spawnPoint.name + "' keeps its last position. Delete the spawn point to remove it.", spawnPoint);
                }
            }
            SpawnPointPositionSelection.Clear();
            Changed(spawnPoint);
        }

        public static void FrameSelected()
        {
            SpawnPoint spawnPoint = SpawnPointPositionSelection.SpawnPoint;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (spawnPoint == null || sceneView == null || !SpawnPointPositionSelection.HasAny)
            {
                return;
            }
            Bounds bounds = new Bounds(spawnPoint.GetWorldPose(SpawnPointPositionSelection.Positions[0]).position, Vector3.one * 2f);
            foreach (int position in SpawnPointPositionSelection.Positions)
            {
                bounds.Encapsulate(new Bounds(spawnPoint.GetWorldPose(position).position, Vector3.one * 2f));
            }
            sceneView.Frame(bounds, false);
        }

        /// <summary>Moves the given positions by a world offset and turn around each position, with Undo.</summary>
        public static void MoveSelected(Vector3 _offset, Quaternion _turn)
        {
            SpawnPoint spawnPoint = SpawnPointPositionSelection.SpawnPoint;
            if (spawnPoint == null || !SpawnPointPositionSelection.HasAny)
            {
                return;
            }
            Undo.RecordObjects(new Object[] { spawnPoint, spawnPoint.transform }, "Move spawn point positions");
            List<Pose> newPoses = new List<Pose>();
            foreach (int position in SpawnPointPositionSelection.Positions)
            {
                Pose pose = spawnPoint.GetWorldPose(position);
                newPoses.Add(new Pose(pose.position + _offset, _turn * pose.rotation));
            }
            // Position 0 is the spawn point's own transform; it moves last so the stored positions keep their place.
            for (int index = 0; index < newPoses.Count; index++)
            {
                int position = SpawnPointPositionSelection.Positions[index];
                if (position != 0)
                {
                    spawnPoint.SetWorldPose(position, newPoses[index]);
                }
            }
            for (int index = 0; index < newPoses.Count; index++)
            {
                if (SpawnPointPositionSelection.Positions[index] == 0)
                {
                    spawnPoint.SetWorldPose(0, newPoses[index]);
                }
            }
            Changed(spawnPoint);
        }

        private static float SpawnPointWidth(SpawnPoint _spawnPoint)
        {
            foreach (RelocationEditorModel.Position position in RelocationEditorModel.Current.PositionsOf(_spawnPoint))
            {
                return Mathf.Max(1f, position.Shape.Width);
            }
            return 1f;
        }

        private static void Changed(SpawnPoint _spawnPoint)
        {
            EditorUtility.SetDirty(_spawnPoint);
            RelocationEditorModel.MarkDirty();
            SceneView.RepaintAll();
        }
    }
}
