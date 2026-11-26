using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The one thing the scene system asks to hold, relocate and release every relocatable during a scene change (ADR 0012).</summary>
    /// <remarks>
    /// - The relocation sub-module provides it and registers it with <see cref="SceneActions.RegisterRelocationBroadcaster"/>.
    /// - With none registered, scenes change without relocation.
    /// - The coordinator calls it in every scene change, start-up and the fallback scene group included, once per step.
    /// </remarks>
    public interface IRelocationBroadcaster
    {
        /// <summary>Hold: called after the change is accepted, before Hide screen. Never called for a refused change.</summary>
        void Hold();

        /// <summary>Relocate: called once every scene of the group is open and its main scene is active.</summary>
        /// <remarks>
        /// - Called before "new level" is raised, before the loading scene leaves and before the last Show screen.
        /// - The scene change waits for it, so the screen stays covered until relocation is safe.
        /// </remarks>
        Task Relocate();

        /// <summary>Release: called once the last Show screen reports that the screen counts as visible.</summary>
        /// <remarks>
        /// - If Show screen never reports, it is called at its end.
        /// - With no screen effect it is called straight after Relocate.
        /// - In start-up it is called after the launch scene hold.
        /// </remarks>
        void Release();
    }
}
