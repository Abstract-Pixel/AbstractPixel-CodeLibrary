using System;
using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// When each saved object was last written to its save file and last restored from it, this session, keyed by
    /// save file name and save ID. The Hierarchy badge and the inspector's runtime previewer read this instead of
    /// keeping their own record.
    /// </summary>
    internal static class SaveActivity
    {
        internal sealed class Record
        {
            public DateTime? LastWrittenUtc;
            public DateTime? LastRestoredUtc;
            public string LastRestoredJson;
        }

        static readonly Dictionary<(string FileName, string SaveId), Record> recordsByKey = new Dictionary<(string, string), Record>();

        /// <summary>Raised after a write or a restore is recorded, and after Reset. The editor repaints on it.</summary>
        public static event Action Changed;

        /// <summary>Every save ID in a file that was just written, all at the moment the file was written.</summary>
        internal static void RecordWrite(string fileName, IEnumerable<string> saveIds)
        {
            DateTime now = DateTime.UtcNow;
            foreach (string saveId in saveIds)
            {
                GetOrAdd(fileName, saveId).LastWrittenUtc = now;
            }
            Changed?.Invoke();
        }

        internal static void RecordRestore(string fileName, string saveId, string json)
        {
            Record record = GetOrAdd(fileName, saveId);
            record.LastRestoredUtc = DateTime.UtcNow;
            record.LastRestoredJson = json;
            Changed?.Invoke();
        }

        internal static bool TryGet(string fileName, string saveId, out Record record)
        {
            return recordsByKey.TryGetValue((fileName, saveId), out record);
        }

        /// <summary>Called when play mode starts and stops, so no activity from an earlier session lingers.</summary>
        internal static void Reset()
        {
            recordsByKey.Clear();
            Changed?.Invoke();
        }

        static Record GetOrAdd(string fileName, string saveId)
        {
            (string, string) key = (fileName, saveId);
            if (!recordsByKey.TryGetValue(key, out Record record))
            {
                record = new Record();
                recordsByKey[key] = record;
            }
            return record;
        }
    }
}
