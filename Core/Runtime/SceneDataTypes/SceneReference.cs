using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AbstractPixel.Core
{
    /// <summary>
    /// A scene in a scene group. It wraps one <see cref="SceneField"/>, which stores the scene asset's GUID, path and name.
    /// Two references match by GUID when both have one, otherwise by name. The loader decides from the asset's state
    /// whether Build Settings or Addressables opens it, so the reference does not say how a scene loads.
    /// </summary>
    [Serializable]
    public class SceneReference : IEquatable<SceneReference>
    {
        // Groups saved before A4 stored this as an auto-property backing field next to an Addressables reference.
        // The old name keeps every scene in every saved group.
        [FormerlySerializedAs("<SceneFieldReference>k__BackingField")]
        [SerializeField] private SceneField scene;

        public SceneReference(string sceneName)
        {
            scene = new SceneField(sceneName);
        }

        public SceneReference(string sceneName, string sceneGuid, string scenePath)
        {
            scene = new SceneField(sceneName, sceneGuid, scenePath);
        }

        /// <summary>The scene field behind this reference, for editor code that assigns the scene asset.</summary>
        public SceneField SceneFieldReference
        {
            get
            {
                if (scene == null)
                {
                    scene = new SceneField(string.Empty);
                }
                return scene;
            }
        }

        public string SceneName
        {
            get
            {
                if (scene == null)
                {
                    return string.Empty;
                }
                return scene.SceneName;
            }
        }

        /// <summary>The scene asset's GUID. Empty when the scene is known only by name.</summary>
        public string SceneGuid
        {
            get
            {
                if (scene == null)
                {
                    return string.Empty;
                }
                return scene.SceneGuid;
            }
        }

        /// <summary>The scene asset's path. Empty when the scene is known only by name.</summary>
        public string ScenePath
        {
            get
            {
                if (scene == null)
                {
                    return string.Empty;
                }
                return scene.ScenePath;
            }
        }

        public bool HasGuid
        {
            get { return !string.IsNullOrEmpty(SceneGuid); }
        }

        public bool Equals(SceneReference other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            bool bothHaveGuid = HasGuid && other.HasGuid;
            if (bothHaveGuid)
            {
                return string.Equals(SceneGuid, other.SceneGuid, StringComparison.Ordinal);
            }

            return string.Equals(SceneName, other.SceneName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SceneReference);
        }

        // Hashed by name, which every reference has, so a reference with a GUID and one without land in the same bucket
        // when they match by name. Two references with one GUID and different names only exist while a name is stale.
        public override int GetHashCode()
        {
            return !string.IsNullOrEmpty(SceneName)
                ? StringComparer.OrdinalIgnoreCase.GetHashCode(SceneName)
                : 0;
        }
    }
}
