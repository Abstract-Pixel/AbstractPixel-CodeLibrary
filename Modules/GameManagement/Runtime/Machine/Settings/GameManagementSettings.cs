using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>The one game management settings asset per project.</summary>
    /// <remarks>
    /// - It can live in any folder; the Editor finds it by type and keeps it in Player Settings' Preloaded Assets,
    ///   the same method as the scene system.
    /// - Edited in Project Settings &gt; Abstract Pixel &gt; Game Management (Session 6).
    /// </remarks>
    public class GameManagementSettings : ScriptableObject
    {
        public const string DefaultFolder = "Assets/GameManagement";

        [Tooltip("The ordered list of every state.")]
        public GameStateCatalog Catalog;

        [Tooltip("Time scale while no state is active. Keep last is the default: a scene change then cannot let loaded shared scenes run early.")]
        public NoStateTimeScale NoStateTimeScale = NoStateTimeScale.KeepLast;

        [Tooltip("How many transitions the journal keeps for the debugger.")]
        [Min(8)] public int JournalSize = 128;

        [Tooltip("Which input actions states switch on and off. None leaves input alone.")]
        public InputSourceKind InputSource = InputSourceKind.None;

        [Tooltip("The asset used when the input source is Chosen Asset.")]
        public UnityEngine.InputSystem.InputActionAsset InputActions;
    }
}
