using System;
using UnityEditor;
using UnityEditor.Build.Profile;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// The scene list the next build uses: the active build profile's own list when it overrides the shared one,
    /// otherwise Build Settings. A status read from the shared list alone would call a left-out scene "In build".
    /// </summary>
    internal static class ActiveSceneList
    {
        /// <summary>The active build profile's own scene list, or null when it uses the shared one.</summary>
        internal static Func<EditorBuildSettingsScene[]> ProfileOverride = ReadProfileOverride;

        internal static EditorBuildSettingsScene[] Read()
        {
            EditorBuildSettingsScene[] own = ProfileOverride();
            return own ?? EditorBuildSettings.scenes;
        }

        /// <summary>Writes the list that <see cref="Read"/> reads, so a fix changes the status it was offered for.</summary>
        internal static void Write(EditorBuildSettingsScene[] _scenes)
        {
            BuildProfile profile = ProfileWithItsOwnList();
            if (profile == null)
            {
                SceneBuildSettingsWriter.Write(_scenes);
                return;
            }

            Undo.RecordObject(profile, "Change the build profile's scene list");
            profile.scenes = _scenes;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
        }

        private static EditorBuildSettingsScene[] ReadProfileOverride()
        {
            BuildProfile profile = ProfileWithItsOwnList();
            return profile == null ? null : profile.scenes;
        }

        private static BuildProfile ProfileWithItsOwnList()
        {
            BuildProfile profile = BuildProfile.GetActiveBuildProfile();
            bool hasOwnList = profile != null && profile.overrideGlobalScenes;
            return hasOwnList ? profile : null;
        }
    }
}
