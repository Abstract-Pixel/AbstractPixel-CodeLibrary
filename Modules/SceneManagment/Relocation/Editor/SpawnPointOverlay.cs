using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>A small Scene view Overlay that repeats the spawn point keys while a spawn point is selected.</summary>
    [Overlay(typeof(SceneView), "Spawn point keys", true)]
    internal sealed class SpawnPointOverlay : Overlay, ITransientOverlay
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
            panel.style.paddingLeft = 6;
            panel.style.paddingRight = 6;
            panel.style.paddingBottom = 4;
            panel.style.width = 290;
            panel.Add(RelocationShortcutList.Create(118f));
            return panel;
        }
    }
}
