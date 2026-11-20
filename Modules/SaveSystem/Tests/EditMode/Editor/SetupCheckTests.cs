using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // 2.0: every setup problem, merged from the checks that used to run on their own. Each kind is exercised
    // through SetupCheck's testable building blocks, with inputs handed in instead of touching the real project's
    // TypeCache, config or scenes, the same way SavedFieldScan.Scan(fields) and the build guards' CheckConfig/CheckScene are.
    public class SetupCheckTests
    {
        sealed class UnlistedSaveFile : ISaveFile { }

        // No [Save] attribute: SavedFieldRules.ProblemWith only looks at a field's shape, so this stays invisible
        // to the project-wide [Save] scan while still proving ProblemsFromFields finds the same problem.
        class BadFieldShape
        {
            public static int staticField;
        }

        class FieldScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        class UnlistedFileScript : MonoBehaviour, ISaveIn<UnlistedSaveFile>
        {
            [Save] public int count;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();
        SaveSystemConfigSO config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SetupCheck.Forget();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
            gameObjects.Clear();
            Object.DestroyImmediate(config);
            SetupCheck.Forget();
        }

        [Test]
        public void ProblemsFromFields_ReportsAnInvalidFieldShape()
        {
            FieldInfo badField = typeof(BadFieldShape).GetField(nameof(BadFieldShape.staticField));

            List<SetupProblem> problems = SetupCheck.ProblemsFromFields(new[] { badField });

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(typeof(BadFieldShape), problems[0].ScriptType);
            StringAssert.Contains("static", problems[0].WhatIsWrong);
            StringAssert.DoesNotContain("[SaveSystem]", problems[0].WhatIsWrong);
            Assert.IsNotEmpty(problems[0].HowToFix);
        }

        [Test]
        public void ProblemsFromFields_WithOnlyValidFields_ReportsNothing()
        {
            FieldInfo validField = typeof(FieldScript).GetField(nameof(FieldScript.gold));

            Assert.AreEqual(0, SetupCheck.ProblemsFromFields(new[] { validField }).Count);
        }

        [Test]
        public void MissingConfigRowProblems_ReportsAClassWithNoRow()
        {
            List<SetupProblem> problems = SetupCheck.MissingConfigRowProblems(new[] { typeof(UnlistedFileScript) }, config);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(typeof(UnlistedFileScript), problems[0].ScriptType);
            StringAssert.Contains(typeof(UnlistedSaveFile).FullName, problems[0].WhatIsWrong);
        }

        [Test]
        public void MissingConfigRowProblems_WithNoConfig_ReportsNothing()
        {
            Assert.AreEqual(0, SetupCheck.MissingConfigRowProblems(new[] { typeof(UnlistedFileScript) }, null).Count);
        }

        [Test]
        public void StaleGameSaveFilesProblemOrNull_WithAnUnwrittenScriptPath_ReportsAProblem()
        {
            config.ReplaceSaveFileList(new[] { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) });

            SetupProblem problem = SetupCheck.StaleGameSaveFilesProblemOrNull(config);

            Assert.IsNotNull(problem);
            Assert.IsNull(problem.ScriptType);
            Assert.AreEqual(config, problem.Target);
        }

        [Test]
        public void MissingBridgeProblems_ReportsASavedScriptWithNoBridge()
        {
            GameObject chest = NewGameObject("Chest");
            chest.AddComponent<FieldScript>();

            List<SetupProblem> problems = SetupCheck.MissingBridgeProblems(chest.scene);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(chest, problems[0].Target);
            Assert.AreEqual(typeof(FieldScript), problems[0].ScriptType);
        }

        [Test]
        public void MissingBridgeProblems_WithABridge_ReportsNothing()
        {
            GameObject chest = NewGameObject("Chest");
            chest.AddComponent<FieldScript>();
            chest.AddComponent<SavableBridge>();

            Assert.AreEqual(0, SetupCheck.MissingBridgeProblems(chest.scene).Count);
        }

        [Test]
        public void ProblemsOn_FindsAMissingBridgeOnThatGameObjectOnly()
        {
            GameObject chest = NewGameObject("Chest");
            chest.AddComponent<FieldScript>();
            GameObject plain = NewGameObject("Plain");
            SetupCheck.Forget();

            List<SetupProblem> onChest = SetupCheck.ProblemsOn(chest).ToList();
            List<SetupProblem> onPlain = SetupCheck.ProblemsOn(plain).ToList();

            Assert.AreEqual(1, onChest.Count);
            Assert.AreEqual(0, onPlain.Count);
        }

        [Test]
        public void Problems_IsCachedUntilForgotten()
        {
            GameObject chest = NewGameObject("Chest");
            chest.AddComponent<FieldScript>();
            SetupCheck.Forget();
            int before = SetupCheck.ProblemsOn(chest).Count;

            chest.AddComponent<SavableBridge>();
            // No Forget() yet: the cache still answers with the stale problem.
            Assert.AreEqual(before, SetupCheck.ProblemsOn(chest).Count);

            SetupCheck.Forget();
            Assert.AreEqual(0, SetupCheck.ProblemsOn(chest).Count);
        }

        GameObject NewGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObjects.Add(gameObject);
            return gameObject;
        }
    }
}
