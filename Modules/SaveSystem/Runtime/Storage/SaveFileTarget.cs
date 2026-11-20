using AbstractPixel.Core;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Which copy of a save file a <see cref="SaveStorage"/> call means: the open game profile's, another game
    /// profile's, and for a Scene save file, which scene's. It remembers what the caller passed, so a scene given for
    /// a file that isn't a Scene file can be reported instead of quietly ignored.
    /// </summary>
    internal readonly struct SaveFileTarget
    {
        /// <summary>The game profile the caller passed, or null to use the open one.</summary>
        public GameProfile GameProfile { get; }

        public bool NamesGameProfile { get; }

        /// <summary>The scene the caller passed. It can be null, which is reported as a mistake.</summary>
        public SceneField Scene { get; }

        public bool NamesScene { get; }

        SaveFileTarget(GameProfile gameProfile, bool namesGameProfile, SceneField scene, bool namesScene)
        {
            GameProfile = gameProfile;
            NamesGameProfile = namesGameProfile;
            Scene = scene;
            NamesScene = namesScene;
        }

        /// <summary>A Global file, or the open game profile's copy of a GameProfile file.</summary>
        public static SaveFileTarget OpenGameProfile => new SaveFileTarget(null, false, null, false);

        /// <summary>A GameProfile file of the given game profile, open or not.</summary>
        public static SaveFileTarget Of(GameProfile gameProfile) => new SaveFileTarget(gameProfile, true, null, false);

        /// <summary>A Scene file for one scene of the open game profile.</summary>
        public static SaveFileTarget In(SceneField scene) => new SaveFileTarget(null, false, scene, true);

        /// <summary>A Scene file for one scene of the given game profile.</summary>
        public static SaveFileTarget In(SceneField scene, GameProfile gameProfile) => new SaveFileTarget(gameProfile, true, scene, true);
    }
}
