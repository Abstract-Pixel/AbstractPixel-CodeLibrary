using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What "Open the group with its main scene" opens and closes after a scene was opened on its own. It opens the
    /// needed scenes that are not open, and closes only scenes it opened itself that the new group does not need: a
    /// scene the user opened by hand is never closed. Plain data, no Unity scene calls.
    /// </summary>
    public class OpenedGroupScenesPlan
    {
        public List<SceneReference> ToOpen { get; } = new List<SceneReference>();
        public List<SceneReference> ToClose { get; } = new List<SceneReference>();

        /// <summary>The scenes the tool has opened once the plan is carried out, for the next switch.</summary>
        public List<SceneReference> OpenedByToolAfter { get; } = new List<SceneReference>();

        /// <param name="_needed">What Play would load next to the opened scene (<see cref="GroupScenesToOpen"/>).</param>
        /// <param name="_open">The scenes open in the Hierarchy now.</param>
        /// <param name="_openedByTool">The scenes the tool remembers having opened.</param>
        public static OpenedGroupScenesPlan Compute(List<SceneReference> _needed, List<SceneReference> _open, List<SceneReference> _openedByTool)
        {
            OpenedGroupScenesPlan plan = new OpenedGroupScenesPlan();
            foreach (SceneReference openedByTool in _openedByTool)
            {
                bool isStillOpen = StartUpSceneMatch.ListHolds(_open, openedByTool);
                if (!isStillOpen)
                {
                    continue;
                }
                bool isNeeded = StartUpSceneMatch.ListHolds(_needed, openedByTool);
                if (isNeeded)
                {
                    plan.OpenedByToolAfter.Add(openedByTool);
                    continue;
                }
                plan.ToClose.Add(openedByTool);
            }

            foreach (SceneReference neededScene in _needed)
            {
                bool isOpen = StartUpSceneMatch.ListHolds(_open, neededScene);
                if (isOpen)
                {
                    continue;
                }
                plan.ToOpen.Add(neededScene);
                plan.OpenedByToolAfter.Add(neededScene);
            }
            return plan;
        }
    }
}
