using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Builds and caches preview shapes, in the order the Scene view uses them: the real meshes of the relocatable that
    /// lands at a position, else the label's preview prefab, else a 1 m box with a forward arrow. The cache is cleared
    /// when the hierarchy or a project asset changes.
    /// </summary>
    [InitializeOnLoad]
    internal static class PreviewShapes
    {
        private static readonly Dictionary<int, PreviewShape> cache = new Dictionary<int, PreviewShape>();
        private static PreviewShape box;

        static PreviewShapes()
        {
            EditorApplication.hierarchyChanged += Clear;
            EditorApplication.projectChanged += Clear;
            Undo.undoRedoPerformed += Clear;
        }

        public static void Clear()
        {
            cache.Clear();
        }

        /// <summary>The preview for a position: the relocatable's meshes, else the label's prefab, else the box.</summary>
        public static PreviewShape For(Relocatable _relocatable, RelocationLabel _label)
        {
            if (_relocatable != null)
            {
                PreviewShape fromObject = FromObject(_relocatable.gameObject);
                if (fromObject.Meshes.Count > 0)
                {
                    return fromObject;
                }
            }
            if (_label != null && _label.PreviewPrefab != null)
            {
                PreviewShape fromPrefab = FromObject(_label.PreviewPrefab);
                if (fromPrefab.Meshes.Count > 0)
                {
                    return fromPrefab;
                }
            }
            return Box();
        }

        public static PreviewShape Box()
        {
            if (box != null)
            {
                return box;
            }
            box = new PreviewShape();
            box.IsFallbackBox = true;
            Mesh cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            box.Add(cube, Matrix4x4.TRS(new Vector3(0f, 0.5f, 0f), Quaternion.identity, Vector3.one));
            return box;
        }

        private static PreviewShape FromObject(GameObject _root)
        {
            int key = _root.GetInstanceID();
            if (cache.TryGetValue(key, out PreviewShape cached))
            {
                return cached;
            }

            PreviewShape shape = new PreviewShape();
            Matrix4x4 rootInverse = Matrix4x4.TRS(_root.transform.position, _root.transform.rotation, Vector3.one).inverse;
            foreach (MeshFilter filter in _root.GetComponentsInChildren<MeshFilter>(false))
            {
                Renderer renderer = filter.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }
                shape.Add(filter.sharedMesh, rootInverse * filter.transform.localToWorldMatrix);
            }
            foreach (SkinnedMeshRenderer skinned in _root.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                shape.Add(skinned.sharedMesh, rootInverse * skinned.transform.localToWorldMatrix);
            }
            if (shape.Meshes.Count == 0)
            {
                AddColliderShapes(_root, rootInverse, shape);
            }
            cache[key] = shape;
            return shape;
        }

        // An object with no meshes, such as a trigger zone, is drawn from its own colliders: boxes and spheres as they are.
        private static void AddColliderShapes(GameObject _root, Matrix4x4 _rootInverse, PreviewShape _shape)
        {
            Mesh cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Mesh sphere = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
            foreach (Collider collider in _root.GetComponentsInChildren<Collider>(false))
            {
                Matrix4x4 local = _rootInverse * collider.transform.localToWorldMatrix;
                BoxCollider boxCollider = collider as BoxCollider;
                if (boxCollider != null)
                {
                    _shape.Add(cube, local * Matrix4x4.TRS(boxCollider.center, Quaternion.identity, boxCollider.size));
                    continue;
                }
                SphereCollider sphereCollider = collider as SphereCollider;
                if (sphereCollider != null && sphere != null)
                {
                    float diameter = sphereCollider.radius * 2f;
                    _shape.Add(sphere, local * Matrix4x4.TRS(sphereCollider.center, Quaternion.identity, Vector3.one * diameter));
                }
            }
        }
    }
}
