using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>One saved object's data inside a save file, stored under its save ID.</summary>
    [Serializable]
    internal sealed class SaveFileEntry
    {
        /// <summary>A readable name such as "Forest/Chest (2)/ChestLid", for humans reading the file. Matching uses only the save ID.</summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>The captured data, kept as JSON until the saved object's type is known.</summary>
        [JsonProperty("data")]
        public JToken Data;

        /// <summary>
        /// A gone marker: the object placed in a scene was forgotten during play, so it turns itself off the next time
        /// its scene loads. Left out of the file when false, and a missing field reads as false, so older files read as before.
        /// </summary>
        [JsonProperty("gone", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Gone;
    }
}
