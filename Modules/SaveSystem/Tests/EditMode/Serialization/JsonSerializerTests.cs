using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // Checks the serializer settings carried over from main when it was merged into save-system.
    public class JsonSerializerTests
    {
        [Serializable]
        class PlayerPosition
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [Serializable]
        class OnlyHealth
        {
            public int Health;
        }

        [Test]
        public void Vector3AndQuaternion_AreWrittenAsPlainNumbers_AndReadBack()
        {
            JsonSerializer serializer = new JsonSerializer();
            PlayerPosition saved = new PlayerPosition
            {
                Position = new Vector3(1f, 2f, 3f),
                Rotation = Quaternion.Euler(0f, 90f, 0f)
            };

            JToken token = serializer.ToToken(saved);
            PlayerPosition restored = (PlayerPosition)serializer.FromToken(token, typeof(PlayerPosition));

            // Without the converter Newtonsoft also writes normalized, magnitude and more, and can loop forever.
            JObject positionObject = (JObject)token["Position"];
            Assert.AreEqual(3, positionObject.Count, "Position should hold only x, y and z.");
            Assert.AreEqual(saved.Position, restored.Position);
            Assert.AreEqual(saved.Rotation, restored.Rotation);
        }

        [Test]
        public void AllZeroQuaternion_IsReadAsIdentity()
        {
            JsonSerializer serializer = new JsonSerializer();
            JToken token = JToken.Parse("{ \"Position\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"Rotation\": { \"x\": 0, \"y\": 0, \"z\": 0, \"w\": 0 } }");

            PlayerPosition restored = (PlayerPosition)serializer.FromToken(token, typeof(PlayerPosition));

            Assert.AreEqual(Quaternion.identity, restored.Rotation);
        }

        [Test]
        public void FieldThatNoLongerExists_IsSkipped_AndTheRestLoads()
        {
            JsonSerializer serializer = new JsonSerializer();
            string olderSaveFile = "{ \"Health\": 40, \"RemovedField\": \"old\" }";

            bool didRead = serializer.TryDeserialize(olderSaveFile, out OnlyHealth restored);

            Assert.IsTrue(didRead);
            Assert.AreEqual(40, restored.Health);
        }

        [Test]
        public void CreatingTheSerializer_LeavesTheGlobalJsonSettingsAlone()
        {
            Func<JsonSerializerSettings> settingsBefore = JsonConvert.DefaultSettings;

            new JsonSerializer();

            // Unity 7 rule: no global lambda that would hold the save system's types past a code reload.
            Assert.AreSame(settingsBefore, JsonConvert.DefaultSettings);
        }
    }
}
