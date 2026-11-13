using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>The file type of save files on disk. The save system writes JSON, so json and txt are the choices.</summary>
    public enum FileExtension
    {
        [Obsolete("The save system only writes JSON. Use json or txt.")]
        None = 0,
        json = 1,
        [Obsolete("The save system only writes JSON. Use json or txt.")]
        xml = 2,
        txt = 3,
        [Obsolete("Backups live in the Backups folder with the save file's own extension. Use json or txt.")]
        bak = 4,
        [Obsolete("The save system only writes JSON. Use json or txt.")]
        tmp = 5,
    }
}
