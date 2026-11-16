using System.Collections.Generic;
using System.Text.RegularExpressions;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // T3: a loaded scene with a saved script and no bridge fails the build and logs an error in play mode, instead
    // of getting a bridge added for it there.
    public class SavableBridgeSceneGuardTests
    {
        class FieldScript : MonoBehaviour
        {
            [Save] public int gold;
        }

        readonly List<GameObject> gameObjects = new List<GameObject>();

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
        }

        [Test]
        public void ASceneWithNoBridge_FailsTheBuild()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();

            Assert.Throws<BuildFailedException>(() => SavableBridgeSceneBuildGuard.CheckScene(gameObject.scene));
        }

        // Unity also calls the build hook for every scene loaded in play mode in the Editor, with no build report.
        // The play mode check already logs that scene, so the build hook must not throw there too.
        [Test]
        public void ASceneLoadedInPlayMode_IsLeftToThePlayModeCheck()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();

            Assert.DoesNotThrow(() => new SavableBridgeSceneBuildGuard().OnProcessScene(gameObject.scene, null));
        }

        [Test]
        public void ASceneWhereEveryScriptHasABridge_DoesNotFailTheBuild()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();
            gameObject.AddComponent<SavableBridge>();

            Assert.DoesNotThrow(() => SavableBridgeSceneBuildGuard.CheckScene(gameObject.scene));
        }

        [Test]
        public void ASceneWithNoBridge_LogsOneErrorForPlayMode()
        {
            GameObject gameObject = new GameObject("Chest");
            gameObjects.Add(gameObject);
            gameObject.AddComponent<FieldScript>();

            SaveLogAssert.Expect(LogType.Error, new Regex("no Savable Bridge.*Chest"));
            SavableBridgePlayModeCheck.CheckScene(gameObject.scene);
        }
    }
}
