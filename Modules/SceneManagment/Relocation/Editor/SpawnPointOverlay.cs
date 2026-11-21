using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A small Scene view Overlay while a spawn point is selected, in the spirit of ProBuilder's: icon buttons for the
    /// selected positions (each tooltip names the action and its key) and, when floating, the current mode in one short
    /// line. Docked to any edge it is just the icon toolbar, so it never takes much of the Scene view or overflows. The
    /// full numbered key list is in the spawn point inspector.
    /// </summary>
    [Overlay(typeof(SceneView), "Spawn point", true)]
    internal sealed class SpawnPointOverlay : Overlay, ITransientOverlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
    {
        public bool visible
        {
            get
            {
                GameObject active = Selection.activeGameObject;
                return active != null && active.GetComponent<SpawnPoint>() != null;
            }
        }

        public override VisualElement CreatePanelContent()
        {
            VisualElement panel = new VisualElement();
            panel.style.minWidth = 132;
            panel.style.maxWidth = 220;
            panel.style.paddingLeft = 3;
            panel.style.paddingRight = 3;
            panel.style.paddingBottom = 3;

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.flexWrap = Wrap.Wrap;
            AddActionButtons(buttons);
            panel.Add(buttons);

            Label mode = new Label();
            mode.style.whiteSpace = WhiteSpace.Normal;
            mode.style.fontSize = 10;
            mode.style.marginTop = 2;
            mode.style.color = SceneSystemUI.DefaultAccent;
            panel.Add(mode);
            panel.schedule.Execute(() => mode.text = SpawnPointShortcuts.ShortModeLine(SpawnPointToolState.Mode)).Every(150);
            return panel;
        }

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            OverlayToolbar toolbar = new OverlayToolbar();
            AddActionButtons(toolbar);
            return toolbar;
        }

        public OverlayToolbar CreateVerticalToolbarContent()
        {
            OverlayToolbar toolbar = new OverlayToolbar();
            AddActionButtons(toolbar);
            return toolbar;
        }

        // The four actions on the selected positions, as icons; greyed out while no position is selected.
        private static void AddActionButtons(VisualElement _parent)
        {
            _parent.Add(ActionButton("TreeEditor.Duplicate", "Duplicate the selected positions (" + SpawnPointShortcuts.KeyFor(SpawnPointShortcuts.DuplicateId) + ")", SpawnPointEditing.DuplicateSelected));
            _parent.Add(ActionButton("TreeEditor.Trash", "Remove the selected positions (" + SpawnPointShortcuts.KeyFor(SpawnPointShortcuts.RemoveId) + ")", SpawnPointEditing.RemoveSelected));
            _parent.Add(ActionButton("ViewToolZoom", "Frame the selected positions (" + SpawnPointShortcuts.KeyFor(SpawnPointShortcuts.FrameId) + ")", SpawnPointEditing.FrameSelected));
            _parent.Add(ActionButton("Toolbar Minus", "Clear the position selection (Esc)", SpawnPointPositionSelection.Clear));
        }

        private static VisualElement ActionButton(string _iconName, string _tooltip, System.Action _clicked)
        {
            EditorToolbarButton button = new EditorToolbarButton(string.Empty, EditorGUIUtility.FindTexture(_iconName), _clicked);
            button.tooltip = _tooltip;
            button.schedule.Execute(() => button.SetEnabled(SpawnPointPositionSelection.HasAny)).Every(150);
            return button;
        }
    }
}
