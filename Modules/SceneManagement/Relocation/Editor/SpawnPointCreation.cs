using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// "Add spawn point to this scene group": a spawn point with the relocatable's label at the Scene view pivot, in the
    /// open main scene (the active scene). When ground is found below the pivot, it stands on it, so it is ready to use.
    /// </summary>
    internal static class SpawnPointCreation
    {
        private const float LookDown = 200f;

        public static SpawnPoint AddFor(Relocatable _relocatable)
        {
            Scene mainScene = SceneManager.GetActiveScene();
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 position = Vector3.zero;
            if (sceneView != null)
            {
                position = sceneView.pivot;
            }
            PreviewShape shape = PreviewShapes.For(_relocatable, _relocatable.Label);
            position = StandOnGroundBelow(position, shape);
            // Read before the new spawn point exists, so it never counts its own first position.
            RelocationEditorModel.MarkDirty();
            int firstInstance = RelocationEditorModel.Current.NextFreeInstance(_relocatable.Label);

            GameObject spawnObject = new GameObject(NameFor(_relocatable.Label));
            SceneManager.MoveGameObjectToScene(spawnObject, mainScene);
            spawnObject.transform.position = position;
            Undo.RegisterCreatedObjectUndo(spawnObject, "Add spawn point to this scene group");
            SpawnPoint spawnPoint = spawnObject.AddComponent<SpawnPoint>();
            spawnPoint.Label = _relocatable.Label;
            spawnPoint.SetInstance(0, firstInstance);
            EditorSceneManager.MarkSceneDirty(mainScene);
            RelocationEditorModel.MarkDirty();
            Selection.activeGameObject = spawnObject;
            return spawnPoint;
        }

        private static Vector3 StandOnGroundBelow(Vector3 _pivot, PreviewShape _shape)
        {
            Ray down = new Ray(_pivot + Vector3.up * LookDown * 0.5f, Vector3.down);
            if (!EditorGround.TryHit(down, out RaycastHit hit))
            {
                return _pivot;
            }
            return GroundPlacement.StandingPose(hit, Vector3.forward, _shape, false).position;
        }

        private static string NameFor(RelocationLabel _label)
        {
            return "Spawn point (" + RelocationLabels.NameOf(_label) + ")";
        }
    }
}
