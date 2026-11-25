namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Decides a scene's status, the one fix the scene field offers, and the message it shows. Pure rules with no
    /// project access, so they can be tested without assets; &lt;see cref="SceneStatusReader"/&gt; reads the facts.
    /// </summary>
    public static class SceneStatusRules
    {
        public static SceneStatus Decide(SceneStatusFacts _facts)
        {
            if (!_facts.HasSceneAsset)
            {
                if (_facts.HasStoredScene)
                {
                    return SceneStatus.Missing;
                }
                return SceneStatus.NoScene;
            }

            // A scene is In build or Addressable, never both; Build Settings wins if a project has both.
            if (_facts.IsInBuild)
            {
                return SceneStatus.InBuild;
            }
            if (_facts.IsAddressable)
            {
                return SceneStatus.Addressable;
            }
            if (_facts.IsNeverInBuild)
            {
                return SceneStatus.NeverInBuild;
            }
            return SceneStatus.NotInBuild;
        }

        /// <summary>The one fix for a scene that is Not in build: the project default place, or Build Settings when Addressables is not installed.</summary>
        public static SceneFix FixFor(SceneStatus _status, ScenePlace _defaultPlace, bool _addressablesInstalled)
        {
            if (_status != SceneStatus.NotInBuild)
            {
                return SceneFix.None;
            }

            bool goesToAddressables = _defaultPlace == ScenePlace.Addressables && _addressablesInstalled;
            if (goesToAddressables)
            {
                return SceneFix.MakeAddressable;
            }
            return SceneFix.AddToBuild;
        }

        public static string FixLabel(SceneFix _fix)
        {
            switch (_fix)
            {
                case SceneFix.AddToBuild:
                    return "Add to build";
                case SceneFix.MakeAddressable:
                    return "Make Addressable";
                default:
                    return string.Empty;
            }
        }

        /// <summary>The scene field's one status line, in rich text, naming the scene.</summary>
        public static string Message(SceneStatus _status, string _sceneName, string _scenePath, bool _addressablesInstalled)
        {
            string scene = "Scene <b>'" + _sceneName + "'</b>";
            switch (_status)
            {
                case SceneStatus.InBuild:
                    return scene + " is in build (Build Settings).";
                case SceneStatus.Addressable:
                    return scene + " is Addressable.";
                case SceneStatus.NotInBuild:
                    return NotInBuildMessage(scene, _addressablesInstalled);
                case SceneStatus.Missing:
                    return MissingMessage(scene, _scenePath);
                case SceneStatus.NeverInBuild:
                    return scene + " never goes into a build: it is a sample scene. It opens in the Editor only.";
                default:
                    return "No scene assigned.";
            }
        }

        private static string NotInBuildMessage(string _scene, bool _addressablesInstalled)
        {
            if (_addressablesInstalled)
            {
                return _scene + " is not in build: it is in neither Build Settings nor Addressables, so it can't load.";
            }
            return _scene + " is not in build: it is not in Build Settings, so it can't load.";
        }

        private static string MissingMessage(string _scene, string _scenePath)
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                return _scene + " is missing: only its name is stored. Pick the scene again.";
            }
            return _scene + " is missing: there is no scene asset at '" + _scenePath + "'. Pick the scene again.";
        }
    }
}
