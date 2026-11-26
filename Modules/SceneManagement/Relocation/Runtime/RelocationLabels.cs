using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>What a relocation label means when it may be "No label" (null): its name, colour and steepest ground.</summary>
    public static class RelocationLabels
    {
        public const string NoLabelName = "No label";
        public const float DefaultSteepestGround = 35f;

        private static readonly Color NoLabelColour = new Color(0.85f, 0.85f, 0.85f, 1f);

        /// <summary>True when two label fields name the same label.</summary>
        /// <remarks>
        /// - Both "No label", the same asset, or two copies of one asset count as the same.
        /// - An Addressables bundle carries its own copy of a label its scenes use; the copies share the label's ID.
        /// </remarks>
        public static bool Same(RelocationLabel _first, RelocationLabel _second)
        {
            return KeyOf(_first) == KeyOf(_second);
        }

        /// <summary>The key labels match by: the label's ID, or its instance when it has none; "" for No label.</summary>
        public static string KeyOf(RelocationLabel _label)
        {
            if (_label == null)
            {
                return string.Empty;
            }
            if (!string.IsNullOrEmpty(_label.Id))
            {
                return _label.Id;
            }
            return "instance:" + AbstractPixel.Core.UnityObjectIds.Of(_label);
        }

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
