using System;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Finds a row's asset again at the moment of a click, by GUID, never by the path the row was built with. A scene
    /// or group renamed or moved while the picker is open still opens; one deleted since gives null, so nothing opens.
    /// </summary>
    public static class ScenePickerTarget
    {
        /// <param name="_guidToPath">AssetDatabase.GUIDToAssetPath, or a fake in tests.</param>
        /// <param name="_assetExists">True when an asset of the row's kind is at the path.</param>
        public static string CurrentPath(ScenePickerRow _row, Func<string, string> _guidToPath, Func<string, bool> _assetExists)
        {
            if (_row == null || _row.IsHeading || string.IsNullOrEmpty(_row.AssetGuid))
            {
                return null;
            }
            string path = _guidToPath(_row.AssetGuid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (!_assetExists(path))
            {
                return null;
            }
            return path;
        }
    }
}
