using System;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What a pick in the scene picker does (decisions 6 to 8). Open waits until Unity is not compiling or importing,
    /// asks Unity's usual save prompt (Cancel changes nothing), then opens the scene the way a double-click in the
    /// Project window does, or runs "Open group". Select selects and pings the asset and opens nothing. Both find the
    /// asset again by GUID, so a renamed asset still opens and a deleted one opens nothing. A pick never throws, never
    /// marks a scene dirty and never changes Build Settings. One pick at a time: a pick made while another one waits
    /// is ignored.
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePickerActions
    {
        private const string LogPrefix = "[Scene system] Scene picker: ";
        private const string PendingPickKey = "AbstractPixel.SceneSystem.ScenePickerPendingPick";

        // A pick made while scripts compile outlives the domain reload that follows: it is kept in SessionState.
        static ScenePickerActions()
        {
            string pending = SessionState.GetString(PendingPickKey, string.Empty);
            if (string.IsNullOrEmpty(pending))
            {
                return;
            }
            SessionState.EraseString(PendingPickKey);
            ScenePickerRow row = RowFromPending(pending);
            if (row != null)
            {
                EditorApplication.delayCall += () => Open(row);
            }
        }
        private const double LongestWaitSeconds = 60;

        private static ScenePickerRow waitingRow;
        private static double waitStartedAt;

        /// <summary>True while a pick waits for Unity to finish compiling or importing.</summary>
        public static bool IsWaiting
        {
            get { return waitingRow != null; }
        }

        /// <summary>Why nothing can open now, or empty when a pick can open.</summary>
        public static string WhyNothingCanOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "Leave Play mode to open a scene or a scene group.";
            }
            return string.Empty;
        }

        public static void Open(ScenePickerRow _row)
        {
            if (_row == null || _row.IsHeading || IsWaiting)
            {
                return;
            }
            string reason = WhyNothingCanOpen();
            if (!string.IsNullOrEmpty(reason))
            {
                Debug.LogWarning(LogPrefix + "nothing opened. " + reason);
                return;
            }
            waitingRow = _row;
            waitStartedAt = EditorApplication.timeSinceStartup;
            SessionState.SetString(PendingPickKey, PendingFromRow(_row));
            EditorApplication.update -= OpenWhenUnityIsReady;
            EditorApplication.update += OpenWhenUnityIsReady;
        }

        public static void Select(ScenePickerRow _row)
        {
            UnityEngine.Object asset = LoadAsset(_row);
            if (asset == null)
            {
                return;
            }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        // Runs once per Editor update until Unity is idle; the popup has closed by then, so the save prompt stands alone.
        private static void OpenWhenUnityIsReady()
        {
            bool unityIsBusy = EditorApplication.isCompiling || EditorApplication.isUpdating;
            bool waitedTooLong = EditorApplication.timeSinceStartup - waitStartedAt > LongestWaitSeconds;
            if (unityIsBusy && !waitedTooLong)
            {
                return;
            }
            EditorApplication.update -= OpenWhenUnityIsReady;
            ScenePickerRow row = waitingRow;
            waitingRow = null;
            SessionState.EraseString(PendingPickKey);
            if (unityIsBusy)
            {
                Debug.LogWarning(LogPrefix + "nothing opened: Unity was still busy after " + LongestWaitSeconds + " seconds. Pick '" + row.Title + "' again.");
                return;
            }
            try
            {
                OpenNow(row);
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + "could not open '" + row.Title + "': " + exception.Message);
            }
        }

        private static void OpenNow(ScenePickerRow _row)
        {
            string reason = WhyNothingCanOpen();
            if (!string.IsNullOrEmpty(reason))
            {
                Debug.LogWarning(LogPrefix + "nothing opened. " + reason);
                return;
            }
            if (_row.Kind == ScenePickerRowKind.SceneGroup)
            {
                OpenGroup(_row);
                return;
            }
            OpenScene(_row);
        }

        // What a double-click on the scene in the Project window does: "Open the group with its main scene" reacts to it.
        private static void OpenScene(ScenePickerRow _row)
        {
            string path = CurrentPath(_row);
            if (path == null)
            {
                return;
            }
            bool userKeptGoing = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!userKeptGoing)
            {
                return;
            }
            ScenePickerRecent.Remember(_row.AssetGuid);
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void OpenGroup(ScenePickerRow _row)
        {
            SceneGroup group = LoadAsset(_row) as SceneGroup;
            if (group == null)
            {
                return;
            }
            SceneStatusReport main = group.MainScene == null ? null : SceneStatusReader.Read(group.MainScene.SceneFieldReference);
            bool hasMainScene = main != null && main.Status != SceneStatus.NoScene && main.Status != SceneStatus.Missing;
            if (!hasMainScene)
            {
                Debug.LogWarning(LogPrefix + "'" + group.name + "' has no main scene, so nothing opened. Select it and set its main scene in the Inspector.", group);
                return;
            }
            ScenePickerRecent.Remember(_row.AssetGuid);
            SceneGroupOpener.Open(group);
        }

        // "SceneGroup|guid|title": enough to find the asset again by GUID after a domain reload.
        private static string PendingFromRow(ScenePickerRow _row)
        {
            return _row.Kind + "|" + _row.AssetGuid + "|" + _row.Title;
        }

        private static ScenePickerRow RowFromPending(string _pending)
        {
            string[] parts = _pending.Split(new[] { '|' }, 3);
            if (parts.Length < 3)
            {
                return null;
            }
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = parts[0] == ScenePickerRowKind.SceneGroup.ToString() ? ScenePickerRowKind.SceneGroup : ScenePickerRowKind.Scene;
            row.AssetGuid = parts[1];
            row.Title = parts[2];
            return row;
        }

        private static UnityEngine.Object LoadAsset(ScenePickerRow _row)
        {
            string path = CurrentPath(_row);
            if (path == null)
            {
                return null;
            }
            return AssetDatabase.LoadMainAssetAtPath(path);
        }

        private static string CurrentPath(ScenePickerRow _row)
        {
            string path = ScenePickerTarget.CurrentPath(_row, AssetDatabase.GUIDToAssetPath, AssetExists);
            if (path == null && _row != null && !_row.IsHeading)
            {
                Debug.LogWarning(LogPrefix + "'" + _row.Title + "' no longer exists, so nothing happened. The picker shows the project as it is now.");
                ScenePickerProject.Forget();
            }
            return path;
        }

        private static bool AssetExists(string _path)
        {
            return AssetDatabase.LoadMainAssetAtPath(_path) != null;
        }
    }
}
