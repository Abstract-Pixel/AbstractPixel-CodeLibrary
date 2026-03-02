using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    [CreateAssetMenu(fileName = "DebugDataBase", menuName = "Utility/DebugDataBase")]
    public class DebugDataBase : ScriptableObject
    {
        [SerializeField] public List<ScriptDebugInfo> debuggableScriptsList;
        private void OnEnable()
        {
            GameLogger.Initialize(this);

        }

        private void OnValidate()
        {
            GameLogger.Initialize(this);
        }

        public void UpdateManifests(List<string> foundClassNames)
        {
            Dictionary<string, ScriptDebugInfo> tempDebuggableDictionary = new Dictionary<string, ScriptDebugInfo>();
            foreach (ScriptDebugInfo debugInfo in debuggableScriptsList)
            {
                if(!tempDebuggableDictionary.ContainsKey(debugInfo.ScriptName))
                {
                    tempDebuggableDictionary.Add(debugInfo.ScriptName, debugInfo);
                }        
            }
            debuggableScriptsList.Clear();
            foreach (string className in foundClassNames)
            {
                if (tempDebuggableDictionary.TryGetValue(className, out ScriptDebugInfo debugInfo))
                {
                    debuggableScriptsList.Add(debugInfo);
                }
                else
                {
                    ScriptDebugInfo newDebugInfo = new ScriptDebugInfo
                    {
                        ScriptName = className,
                        AllowDebugging = true,
                        DebugColor = Color.white
                    };
                    debuggableScriptsList.Add(newDebugInfo);
                }
            }
            GameLogger.Initialize(this);
        }
    }
}
