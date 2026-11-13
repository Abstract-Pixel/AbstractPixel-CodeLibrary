using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace ScreenshotTool.Editor
{
    public sealed class ScreenshotSessionTracker
    {
        private readonly List<ScreenshotSessionItem> trackedItems = new List<ScreenshotSessionItem>();

        public IReadOnlyList<ScreenshotSessionItem> TrackedItems => trackedItems;
        public int TotalCapturedCount => trackedItems.Count;

        public int KeptCount
        {
            get
            {
                int count = 0;
                for (int itemIndex = 0; itemIndex < trackedItems.Count; itemIndex++)
                {
                    if (trackedItems[itemIndex].IsMarkedToKeep)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public int UnmarkedCount => TotalCapturedCount - KeptCount;
        public bool HasSessionData => trackedItems.Count > 0;

        public void RegisterCapturedFile(string _filePath)
        {
            if (string.IsNullOrWhiteSpace(_filePath) || !File.Exists(_filePath))
            {
                return;
            }

            trackedItems.Add(new ScreenshotSessionItem(_filePath));
        }

        public void MarkAllToKeep(bool _keepAll)
        {
            for (int itemIndex = 0; itemIndex < trackedItems.Count; itemIndex++)
            {
                trackedItems[itemIndex].IsMarkedToKeep = _keepAll;
            }
        }

        public void KeepAllAndFinalizeSession()
        {
            CommitRenamesOnly();
            ClearSession();
        }

        public void ClearSession()
        {
            for (int itemIndex = 0; itemIndex < trackedItems.Count; itemIndex++)
            {
                trackedItems[itemIndex].DisposeThumbnail();
            }
            trackedItems.Clear();
        }

        public void DeleteAll()
        {
            for (int itemIndex = trackedItems.Count - 1; itemIndex >= 0; itemIndex--)
            {
                ScreenshotSessionItem currentItem = trackedItems[itemIndex];
                if (File.Exists(currentItem.CurrentFilePath))
                {
                    try
                    {
                        File.Delete(currentItem.CurrentFilePath);
                    }
                    catch (Exception)
                    {
                    }
                }

                currentItem.DisposeThumbnail();
                trackedItems.RemoveAt(itemIndex);
            }
        }

        public void DeleteUnmarkedNow()
        {
            for (int itemIndex = trackedItems.Count - 1; itemIndex >= 0; itemIndex--)
            {
                ScreenshotSessionItem currentItem = trackedItems[itemIndex];
                if (!currentItem.IsMarkedToKeep)
                {
                    if (File.Exists(currentItem.CurrentFilePath))
                    {
                        try
                        {
                            File.Delete(currentItem.CurrentFilePath);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    currentItem.DisposeThumbnail();
                    trackedItems.RemoveAt(itemIndex);
                }
            }
        }

        public void CommitRenamesOnly()
        {
            if (trackedItems.Count == 0)
            {
                return;
            }

            for (int itemIndex = 0; itemIndex < trackedItems.Count; itemIndex++)
            {
                ScreenshotSessionItem currentItem = trackedItems[itemIndex];
                string currentFileNameOnDisk = Path.GetFileNameWithoutExtension(currentItem.CurrentFilePath);
                string sanitizedDesiredName = SanitizeFileName(currentItem.DesiredFileNameWithoutExtension);

                if (!string.Equals(currentFileNameOnDisk, sanitizedDesiredName, StringComparison.Ordinal))
                {
                    string directory = Path.GetDirectoryName(currentItem.CurrentFilePath);
                    string newFilePath = Path.Combine(directory, $"{sanitizedDesiredName}{currentItem.FileExtension}");

                    if (File.Exists(currentItem.CurrentFilePath))
                    {
                        try
                        {
                            if (File.Exists(newFilePath))
                            {
                                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                                newFilePath = Path.Combine(directory, $"{sanitizedDesiredName}_{timeStamp}{currentItem.FileExtension}");
                            }

                            File.Move(currentItem.CurrentFilePath, newFilePath);
                            currentItem.CurrentFilePath = newFilePath;
                            currentItem.DesiredFileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFilePath);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        public void FinalizeAndPurgeUnmarked()
        {
            CommitRenamesOnly();
            DeleteUnmarkedNow();
        }

        public void RevealInExplorer(ScreenshotSessionItem _item)
        {
            if (_item != null && File.Exists(_item.CurrentFilePath))
            {
                EditorUtility.RevealInFinder(_item.CurrentFilePath);
            }
        }

        public void OpenInDefaultViewer(ScreenshotSessionItem _item)
        {
            if (_item != null && File.Exists(_item.CurrentFilePath))
            {
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(_item.CurrentFilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(processStartInfo);
                }
                catch (Exception)
                {
                    EditorUtility.OpenWithDefaultApp(_item.CurrentFilePath);
                }
            }
        }

        private static string SanitizeFileName(string _fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = _fileName;

            for (int charIndex = 0; charIndex < invalidChars.Length; charIndex++)
            {
                sanitized = sanitized.Replace(invalidChars[charIndex], '_');
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "Screenshot" : sanitized.Trim();
        }
    }
}