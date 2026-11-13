using UnityEngine;
using UnityEngine.Rendering;

namespace AbstractPixel.Settings
{
    // Player settings (brightness, motion blur) are written to a runtime-only volume layered just above the level's volume.
    // Reading Volume.profile instead would clone the level's whole profile: the Volume inspector then edits that clone,
    // so every override tweaked in Play mode (moon shafts, fog, ...) is lost on exit.
    public static class SettingsOverlayVolume
    {
        private const string OverlayObjectName = "Settings Overlay Volume";
        private const float PriorityAboveSource = 0.01f;

        // Returns the profile a setting should write T into, or null when the level's volume has no T (setting not used there).
        // The overlay's T starts with no parameter overridden, so it only changes what the setting itself overrides.
        public static VolumeProfile GetProfileForOverride<T>(Volume _sourceVolume) where T : VolumeComponent
        {
            if (_sourceVolume == null || _sourceVolume.sharedProfile == null)
            {
                return null;
            }

            bool levelUsesOverride = _sourceVolume.sharedProfile.Has<T>();
            if (!levelUsesOverride)
            {
                return null;
            }

            // A local volume only applies inside its collider; a global overlay can't copy that, so keep the old clone there.
            if (!_sourceVolume.isGlobal)
            {
                return _sourceVolume.profile;
            }

            VolumeProfile overlayProfile = GetOrCreateProfile(_sourceVolume);
            if (!overlayProfile.Has<T>())
            {
                overlayProfile.Add<T>();
            }
            return overlayProfile;
        }

        private static VolumeProfile GetOrCreateProfile(Volume _sourceVolume)
        {
            Volume existingOverlay = FindExistingOverlay(_sourceVolume);
            if (existingOverlay != null)
            {
                return existingOverlay.sharedProfile;
            }

            GameObject overlayObject = new GameObject(OverlayObjectName);
            overlayObject.transform.SetParent(_sourceVolume.transform, false);

            VolumeProfile overlayProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            overlayProfile.name = OverlayObjectName;

            Volume overlayVolume = overlayObject.AddComponent<Volume>();
            overlayVolume.isGlobal = true;
            overlayVolume.priority = _sourceVolume.priority + PriorityAboveSource;
            overlayVolume.weight = _sourceVolume.weight;
            overlayVolume.sharedProfile = overlayProfile;

            return overlayProfile;
        }

        private static Volume FindExistingOverlay(Volume _sourceVolume)
        {
            Transform overlayTransform = _sourceVolume.transform.Find(OverlayObjectName);
            if (overlayTransform == null)
            {
                return null;
            }

            overlayTransform.TryGetComponent(out Volume overlayVolume);
            return overlayVolume;
        }
    }
}
