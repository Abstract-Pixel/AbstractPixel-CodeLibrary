using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>5B known item 2: a scene in no group plays without a Build Settings entry, so Retry must open it again too.</summary>
    public class SceneInNoGroupTests
    {
        [Test]
        public void ASceneInNoGroupOpensFromItsFileAgain()
        {
            SceneReference lone = new SceneReference("Lone", string.Empty, "Assets/Lone.unity");

            SceneGroup temporary = TemporarySceneGroup.Create(lone, null);

            Assert.IsTrue(DefaultSceneLoader.OpensFromItsFile(lone));
            Assert.IsFalse(DefaultSceneLoader.OpensFromItsFile(new SceneReference("Other", string.Empty, "Assets/Other.unity")));
            Object.DestroyImmediate(temporary);
        }
    }
}
