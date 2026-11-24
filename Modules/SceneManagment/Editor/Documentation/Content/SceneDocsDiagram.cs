using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The scene documentation's graphs. Parts (Home): how the parts of the scene system feed each other; each box
    /// opens its guide page. Code map: the folders and how they depend on each other; each folder opens its own graph,
    /// whose arrows are the references between its scripts, and a script opens in your code editor.
    /// </summary>
    public static class SceneDocsDiagram
    {
        public static DocsGraphView Parts(Action<string> _open)
        {
            DocsGraph graph = new DocsGraph("Scene system");
            DocsGraph setUp = new DocsGraph("Set up");
            Part(setUp, "groups", "Scene groups and sets", "What opens together", "guide/scene-groups-and-scene-sets", _open).Layer = 1;
            Part(setUp, "build", "Build and Addressables", "Where scenes load from", "guide/build-settings-and-addressables", _open).Layer = 1;
            Part(setUp, "editor", "Editor tools", "Picker, checks, setup", "guide/editor-tools", _open).Layer = 0;
            setUp.Link("editor", "groups");
            setUp.Link("editor", "build");

            DocsGraph change = new DocsGraph("Change scene");
            Part(change, "startup", "Start-up", "The first scene change", "guide/start-up", _open).Layer = 0;
            Part(change, "preload", "Preload", "Load the next one early", "guide/preload", _open).Layer = 0;
            DocsGraphNode changeNode = Part(change, "change", "Change scene", "Go to a scene group", "guide/change-scene", _open);
            changeNode.Layer = 1;
            changeNode.IsKey = true;
            Part(change, "failed", "When it fails", "Fallback scene group", "guide/when-a-scene-change-fails", _open).Layer = 2;
            Part(change, "messages", "Scene messages", "What fires, and when", "guide/scene-messages", _open).Layer = 2;
            change.Link("startup", "change");
            change.Link("preload", "change");
            change.Link("change", "failed");
            change.Link("change", "messages");

            DocsGraph during = new DocsGraph("During the change");
            Part(during, "screen", "Screen and transitions", "Hide and show the screen", "guide/screen-and-transitions", _open).Layer = 0;
            Part(during, "loading", "Loading scene", "Shown while scenes load", "guide/loading-scene", _open).Layer = 1;
            Part(during, "relocation", "Relocation", "Objects to spawn points", "guide/relocation", _open).Layer = 1;
            during.Link("screen", "loading");
            during.Link("screen", "relocation");

            Group(graph, "setup", "Set up", "Groups, sets, tools", setUp, 0);
            DocsGraphNode core = Group(graph, "change", "Change scene", "Start-up, preload", change, 1);
            core.IsKey = true;
            Group(graph, "during", "During the change", "Screen, loading, moves", during, 2);
            graph.Link("setup", "change");
            graph.Link("change", "during");
            return new DocsGraphView(graph);
        }

        private static DocsGraphNode Group(DocsGraph _graph, string _id, string _title, string _line, DocsGraph _inner, int _layer)
        {
            DocsGraphNode node = _graph.Add(_id, _title, _line);
            node.Inner = _inner;
            node.Layer = _layer;
            return node;
        }

        private static DocsGraphNode Part(DocsGraph _graph, string _id, string _title, string _line, string _topic, Action<string> _open)
        {
            DocsGraphNode node = _graph.Add(_id, _title, _line);
            node.Clicked = () => _open(_topic);
            return node;
        }

        // The folders of the code map, with how they depend on each other (who calls whom), read from the code.
        public static DocsGraphView CodeMap()
        {
            DocsGraph entry = new DocsGraph("What you call");
            Folder(entry, "PublicAPI", "Runtime/PublicAPI", "SceneActions, MonoSceneActions", 0);
            Folder(entry, "Configuration", "Runtime/Configuration", "SceneGroup, SceneSet, settings", 0);
            Folder(entry, "StartUp", "Runtime/StartUp", "The first scene change", 0);

            DocsGraph parts = new DocsGraph("Plugged-in parts");
            Folder(parts, "SceneLoaderTypes", "Runtime/SceneLoaderTypes", "Build Settings, Addressables", 0);
            Folder(parts, "Transitions", "../SceneTransitionSystem/Runtime", "Hide and show the screen", 0);
            Folder(parts, "Relocation", "Relocation/Runtime", "Relocatables, spawn points", 0);
            Folder(parts, "SceneScreens", "SceneScreens/Runtime", "Loading and launch scenes", 0);

            DocsGraph editor = new DocsGraph("Editor");
            Folder(editor, "Editor", "Editor", "Page, picker, checks, setup", 0);
            Folder(editor, "RelocationEditor", "Relocation/Editor", "Spawn point tools", 1);
            editor.Link("Editor", "RelocationEditor");

            DocsGraph graph = new DocsGraph("Scene system code");
            Group(graph, "entry", "What you call", "API, settings, start-up", entry, 0);
            DocsGraphNode core = Folder(graph, "Core", "Runtime/Core", "The coordinator", 1);
            core.IsKey = true;
            Group(graph, "parts", "Plugged-in parts", "Loaders, screen, moves", parts, 2);
            Group(graph, "editor", "Editor", "Tools on top of it all", editor, 2);
            graph.Link("entry", "Core");
            graph.Link("Core", "parts");
            graph.Link("Core", "editor");
            return new DocsGraphView(graph);
        }

        private static DocsGraphNode Folder(DocsGraph _graph, string _id, string _relative, string _line, int _layer)
        {
            string assetFolder = SceneDocsPaths.Combine(SceneDocsPaths.Module.TrimEnd('/'), _relative);
            DocsGraphNode node = _graph.Add(_id, _id, _line);
            node.Layer = _layer;
            node.Inner = ScriptsGraph(_id, assetFolder);
            return node;
        }

        // One box per script directly in the folder; an arrow from A to B when A's code names B's type.
        public static DocsGraph ScriptsGraph(string _title, string _assetFolder)
        {
            DocsGraph graph = new DocsGraph(_title);
            List<string> scripts = new List<string>();
            foreach (string path in SceneDocsSource.Files())
            {
                if (SceneDocsPaths.FolderOf(path) == _assetFolder || SceneDocsPaths.FolderOf(path).StartsWith(_assetFolder + "/"))
                {
                    scripts.Add(path);
                }
            }
            Dictionary<string, string> textOf = new Dictionary<string, string>();
            foreach (string path in scripts)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                string summary = SceneDocsSource.FirstSentence(SceneDocsSource.SummaryAbove(path, name));
                DocsGraphNode node = graph.Add(name, name, Shorten(summary));
                string scriptPath = path;
                node.Clicked = () => OpenScript(scriptPath);
                textOf[name] = SceneDocsPaths.Read(path) ?? "";
            }
            foreach (KeyValuePair<string, string> from in textOf)
            {
                foreach (string to in textOf.Keys)
                {
                    bool names = to != from.Key && Regex.IsMatch(from.Value, @"\b" + Regex.Escape(to) + @"\b");
                    if (names)
                    {
                        graph.Link(from.Key, to);
                    }
                }
            }
            return graph;
        }

        private static string Shorten(string _text)
        {
            // Two lines of a code map box hold about 46 characters once the words wrap.
            const int MaxLength = 46;
            if (_text.Length <= MaxLength)
            {
                return _text;
            }
            string cut = _text.Substring(0, MaxLength - 1);
            int lastSpace = cut.LastIndexOf(' ');
            if (lastSpace > MaxLength / 2)
            {
                cut = cut.Substring(0, lastSpace);
            }
            return cut.TrimEnd(' ', ',', ';', ':') + "…";
        }

        private static void OpenScript(string _path)
        {
            UnityEngine.Object script = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_path);
            if (script != null)
            {
                UnityEditor.AssetDatabase.OpenAsset(script);
            }
        }
    }
}
