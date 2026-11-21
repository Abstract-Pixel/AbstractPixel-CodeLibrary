using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>What a relocation label means when it may be "No label" (null): its name, colour and steepest ground.</summary>
    public static class RelocationLabels
    {
        public const string NoLabelName = "No label";
        public const float DefaultSteepestGround = 35f;

        private static readonly Color NoLabelColour = new Color(0.85f, 0.85f, 0.85f, 1f);

        public static string NameOf(RelocationLabel _label)
        {
            if (_label == null)
            {
                return NoLabelName;
            }
            return _label.name;
        }

        public static Color ColourOf(RelocationLabel _label)
        {
            if (_label == null)
            {
                return NoLabelColour;
            }
            return _label.Colour;
        }

        /// <summary>The label's Steepest ground; for "No label", the project-wide value on the Scene System page.</summary>
        public static float SteepestGroundOf(RelocationLabel _label)
        {
            if (_label != null)
            {
                return _label.SteepestGround;
            }
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            if (settings == null)
            {
                return DefaultSteepestGround;
            }
            return settings.NoLabelSteepestGround;
        }

        /// <summary>The Ground layers from the Scene System page; everything except Ignore Raycast when there are no settings.</summary>
        public static int GroundLayers()
        {
            SceneSystemSettings settings = SceneSystemSettingsSource.Find();
            if (settings == null)
            {
                return ~(1 << 2);
            }
            return settings.GroundLayers.value;
        }
    }
}
