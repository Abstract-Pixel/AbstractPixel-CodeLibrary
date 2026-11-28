using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>The overlay's UI assets. A build holds them only when it includes the debugger: the build step preloads this.</summary>
    internal class DebuggerOverlayAssets : ScriptableObject
    {
        public PanelSettings Panel;
        public StyleSheet Style;
    }
}
