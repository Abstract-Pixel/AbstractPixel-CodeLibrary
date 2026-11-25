using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Every scene group under the scene system Samples folder is a sample scene group with no scene set, and none of
    /// its scenes is in Build Settings (ADR 0014; scene system S, decision D6).
    /// </summary>
    public class SceneSystemSamplesGroupTests
    {
        public static IEnumerable<string> SampleGroupPaths()
        {
            List<string> paths = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:SceneGroup", new[] { SampleFolders.SamplesAssetPath });
            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort();
            return paths;
        }

        [Test]
        public void TheSamplesHaveSceneGroups()
        {
            List<string> paths = new List<string>(SampleGroupPaths());

            Assert.IsNotEmpty(paths, "no scene group was found under " + SampleFolders.SamplesAssetPath);
        }

        [TestCaseSource(nameof(SampleGroupPaths))]
        public void IsASampleSceneGroupWithNoSceneSet(string _groupPath)
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(_groupPath);

            Assert.IsNotNull(group, _groupPath);
            Assert.IsTrue(group.IsSample, _groupPath + ": Sample scene group is not ticked");
            Assert.IsNull(group.SceneSet, _groupPath + ": a sample scene group has no scene set");
        }

        [TestCaseSource(nameof(SampleGroupPaths))]
        public void NoSceneOfTheGroupIsInBuildSettings(string _groupPath)
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(_groupPath);
            HashSet<string> buildGuids = new HashSet<string>();
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                buildGuids.Add(buildScene.guid.ToString());
            }

            List<string> inBuild = new List<string>();
            foreach (AbstractPixel.Core.SceneReference scene in ScenesOf(group))
            {
                if (scene == null || string.IsNullOrEmpty(scene.SceneGuid))
                {
                    continue;
                }
                if (buildGuids.Contains(scene.SceneGuid))
                {
                    inBuild.Add(scene.SceneName);
                }
            }

            Assert.IsEmpty(inBuild, _groupPath + " has scenes in Build Settings");
        }

        private static List<AbstractPixel.Core.SceneReference> ScenesOf(SceneGroup _group)
        {
            List<AbstractPixel.Core.SceneReference> scenes = new List<AbstractPixel.Core.SceneReference>();
            scenes.Add(_group.MainScene);
            scenes.AddRange(_group.ManagerialBootScenesList);
            scenes.AddRange(_group.ContextualBootScenesList);
            scenes.Add(_group.LoadingScene);
            scenes.Add(_group.SampleLaunchScene);
            return scenes;
        }
    }
}
