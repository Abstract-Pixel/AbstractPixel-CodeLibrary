using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Relocation section of the Scene System page: the Ground layers, used by Wait for ground below and by adding
    /// spawn point positions with Shift+click, and the Steepest ground for relocatables with No label.
    /// </summary>
    internal static class SceneSystemRelocationSection
    {
        public const string Title = "Relocation";
        public const string Tooltip = "How relocatables find ground. Relocatables move to spawn point positions on every scene change.";
        private const string GroundLayersTooltip = "Ground layers: the physics layers that count as ground, for Wait for ground below and for adding spawn point positions with Shift+click. Triggers never count.";
        private const string SteepestGroundTooltip = "Steepest ground, in degrees, for relocatables and spawn points with No label. A relocation label sets its own. A steeper position is allowed but shown as a warning.";

        public static void Fill(VisualElement _card, SceneSystemSettings _settings)
        {
            _card.Clear();
            if (_settings == null)
            {
                _card.Add(SceneSystemUI.Hint("Available once the scene system is set up."));
                return;
            }

            LayerMaskField groundLayers = new LayerMaskField("Ground layers", _settings.GroundLayers.value);
            groundLayers.tooltip = GroundLayersTooltip;
            groundLayers.RegisterValueChangedCallback(_change =>
            {
                _settings.GroundLayers = _change.newValue;
                SceneSystemSettingsAssets.Save(_settings);
            });
            _card.Add(groundLayers);

            Slider steepestGround = new Slider("Steepest ground, No label (°)", 0f, 90f);
            steepestGround.showInputField = true;
            steepestGround.SetValueWithoutNotify(_settings.NoLabelSteepestGround);
            steepestGround.tooltip = SteepestGroundTooltip;
            steepestGround.RegisterValueChangedCallback(_change =>
            {
                _settings.NoLabelSteepestGround = _change.newValue;
                SceneSystemSettingsAssets.Save(_settings);
            });
            _card.Add(steepestGround);
        }
    }
}
