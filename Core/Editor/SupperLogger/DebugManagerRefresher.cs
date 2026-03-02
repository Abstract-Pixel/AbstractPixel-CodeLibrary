using UnityEditor.Callbacks;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace AbstractPixel.Core.Editor
{
    public class DebugManagerRefresher
    {
        [DidReloadScripts]
        private static void RefreshDebugManagerOnScriptsReloaded()
        {
           TypeCache.TypeCollection debugMarkedScriptsCollection = TypeCache.GetTypesWithAttribute<DebuggableAttribute>();

            List<string> scriptNames = new List<string>();
            foreach (Type type in debugMarkedScriptsCollection)
            {
                scriptNames.Add(type.Name);
            }
            string[] GUIDs = AssetDatabase.FindAssets("t:DebugDataBase");
            if(GUIDs.Length<=0)
            {
                Debug.LogError("No DebugManager asset found in the project." +
                    " Please create one via Assets -> Create -> Utility -> DebugManager");
                return;
            }
            string debugManagerPath = AssetDatabase.GUIDToAssetPath(GUIDs[0]);
            DebugDataBase debugDataBase = AssetDatabase.LoadAssetAtPath<DebugDataBase>(debugManagerPath);
            if(debugDataBase!=null)
            {
                debugDataBase.UpdateManifests(scriptNames);
                EditorUtility.SetDirty(debugDataBase);
                AssetDatabase.SaveAssets();
                EnforceUnityPreload(debugDataBase);
            }
        }

        private static void EnforceUnityPreload(DebugDataBase dataBase)
        {
            List<UnityEngine.Object> preloadedObjects = PlayerSettings.GetPreloadedAssets().ToList();
            if(preloadedObjects.Contains(dataBase)==false)
            {
                preloadedObjects.Add(dataBase);
                PlayerSettings.SetPreloadedAssets(preloadedObjects.ToArray());
            }
        }
    }

   
}
