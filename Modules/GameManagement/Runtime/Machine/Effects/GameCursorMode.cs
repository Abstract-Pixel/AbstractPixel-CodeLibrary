using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>The cursor while a state is on top. Replaces the old two cursor bools.</summary>
    public enum GameCursorMode
    {
        Free,
        LockedHidden,
        LockedVisible,
        FreeHidden
    }
}
