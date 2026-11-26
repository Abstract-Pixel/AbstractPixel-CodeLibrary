using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The words setup explains, in glossary wording (session 5A2, decision 15): one plain sentence of 8 to 15 words
    /// each. A step's "Terminology" lines explain the words it brings in, at most three; a later step that uses a word
    /// again carries its line as a tooltip.
    /// </summary>
    public static class SceneSystemTerms
    {
        public const string Addressables = "Addressables";
        public const string SceneGroup = "Scene group";
        public const string StartSceneGroup = "Start scene group";
        public const string LaunchScene = "Launch scene";
        public const string LoadingScene = "Loading scene";
        public const string SceneChange = "Scene change";
        public const string SceneSet = "Scene set";
        public const string ManagerScene = "Manager scene";
        public const string SharedScene = "Shared scene";
        public const string MainScene = "Main scene";
        public const string SceneInNoGroup = "Scene in no group";

        public const int MostWordsPerStep = 3;
        public const int FewestWordsInDefinition = 8;
        public const int MostWordsInDefinition = 15;

        private static readonly Dictionary<string, string> Definitions = new Dictionary<string, string>
        {
            { Addressables, "Unity's package that loads scenes on demand instead of keeping them in Build Settings." },
            { SceneGroup, "One place in your game, like a level or the main menu, and its scenes." },
            { StartSceneGroup, "The scene group the game opens when it starts." },
            { LaunchScene, "An optional scene, like a splash, that opens before the start scene group." },
            { LoadingScene, "A scene shown while the old place closes and the new one opens." },
            { SceneChange, "Leaving the current scene group and arriving in another one, or the same one." },
            { SceneSet, "The manager and shared scenes that come with every scene group using it." },
            { ManagerScene, "A scene that opens once and stays open, such as audio and save managers." },
            { SharedScene, "A scene several scene groups open next to their main scene, such as the player." },
            { MainScene, "The one scene in a scene group that makes it that place, like Level 7." },
            { SceneInNoGroup, "A scene that is no scene group's main scene and in no scene set." }
        };

        /// <summary>The words each setup step explains, in step order (step 1 is the welcome page).</summary>
        public static readonly string[][] WordsPerStep =
        {
            new string[0],
            new string[0],
            new[] { Addressables },
            new[] { SceneGroup, StartSceneGroup, LaunchScene },
            new[] { LoadingScene, SceneChange },
            new[] { SceneSet, ManagerScene, SharedScene },
            new[] { MainScene, SceneInNoGroup }
        };

        public static IEnumerable<string> AllWords
        {
            get { return Definitions.Keys; }
        }

        public static string Definition(string _word)
        {
            string definition;
            if (_word != null && Definitions.TryGetValue(_word, out definition))
            {
                return definition;
            }
            return string.Empty;
        }

        /// <summary>"Scene group: One place in your game…", one line per word, for a tooltip on a later step.</summary>
        public static string Tooltip(params string[] _words)
        {
            List<string> lines = new List<string>();
            foreach (string word in _words)
            {
                string definition = Definition(word);
                if (definition.Length > 0)
                {
                    lines.Add(word + ": " + definition);
                }
            }
            return string.Join("\n", lines);
        }

        /// <summary>A tooltip with the option's own text first, then the words it uses that earlier steps explained.</summary>
        public static string WithTerms(string _tooltip, params string[] _words)
        {
            string terms = Tooltip(_words);
            if (terms.Length == 0)
            {
                return _tooltip;
            }
            if (string.IsNullOrEmpty(_tooltip))
            {
                return terms;
            }
            return _tooltip + "\n\n" + terms;
        }

        /// <summary>How many words a definition has (split on spaces), for the 8 to 15 word rule.</summary>
        public static int WordCount(string _sentence)
        {
            if (string.IsNullOrEmpty(_sentence))
            {
                return 0;
            }
            return _sentence.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
