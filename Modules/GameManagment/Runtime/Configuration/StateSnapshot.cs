using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public struct StateSnapshot
    {
        public float PreviousTimeScale;
        public bool PreviousCursorVisibility;
        public CursorLockMode PreviousCursorLockMode;
    }
}
