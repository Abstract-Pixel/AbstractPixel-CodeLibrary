using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Which scene group start-up opens, decided from the settings and the scenes Unity opened (session 4B, decision 2).
    /// One rule for builds and Play: a build is the case where only build index 0 is open.
    /// <list type="number">
    /// <item>The launch scene is open: the start scene group, behind the launch scene.</item>
    /// <item>An open scene, the active one first, is the main scene of a scene group in the settings list: that group.
    /// Two such scenes open: an error, and the first one wins.</item>
    /// <item>The active scene is a manager or shared scene: the start scene group, with one line saying why.</item>
    /// <item>The active scene is in no scene group: a temporary scene group built from the default scene set, with one
    /// warning.</item>
    /// </list>
    /// Plain data in and out: it never calls Unity's scene API.
    /// </summary>
    public static class StartUpRule
    {
        private const string LogPrefix = "[Scene system] Start-up: ";

        /// <param name="_settings">The scene system settings, or null when the project has none.</param>
        /// <param name="_openScenesActiveFirst">The scenes Unity opened, the active scene first.</param>
        public static StartUpChoice Choose(SceneSystemSettings _settings, List<SceneReference> _openScenesActiveFirst)
        {
            StartUpChoice choice = new StartUpChoice();
            choice.Kind = StartUpChoiceKind.DoNothing;
            if (_settings == null)
            {
                choice.ErrorMessage = LogPrefix + "no scene system settings asset was found, so start-up did nothing. Create one on the Scene System page (Project Settings > Abstract Pixel > Scene System).";
                return choice;
            }
            if (_openScenesActiveFirst == null || _openScenesActiveFirst.Count == 0)
            {
                return choice;
            }

            bool launchSceneIsOpen = ShouldHoldLaunchScene(_settings, _openScenesActiveFirst);
            if (launchSceneIsOpen)
            {
                return ChooseStartSceneGroup(_settings, StartUpChoiceKind.BehindLaunchScene, _settings.LaunchScene);
            }

            bool foundMainScene = TryChooseGroupOfOpenMainScene(_settings, _openScenesActiveFirst, choice);
            if (foundMainScene)
            {
                return choice;
            }

            SceneReference activeScene = _openScenesActiveFirst[0];
            if (IsInASceneSet(_settings, activeScene))
            {
                StartUpChoice startGroupChoice = ChooseStartSceneGroup(_settings, StartUpChoiceKind.StartSceneGroup, activeScene);
                if (startGroupChoice.Group != null)
                {
                    startGroupChoice.InfoMessage = LogPrefix + "'" + activeScene.SceneName + "' is a manager or shared scene and no main scene is open, so start-up opens the start scene group '" + startGroupChoice.Group.name + "', as a build would.";
                }
                return startGroupChoice;
            }

            return ChooseTemporaryGroup(_settings, activeScene);
        }

        /// <summary>
        /// Whether start-up holds the launch scene hold: only when the settings name a launch scene and it is among the
        /// scenes Unity is opening. Play from a level or the start scene group never holds.
        /// </summary>
        public static bool ShouldHoldLaunchScene(SceneSystemSettings _settings, List<SceneReference> _openScenes)
        {
            if (_settings == null || _settings.HasLaunchScene == false)
            {
                return false;
            }
            if (_openScenes == null || _openScenes.Count == 0)
            {
                return false;
            }
            return StartUpSceneMatch.ListHolds(_openScenes, _settings.LaunchScene);
        }

        private static StartUpChoice ChooseStartSceneGroup(SceneSystemSettings _settings, StartUpChoiceKind _kind, SceneReference _reasonScene)
        {
            StartUpChoice choice = new StartUpChoice();
            SceneGroup startGroup = _settings.StartSceneGroup;
            if (startGroup == null)
            {
                choice.Kind = StartUpChoiceKind.DoNothing;
                choice.ErrorMessage = LogPrefix + "'" + _reasonScene.SceneName + "' needs the start scene group, but none is set, so start-up did nothing. Set the start scene group on the Scene System page (Project Settings > Abstract Pixel > Scene System).";
                return choice;
            }

            choice.Kind = _kind;
            choice.Group = startGroup;
            choice.MainScene = startGroup.MainScene;
            return choice;
        }

        // Fills the choice and returns true when an open scene is the main scene of a scene group in the settings list.
        private static bool TryChooseGroupOfOpenMainScene(SceneSystemSettings _settings, List<SceneReference> _openScenes, StartUpChoice _choice)
        {
            SceneGroup pickedGroup = null;
            SceneReference pickedScene = null;
            List<string> otherMainScenes = new List<string>();
            foreach (SceneReference openScene in _openScenes)
            {
                SceneGroup group = FindGroupWithMainScene(_settings, openScene);
                if (group == null)
                {
                    continue;
                }
                if (pickedGroup == null)
                {
                    pickedGroup = group;
                    pickedScene = openScene;
                    continue;
                }
                if (group != pickedGroup)
                {
                    otherMainScenes.Add("'" + openScene.SceneName + "' (" + group.name + ")");
                }
            }

            if (pickedGroup == null)
            {
                return false;
            }

            _choice.Kind = StartUpChoiceKind.GroupOfOpenMainScene;
            _choice.Group = pickedGroup;
            _choice.MainScene = pickedScene;
            if (otherMainScenes.Count > 0)
            {
                bool pickedIsActive = StartUpSceneMatch.IsSameScene(_openScenes[0], pickedScene);
                string which = pickedIsActive ? "the active scene" : "the first one Unity lists";
                _choice.ErrorMessage = LogPrefix + "more than one main scene is open: '" + pickedScene.SceneName + "' (" + pickedGroup.name + ") and " + string.Join(", ", otherMainScenes) + ". Start-up opens '" + pickedGroup.name + "', " + which + ". Close the other main scenes before Play.";
            }
            return true;
        }

        private static SceneGroup FindGroupWithMainScene(SceneSystemSettings _settings, SceneReference _scene)
        {
            foreach (SceneGroup group in _settings.SceneGroups)
            {
                if (group == null || group.MainScene == null)
                {
                    continue;
                }
                if (StartUpSceneMatch.IsSameScene(group.MainScene, _scene))
                {
                    return group;
                }
            }
            return null;
        }

        // A manager or shared scene of any scene set, or listed as one by any scene group (a group from before scene sets).
        private static bool IsInASceneSet(SceneSystemSettings _settings, SceneReference _scene)
        {
            List<SceneSet> sets = new List<SceneSet>(_settings.SceneSets);
            if (_settings.DefaultSceneSet != null)
            {
                sets.Add(_settings.DefaultSceneSet);
            }
            foreach (SceneSet set in sets)
            {
                if (set == null)
                {
                    continue;
                }
                if (StartUpSceneMatch.ListHolds(set.ManagerScenes, _scene) || StartUpSceneMatch.ListHolds(set.SharedScenes, _scene))
                {
                    return true;
                }
            }
            foreach (SceneGroup group in _settings.SceneGroups)
            {
                if (group == null)
                {
                    continue;
                }
                if (StartUpSceneMatch.ListHolds(group.ManagerialBootScenesList, _scene) || StartUpSceneMatch.ListHolds(group.ContextualBootScenesList, _scene))
                {
                    return true;
                }
            }
            return false;
        }

        private static StartUpChoice ChooseTemporaryGroup(SceneSystemSettings _settings, SceneReference _activeScene)
        {
            StartUpChoice choice = new StartUpChoice();
            choice.Kind = StartUpChoiceKind.TemporaryGroup;
            choice.MainScene = _activeScene;
            choice.SceneSet = _settings.DefaultSceneSet;

            string setText = "no scene set (the project has no default scene set)";
            if (choice.SceneSet != null)
            {
                setText = "the default scene set '" + choice.SceneSet.name + "'";
            }
            choice.WarningMessage = LogPrefix + "'" + _activeScene.SceneName + "' is in no scene group, so start-up opens it in a temporary scene group with " + setText + ". To give it its own, use Create Scene Group: right-click the scene in the Project window, then Create > Scene Group.";
            return choice;
        }
    }
}
