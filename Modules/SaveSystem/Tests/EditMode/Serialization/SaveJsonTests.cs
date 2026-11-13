using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // Checks the serializer settings carried over from main when it was merged into save-system.
    public class SaveJsonTests
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
            SaveJson serializer = new SaveJson();
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
            SaveJson serializer = new SaveJson();
            JToken token = JToken.Parse("{ \"Position\": { \"x\": 0, \"y\": 0, \"z\": 0 }, \"Rotation\": { \"x\": 0, \"y\": 0, \"z\": 0, \"w\": 0 } }");

            PlayerPosition restored = (PlayerPosition)serializer.FromToken(token, typeof(PlayerPosition));

            Assert.AreEqual(Quaternion.identity, restored.Rotation);
        }

        [Test]
        public void FieldThatNoLongerExists_IsSkipped_AndTheRestLoads()
        {
            SaveJson serializer = new SaveJson();
            string olderSaveFile = "{ \"Health\": 40, \"RemovedField\": \"old\" }";

            bool didRead = serializer.TryDeserialize(olderSaveFile, out OnlyHealth restored);

            Assert.IsTrue(didRead);
            Assert.AreEqual(40, restored.Health);
        }

        // S7: clean JSON for Vector2, Vector2Int, Vector3Int, Color and Rect.
        [Test]
        public void WithoutAConverter_NewtonsoftCannotWriteAColorAtAll()
        {
            // What Newtonsoft does today, with no save system settings at all: documents why Color needs a converter.
            // Color.linear and Color.gamma each return a Color, so the plain default serializer recurses forever.
            Assert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(new Color(0.1f, 0.2f, 0.3f, 0.4f)));
        }

        [Test]
        public void Vector2_IsWrittenAsPlainNumbers_AndReadBack()
        {
            SaveJson serializer = new SaveJson();
            Vector2 saved = new Vector2(1.5f, -2.5f);

            JToken token = serializer.ToToken(saved);

            Assert.AreEqual(2, ((JObject)token).Count);
            Assert.AreEqual(saved, (Vector2)serializer.FromToken(token, typeof(Vector2)));
        }

        [Test]
        public void Vector2Int_IsWrittenAsPlainNumbers_AndReadBack()
        {
            SaveJson serializer = new SaveJson();
            Vector2Int saved = new Vector2Int(3, -4);

            JToken token = serializer.ToToken(saved);

            Assert.AreEqual(2, ((JObject)token).Count);
            Assert.AreEqual(saved, (Vector2Int)serializer.FromToken(token, typeof(Vector2Int)));
        }

        [Test]
        public void Vector3Int_IsWrittenAsPlainNumbers_AndReadBack()
        {
            SaveJson serializer = new SaveJson();
            Vector3Int saved = new Vector3Int(3, -4, 5);

            JToken token = serializer.ToToken(saved);

            Assert.AreEqual(3, ((JObject)token).Count);
            Assert.AreEqual(saved, (Vector3Int)serializer.FromToken(token, typeof(Vector3Int)));
        }

        [Test]
        public void Color_IsWrittenAsPlainNumbers_AndReadBack()
        {
            SaveJson serializer = new SaveJson();
            Color saved = new Color(0.1f, 0.2f, 0.3f, 0.4f);

            JToken token = serializer.ToToken(saved);

            Assert.AreEqual(4, ((JObject)token).Count, "Color should hold only r, g, b and a.");
            Assert.AreEqual(saved, (Color)serializer.FromToken(token, typeof(Color)));
        }

        [Test]
        public void Rect_IsWrittenAsPlainNumbers_AndReadBack()
        {
            SaveJson serializer = new SaveJson();
            Rect saved = new Rect(1f, 2f, 30f, 40f);

            JToken token = serializer.ToToken(saved);

            Assert.AreEqual(4, ((JObject)token).Count, "Rect should hold only x, y, width and height, not xMin, center and the rest.");
            Assert.AreEqual(saved, (Rect)serializer.FromToken(token, typeof(Rect)));
        }

        [Test]
        public void OlderSaveFile_WithOnlyTheCoreKeys_StillReadsBack()
        {
            // A save written before these converters existed, or by hand, with only the essential keys.
            SaveJson serializer = new SaveJson();
            JToken token = JToken.Parse("{ \"r\": 0.5, \"g\": 0.5, \"b\": 0.5 }");

            Color restored = (Color)serializer.FromToken(token, typeof(Color));

            Assert.AreEqual(new Color(0.5f, 0.5f, 0.5f, 1f), restored, "A missing alpha defaults to opaque.");
        }

        [Test]
        public void CreatingTheSerializer_LeavesTheGlobalJsonSettingsAlone()
        {
            Func<JsonSerializerSettings> settingsBefore = JsonConvert.DefaultSettings;

            new SaveJson();

            // Unity 7 rule: no global lambda that would hold the save system's types past a code reload.
            Assert.AreSame(settingsBefore, JsonConvert.DefaultSettings);
        }
    }
}
