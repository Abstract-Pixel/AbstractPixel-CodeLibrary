using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>One header in the Saved Objects window: a save file, or a scene inside a Scene save file.</summary>
    internal sealed class SavedObjectGroup
    {
        public string Title;

        /// <summary>Every saved object under this header, including the ones under its children.</summary>
        public int Count;

        /// <summary>Saved objects that sit directly under this header.</summary>
        public List<SavedObjectEntry> Entries = new List<SavedObjectEntry>();

        /// <summary>One group per scene, for a Scene save file. Empty for every other save file.</summary>
        public List<SavedObjectGroup> Children = new List<SavedObjectGroup>();
    }

    /// <summary>Turns the registered saved objects into the headers the Saved Objects window shows. No drawing, so it is easy to test.</summary>
    internal static class SavedObjectGrouping
    {
        public static List<SavedObjectGroup> Group(IEnumerable<SavedObjectEntry> entries)
        {
            return entries
                .GroupBy(entry => entry.SaveFileName)
                .OrderBy(byFile => byFile.Key)
                .Select(ToGroup)
                .ToList();
        }

        /// <summary>Changes whenever a saved object registers or unregisters, so the window knows when to rebuild.</summary>
        public static string Signature(IEnumerable<SavedObjectEntry> entries)
        {
            IEnumerable<string> parts = entries
                .Select(entry => $"{entry.SaveFileName}|{entry.SceneName}|{entry.SaveId}|{RuntimeHelpers.GetHashCode(entry)}")
                .OrderBy(part => part);
            return string.Join(";", parts);
        }

        static SavedObjectGroup ToGroup(IGrouping<string, SavedObjectEntry> entriesOfOneFile)
        {
            List<SavedObjectEntry> fileEntries = entriesOfOneFile.OrderBy(entry => entry.EntryName).ToList();
            SavedObjectGroup group = new SavedObjectGroup { Title = entriesOfOneFile.Key, Count = fileEntries.Count };

            bool isPerScene = fileEntries[0].Location == SaveLocation.Scene;
            if (!isPerScene)
            {
                group.Entries = fileEntries;
                return group;
            }

            group.Children = fileEntries
                .GroupBy(entry => entry.SceneName ?? string.Empty)
                .OrderBy(byScene => byScene.Key)
                .Select(byScene => new SavedObjectGroup { Title = byScene.Key, Count = byScene.Count(), Entries = byScene.ToList() })
                .ToList();
            return group;
        }
    }
}
