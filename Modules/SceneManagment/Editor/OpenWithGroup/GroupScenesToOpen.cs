using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Which scenes "Open the group with its main scene" opens next to a scene opened on its own: what Play would
    /// load from that scene, by the same rule start-up uses (<see cref="StartUpRule"/>).
    /// <list type="bullet">
    /// <item>A group's main scene: its manager and shared scenes.</item>
    /// <item>A manager or shared scene: the start scene group's main, manager and shared scenes.</item>
    /// <item>A manager or shared scene of a sample scene group: that sample scene group's main, manager and shared scenes.</item>
    /// <item>A scene in no group: the default scene set's manager and shared scenes.</item>
    /// <item>The launch scene: nothing, because a build opens only the launch scene.</item>
    /// </list>
    /// The opened scene itself is never in the list.
    /// </summary>
    public static class GroupScenesToOpen
    {
        public static List<SceneReference> For(SceneSystemSettings _settings, SceneReference _openedScene)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            if (_settings == null || _openedScene == null)
            {
                return scenes;
            }

            List<SceneReference> openScenes = new List<SceneReference> { _openedScene };
            StartUpChoice choice = StartUpRule.Choose(_settings, openScenes);
            switch (choice.Kind)
            {
                case StartUpChoiceKind.GroupOfOpenMainScene:
                    AddGroupScenes(scenes, choice.Group, false);
                    break;
                case StartUpChoiceKind.StartSceneGroup:
                case StartUpChoiceKind.SampleSceneGroupOfOpenScene:
                    AddGroupScenes(scenes, choice.Group, true);
                    break;
                case StartUpChoiceKind.TemporaryGroup:
                    AddSetScenes(scenes, choice.SceneSet);
                    break;
                default:
                    // The launch scene, or nothing start-up could do.
                    break;
            }

            RemoveScene(scenes, _openedScene);
            return scenes;
        }

        private static void AddGroupScenes(List<SceneReference> _scenes, SceneGroup _group, bool _withMainScene)
        {
            if (_group == null)
            {
                return;
            }
            if (_withMainScene)
            {
                AddOnce(_scenes, _group.MainScene);
            }
            AddAllOnce(_scenes, _group.ManagerialBootScenesList);
            AddAllOnce(_scenes, _group.ContextualBootScenesList);
        }

        private static void AddSetScenes(List<SceneReference> _scenes, SceneSet _set)
        {
            if (_set == null)
            {
                return;
            }
            AddAllOnce(_scenes, _set.ManagerScenes);
            AddAllOnce(_scenes, _set.SharedScenes);
        }

        private static void AddAllOnce(List<SceneReference> _scenes, List<SceneReference> _toAdd)
        {
            if (_toAdd == null)
            {
                return;
            }
            foreach (SceneReference scene in _toAdd)
            {
                AddOnce(_scenes, scene);
            }
        }

        private static void AddOnce(List<SceneReference> _scenes, SceneReference _scene)
        {
            bool isEmpty = _scene == null || string.IsNullOrEmpty(_scene.SceneName);
            if (isEmpty)
            {
                return;
            }
            if (StartUpSceneMatch.ListHolds(_scenes, _scene))
            {
                return;
            }
            _scenes.Add(_scene);
        }

        private static void RemoveScene(List<SceneReference> _scenes, SceneReference _scene)
        {
            for (int index = _scenes.Count - 1; index >= 0; index--)
            {
                if (StartUpSceneMatch.IsSameScene(_scenes[index], _scene))
                {
                    _scenes.RemoveAt(index);
                }
            }
        }
    }
}
