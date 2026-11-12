using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>One saved object's data inside a save file, stored under its save ID.</summary>
    [Serializable]
    public sealed class SaveFileEntry
    {
        /// <summary>A readable name such as "Forest/Chest (2)/ChestLid", for humans reading the file. Matching uses only the save ID.</summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>The captured data, kept as JSON until the saved object's type is known.</summary>
        [JsonProperty("data")]
        public JToken Data;
    }
}
