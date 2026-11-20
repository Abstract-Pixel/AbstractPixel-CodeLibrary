using System;
using System.Linq;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Whether a saved script's save file has a row in the save system config. Shared by SaveSystemBodyGuard
    /// (the Console) and the bridge inspector's card (T2), so both report the same mistake the same way.
    /// </summary>
    internal static class SaveFileRowCheck
    {
        /// <summary>Null when the class needs no row, or already has one. Otherwise the warning naming the fix.</summary>
        public static string ProblemWith(Type savedScriptClass, SaveSystemConfigSO config)
        {
            // B14: SaveFileClassOf reads ISaveIn<TFile>, which ISavable<TData, TFile> also extends, so this covers
            // a plain [Save]-field class choosing a file as well as an ISavable one.
            Type saveFileClass = SavedScript.SaveFileClassOf(savedScriptClass);
            if (saveFileClass == null || config == null)
            {
                return null;
            }

            bool hasRow = config.SaveFileList.Any(row => row != null && row.ClassFullName == saveFileClass.FullName);
            bool rowWillBeAdded = Attribute.IsDefined(saveFileClass, typeof(SaveFileDefaultsAttribute));
            if (hasRow || rowWillBeAdded)
            {
                return null;
            }

            return $"Saves into '{saveFileClass.FullName}', but the save system config has no row for that class, so its data isn't saved. Select the config and press Update save file classes, or add a row named '{saveFileClass.Name}'.";
        }
    }
}
