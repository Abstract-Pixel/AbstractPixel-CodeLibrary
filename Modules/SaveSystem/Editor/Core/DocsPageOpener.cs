using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Double-clicking an .html page under any module's Documentation folder opens it in the browser.</summary>
    internal static class DocsPageOpener
    {
        [OnOpenAsset]
        static bool OpenDocsPage(EntityId instanceId, int line)
        {
            string path = AssetDatabase.GetAssetPath(instanceId);
            if (Path.GetExtension(path) != ".html" || !IsUnderADocumentationFolder(path))
            {
                return false;
            }
            Application.OpenURL(new Uri(Path.GetFullPath(path)).AbsoluteUri);
            return true;
        }

        static bool IsUnderADocumentationFolder(string assetPath)
        {
            foreach (string part in assetPath.Replace('\\', '/').Split('/'))
            {
                if (part == "Documentation")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
