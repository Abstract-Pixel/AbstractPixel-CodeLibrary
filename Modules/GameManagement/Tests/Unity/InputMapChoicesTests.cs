using System.Collections.Generic;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Tests
{
    public class InputMapChoicesTests
    {
        [Test]
        public void LabelsAreNamesAndValuesAreIds()
        {
            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap driving = asset.AddActionMap("Driving");
            List<string> labels = new List<string>();
            List<string> ids = new List<string>();

            InputMapChoices.List(asset, labels, ids);

            CollectionAssert.AreEqual(new[] { "Driving" }, labels);
            CollectionAssert.AreEqual(new[] { driving.id.ToString() }, ids);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void NoInputSourceHasNoChoices()
        {
            GameManagementSettings settings = ScriptableObject.CreateInstance<GameManagementSettings>();
            Assert.IsNull(InputMapChoices.AssetFor(settings));
            Object.DestroyImmediate(settings);
        }
    }
}
