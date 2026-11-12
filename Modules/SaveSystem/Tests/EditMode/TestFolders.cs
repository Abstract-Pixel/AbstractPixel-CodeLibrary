using System;
using System.IO;

namespace AbstractPixel.SaveSystem.Tests
{
    /// <summary>Folders for tests that touch the disk. They sit in the project's Temp folder, never outside the project.</summary>
    internal static class TestFolders
    {
        public static string CreateTemporaryFolderPath()
        {
            return Path.GetFullPath(Path.Combine("Temp", "SaveSystemTests", Guid.NewGuid().ToString("N")));
        }
    }
}
