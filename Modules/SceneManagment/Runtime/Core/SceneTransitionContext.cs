using AbstractPixel.Core;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement
{
    internal class SceneTransitionContext
    {
        public HashSet<SceneReference> ContextualToLoad = new HashSet<SceneReference>();
        public HashSet<SceneReference> ContextualToUnload = new HashSet<SceneReference>();
        public HashSet<SceneReference> ManagerialToLoad = new HashSet<SceneReference>();
        public SceneGroup sceneGroupToTransitionTo;
        public bool doImmediateSceneActivation = true;
        public SceneTransitionContext(SceneCoordinator orchestrator, SceneGroup newSceneGroup,bool immediateSceneActivation = true)
        {
            sceneGroupToTransitionTo = newSceneGroup;
            doImmediateSceneActivation = immediateSceneActivation;
            ContextualToUnload = new HashSet<SceneReference>(orchestrator.activeContextualScenesSet);
            ContextualToUnload.ExceptWith(newSceneGroup.ContextualBootScenesList);

            ContextualToLoad = new HashSet<SceneReference>(newSceneGroup.ContextualBootScenesList);
            ContextualToLoad.ExceptWith(orchestrator.activeContextualScenesSet);

            ManagerialToLoad = new HashSet<SceneReference>(newSceneGroup.ManagerialBootScenesList);
            ManagerialToLoad.ExceptWith(orchestrator.activeManagerialScenesSet);
        }


        public void GetTransitionContext(out HashSet<SceneReference> contextualToUnload, out HashSet<SceneReference> contextualToLoad, out HashSet<SceneReference> managerialToLoad)
        {
            contextualToUnload = ContextualToUnload;
            contextualToLoad = ContextualToLoad;
            managerialToLoad = ManagerialToLoad;
        }
    }
}