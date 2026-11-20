using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Turns a parsed save file's JSON values into each target's own data type on a worker thread, so the main thread only
    /// hands finished objects to the receivers (SavableBridge.RestoreState passes values of the right type through).
    /// Anything that fails to convert here is left as JSON and converted on the main thread as before.
    /// </summary>
    internal static class SaveFilePreConverter
    {
        private const char Separator = '#';

        /// <param name="_typesByBridge">Bridge GUID -> (target GUID -> data type), read on the main thread.</param>
        public static void ConvertInPlace(SaveFileData _file, Dictionary<string, Dictionary<string, Type>> _typesByBridge)
        {
            if (_file == null || _file.DataMap == null)
            {
                return;
            }

            List<string> bridgeKeys = new List<string>(_file.DataMap.Keys);
            foreach (string bridgeKey in bridgeKeys)
            {
                string bridgeGuid = GuidOf(bridgeKey);
                if (!_typesByBridge.TryGetValue(bridgeGuid, out Dictionary<string, Type> typesByTarget))
                {
                    continue;
                }

                JObject targetsJson = _file.DataMap[bridgeKey] as JObject;
                if (targetsJson == null)
                {
                    continue;
                }

                _file.DataMap[bridgeKey] = ConvertTargets(targetsJson, typesByTarget);
            }
        }

        private static Dictionary<string, object> ConvertTargets(JObject _targetsJson, Dictionary<string, Type> _typesByTarget)
        {
            Dictionary<string, object> targets = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JToken> entry in _targetsJson)
            {
                object value = entry.Value;
                bool hasType = _typesByTarget.TryGetValue(GuidOf(entry.Key), out Type targetType);
                if (hasType)
                {
                    value = TryConvert(entry.Value, targetType);
                }
                targets[entry.Key] = value;
            }
            return targets;
        }

        private static object TryConvert(JToken _json, Type _targetType)
        {
            try
            {
                return _json.ToObject(_targetType);
            }
            catch (Exception)
            {
                // Left as JSON: the main thread converts it the usual way (and reports problems there).
                return _json;
            }
        }

        private static string GuidOf(string _compositeKey)
        {
            int separatorIndex = _compositeKey.LastIndexOf(Separator) + 1;
            return _compositeKey.Substring(separatorIndex);
        }
    }
}
