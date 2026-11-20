using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>The colour of the scene field's status square for each scene status.</summary>
    public static class SceneStatusColours
    {
        public static readonly Color Outline = new Color(0.2f, 0.2f, 0.2f, 1f);

        public static Color For(SceneStatus _status)
        {
            switch (_status)
            {
                case SceneStatus.InBuild:
                    return new Color(0f, 1f, 0f, 0.5f);
                case SceneStatus.Addressable:
                    return new Color(0.2f, 0.6f, 1f, 0.7f);
                default:
                    return new Color(1f, 0f, 0f, 0.5f);
            }
        }
    }
}
