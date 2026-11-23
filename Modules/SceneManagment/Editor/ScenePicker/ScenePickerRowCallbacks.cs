using System;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What a row of the scene picker calls when its parts are clicked; the popup supplies them.</summary>
    public sealed class ScenePickerRowCallbacks
    {
        /// <summary>A click on the row itself: opens the scene or group.</summary>
        public Action<ScenePickerRow> Opened;

        /// <summary>The folder button: selects the asset in the Project window.</summary>
        public Action<ScenePickerRow> FolderClicked;

        /// <summary>The label button: selects the scene group or scene set it names.</summary>
        public Action<ScenePickerRow> LabelClicked;

        /// <summary>The status button: see <see cref="ScenePickerStatusRules"/>.</summary>
        public Action<ScenePickerRow> StatusClicked;

        /// <summary>Fills the right-click menu of a row.</summary>
        public Action<ScenePickerRow, DropdownMenu> MenuOpening;
    }
}
