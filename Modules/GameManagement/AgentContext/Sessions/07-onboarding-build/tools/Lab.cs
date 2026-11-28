using System.Linq;
using UnityEditor;
using UnityEngine;

public static class Lab
{
    // The Lab's own Preloaded Assets at the start of the session (scene system settings, debug data base).
    static readonly string[] OriginalGuids = { "5d9c28b8fe7efcc408bb8bc3b9af596b", "415bb643defda7448b9df3e4c371b0c1" };

    public static string Restore()
    {
        var originals = OriginalGuids.Select(g => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        PlayerSettings.SetPreloadedAssets(originals);
        AssetDatabase.SaveAssets();
        return "preloaded " + string.Join(", ", PlayerSettings.GetPreloadedAssets().Select(o => o == null ? "null" : o.name));
    }

    public static string Delete(string path)
    {
        bool ok = AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
        return "deleted " + path + " " + ok;
    }

    public static string MakePrefab(string path)
    {
        string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string made = null;
        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
        var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        new GameObject("Panel").transform.SetParent(root.transform);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return "prefab " + path;
    }

    public static string AutoState()
    {
        return "shown=" + SessionState.GetBool("AbstractPixel.GameManagement.OnboardingShown", false)
            + " turnedOff=" + EditorUserSettings.GetConfigValue("AbstractPixel.GameManagement.OnboardingTurnedOff");
    }

    public static string ResetAuto(string clearTurnedOff)
    {
        SessionState.SetBool("AbstractPixel.GameManagement.OnboardingShown", false);
        if (clearTurnedOff == "yes") EditorUserSettings.SetConfigValue("AbstractPixel.GameManagement.OnboardingTurnedOff", null);
        return AutoState();
    }

    public static string Preloaded()
    {
        return string.Join(", ", PlayerSettings.GetPreloadedAssets().Select(o => o == null ? "null" : o.name + "@" + AssetDatabase.GetAssetPath(o)));
    }
}
