using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Reads the scenes, scene groups and scene sets the scene picker lists, once per project change. Read only: it
    /// never writes an asset, never marks a scene dirty and never changes Build Settings.
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePickerProject
    {
        /// <summary>Probes and tests replace these to read an empty or fake project; null means the real project.</summary>
        internal static System.Func<List<ScenePickerSceneInfo>> FakeScenes;
        internal static System.Func<List<ScenePickerGroupInfo>> FakeGroups;

        private static List<ScenePickerSceneInfo> scenes;
        private static List<ScenePickerGroupInfo> groups;
        private static List<ScenePickerSetInfo> sets;
        private static readonly Dictionary<string, SceneGroup> groupAssets = new Dictionary<string, SceneGroup>();
        private static bool groupChecksAreCurrent;

        /// <summary>How long the last group check of every scene group took, in milliseconds (shown by probes).</summary>
        public static double LastGroupCheckMilliseconds { get; private set; }

        /// <summary>Raised after the cached project data is dropped, so open views can read it again.</summary>
        public static event System.Action Changed;

        static ScenePickerProject()
        {
            EditorApplication.projectChanged -= Forget;
            EditorApplication.projectChanged += Forget;
            EditorBuildSettings.sceneListChanged -= Forget;
            EditorBuildSettings.sceneListChanged += Forget;
        }

        public static void Forget()
        {
            scenes = null;
            groups = null;
            sets = null;
            groupChecksAreCurrent = false;
            Changed?.Invoke();
        }

        public static List<ScenePickerRow> SceneRows()
        {
            return ScenePickerRows.ForScenes(Scenes(), Groups(), Sets());
        }

        public static List<ScenePickerRow> GroupRows()
        {
            List<ScenePickerGroupInfo> current = Groups();
            RunGroupChecksOnce(current);
            return ScenePickerRows.ForGroups(current);
        }

        /// <summary>Called when the picker opens: the group check runs again once, the first time the Scene Groups tab needs it.</summary>
        public static void BeginPicking()
        {
            groupChecksAreCurrent = false;
        }

        /// <summary>The project settings a status click depends on, read now.</summary>
        public static ScenePickerProjectFacts Facts()
        {
            ScenePickerProjectFacts facts = new ScenePickerProjectFacts();
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            facts.NewScenesGoToAddressables = settings != null && settings.NewScenesAreAddressable && AddressableScenes.IsInstalled;
            facts.AddressablesSetupProblem = AddressablesSetup.Problem();
            UnityEditor.Build.Profile.BuildProfile profile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();
            if (profile != null && profile.overrideGlobalScenes)
            {
                facts.SceneListProfileName = profile.name;
            }
            return facts;
        }

        /// <summary>The scene group or scene set asset with this GUID, for a label or status click; null when it is gone.</summary>
        public static UnityEngine.Object LoadByGuid(string _guid)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadMainAssetAtPath(path);
        }

        // The group check (SceneGroupCheck) of every game scene group, once per popup open and project change.
        private static void RunGroupChecksOnce(List<ScenePickerGroupInfo> _groups)
        {
            if (groupChecksAreCurrent || FakeGroups != null)
            {
                return;
            }
            groupChecksAreCurrent = true;
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (ScenePickerGroupInfo info in _groups)
            {
                ApplyGroupCheck(info);
            }
            watch.Stop();
            LastGroupCheckMilliseconds = watch.Elapsed.TotalMilliseconds;
        }

        private static void ApplyGroupCheck(ScenePickerGroupInfo _info)
        {
            _info.ProblemCount = 0;
            _info.FirstProblem = string.Empty;
            _info.HasMakeAddressableProblem = false;
            SceneGroup group;
            if (_info.IsSample || !groupAssets.TryGetValue(_info.Guid, out group) || group == null)
            {
                return;
            }
            List<SceneGroupProblem> problems = SceneGroupCheck.Find(group);
            _info.ProblemCount = problems.Count;
            if (problems.Count > 0)
            {
                _info.FirstProblem = problems[0].Message;
            }
            foreach (SceneGroupProblem problem in problems)
            {
                if (problem.Fix == SceneGroupProblemFix.MakeAddressable)
                {
                    _info.HasMakeAddressableProblem = true;
                }
            }
        }

        /// <summary>The toolbar button's label: the open group's name, or the active scene's name with "(no group)".</summary>
        public static string ButtonLabel()
        {
            return ScenePickerLabel.For(OpenScenesActiveFirst(), Groups());
        }

        /// <summary>
        /// The GUID of what is open now: for the Scene Groups tab the group whose main scene is open (active scene
        /// first), for the Scenes tab the active scene. Empty when nothing matches.
        /// </summary>
        public static string OpenAssetGuid(bool _groups)
        {
            List<KeyValuePair<string, string>> open = OpenScenesActiveFirst();
            if (!_groups)
            {
                return open.Count > 0 ? open[0].Value : string.Empty;
            }
            foreach (KeyValuePair<string, string> scene in open)
            {
                foreach (ScenePickerGroupInfo group in Groups())
                {
                    if (!string.IsNullOrEmpty(scene.Value) && group.MainSceneGuid == scene.Value)
                    {
                        return group.Guid;
                    }
                }
            }
            return string.Empty;
        }

        private static List<KeyValuePair<string, string>> OpenScenesActiveFirst()
        {
            List<KeyValuePair<string, string>> open = new List<KeyValuePair<string, string>>();
            Scene active = SceneManager.GetActiveScene();
            AddOpenScene(open, active);
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene != active && scene.isLoaded)
                {
                    AddOpenScene(open, scene);
                }
            }
            return open;
        }

        private static void AddOpenScene(List<KeyValuePair<string, string>> _open, Scene _scene)
        {
            if (!_scene.IsValid())
            {
                return;
            }
            string guid = string.IsNullOrEmpty(_scene.path) ? string.Empty : AssetDatabase.AssetPathToGUID(_scene.path);
            _open.Add(new KeyValuePair<string, string>(_scene.name, guid));
        }

        private static List<ScenePickerSceneInfo> Scenes()
        {
            if (scenes != null)
            {
                return scenes;
            }
            if (FakeScenes != null)
            {
                scenes = FakeScenes();
                return scenes;
            }
            scenes = new List<ScenePickerSceneInfo>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneAsset)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                ScenePickerSceneInfo scene = new ScenePickerSceneInfo(name, path, guid);
                scene.Status = SceneStatusReader.Read(null, name, guid, path).Status;
                scenes.Add(scene);
            }
            return scenes;
        }

        private static List<ScenePickerGroupInfo> Groups()
        {
            if (groups != null)
            {
                return groups;
            }
            if (FakeGroups != null)
            {
                groups = FakeGroups();
                return groups;
            }
            groups = new List<ScenePickerGroupInfo>();
            groupAssets.Clear();
            groupChecksAreCurrent = false;
            List<SceneGroup> listed = SceneGroupsDebugger.ListedGroups();
            foreach (SceneGroup group in SceneGroupAssets.FindAll())
            {
                ScenePickerGroupInfo info = GroupInfo(group);
                // The same number the Scene Groups Debugger shows; sample scene groups are not in that list.
                int place = listed.IndexOf(group);
                info.Number = place >= 0 ? place + 1 : 0;
                groups.Add(info);
                groupAssets[info.Guid] = group;
            }
            return groups;
        }

        private static ScenePickerGroupInfo GroupInfo(SceneGroup _group)
        {
            string path = AssetDatabase.GetAssetPath(_group);
            ScenePickerGroupInfo info = new ScenePickerGroupInfo(_group.name, path, AssetDatabase.AssetPathToGUID(path));
            info.IsSample = _group.IsSample;
            info.IsBootGroup = SceneGroupCheck.IsBootGroup(_group);
            if (_group.SceneSet != null)
            {
                info.SceneSetName = _group.SceneSet.name;
                info.SceneSetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_group.SceneSet));
            }

            SceneStatusReport main = ReadScene(_group.MainScene);
            info.MainSceneStatus = main.Status;
            if (main.Status != SceneStatus.NoScene && main.Status != SceneStatus.Missing)
            {
                info.MainSceneName = main.SceneName;
                info.MainScenePath = main.ScenePath;
                info.MainSceneGuid = main.SceneGuid;
            }
            AddSceneGuids(info.OtherSceneGuids, _group.ManagerialBootScenesList);
            AddSceneGuids(info.OtherSceneGuids, _group.ContextualBootScenesList);
            return info;
        }

        private static List<ScenePickerSetInfo> Sets()
        {
            if (sets != null)
            {
                return sets;
            }
            sets = new List<ScenePickerSetInfo>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(SceneSet)))
            {
                SceneSet set = AssetDatabase.LoadAssetAtPath<SceneSet>(AssetDatabase.GUIDToAssetPath(guid));
                if (set == null)
                {
                    continue;
                }
                ScenePickerSetInfo info = new ScenePickerSetInfo(set.name, guid);
                AddSceneGuids(info.SceneGuids, set.ManagerScenes);
                AddSceneGuids(info.SceneGuids, set.SharedScenes);
                sets.Add(info);
            }
            return sets;
        }

        private static void AddSceneGuids(List<string> _guids, List<SceneReference> _scenes)
        {
            if (_scenes == null)
            {
                return;
            }
            foreach (SceneReference scene in _scenes)
            {
                SceneStatusReport report = ReadScene(scene);
                if (!string.IsNullOrEmpty(report.SceneGuid) && !_guids.Contains(report.SceneGuid))
                {
                    _guids.Add(report.SceneGuid);
                }
            }
        }

        private static SceneStatusReport ReadScene(SceneReference _scene)
        {
            if (_scene == null)
            {
                return SceneStatusReader.Read(null);
            }
            return SceneStatusReader.Read(_scene.SceneFieldReference);
        }
    }
}
