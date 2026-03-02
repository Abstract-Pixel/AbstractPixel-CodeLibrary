using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.Core
{
    public static class GameLogger
    {
        private static Dictionary<string, ScriptDebugInfo> debuggableScriptsDictionary = new();
        private static bool isInitilized;

        public static void Initialize(DebugDataBase dataBase)
        {
            isInitilized = true;
            debuggableScriptsDictionary = new Dictionary<string, ScriptDebugInfo>();
            if(dataBase.debuggableScriptsList.Count == 0)
            {
                //Debug.LogWarning("DebugDataBase contains no debuggable scripts. Please add scripts to the database to enable logging.");
                return;
            }
            foreach (ScriptDebugInfo debugInfo in dataBase.debuggableScriptsList)
            {
                if (!debuggableScriptsDictionary.Keys.Contains(debugInfo.ScriptName))
                {
                    debuggableScriptsDictionary.Add(debugInfo.ScriptName, debugInfo);
                }
            }
        }

        public static void Log(object sender, string context)
        {
            if (!isInitilized)
            {
                Debug.LogWarning("GameLogger not initialized. Please initialize with DebugDataBase before logging.");
                return;
            }
            string senderTypeName = sender.GetType().Name;
            if (!debuggableScriptsDictionary.TryGetValue(senderTypeName, out ScriptDebugInfo debugInfo))
            {
                Debug.LogError($"No debug info found for script: {senderTypeName}. Please ensure The script you are tryign to debug has [Debuggable] Attribute");
                return;
            }

            if (debugInfo.AllowDebugging)
            {
                string objectSenderColor = ColorUtility.ToHtmlStringRGB(debugInfo.DebugColor);
                Debug.Log($"<color=#{objectSenderColor}>[{senderTypeName}]</color> {context}");
            }
        }

    }
}
