using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Adds the relocation setup problems to the group check, for a scene group whose main scene is open and active in
    /// the Editor (the check cannot see inside closed scenes). Each problem selects its object. They are warnings:
    /// they never stop a build.
    /// </summary>
    internal static class SceneSystemCheckHook
    {
        public static void Register()
        {
            SceneGroupCheck.AddCheck(Find);
        }

        public static List<SceneGroupProblem> Find(SceneGroup _group)
        {
            List<SceneGroupProblem> problems = new List<SceneGroupProblem>();
            if (_group == null || _group.MainScene == null || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return problems;
            }
            Scene active = SceneManager.GetActiveScene();
            bool mainSceneIsActive = active.IsValid() && active.path == _group.MainScene.ScenePath;
            if (!mainSceneIsActive)
            {
                return problems;
            }
            foreach (RelocationEditorModel.Problem problem in RelocationEditorModel.Current.Problems)
            {
                string message = "Scene group '" + _group.name + "': " + problem.Message;
                problems.Add(SceneGroupProblem.Warning(SceneGroupProblemKind.RelocationSetup, problem.Target, message));
            }
            return problems;
        }
    }
}
