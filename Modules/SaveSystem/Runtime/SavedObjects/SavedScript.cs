using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// What the save system needs to know about a saved script's class: whether it saves, what data it saves and
    /// which save file the data goes in. The Savable Bridge, the editor tools and the save file lookup all ask here.
    /// </summary>
    internal static class SavedScript
    {
        /// <summary>True when the class implements ISavable&lt;TData&gt;, directly or through ISavable&lt;TData, TFile&gt;.</summary>
        public static bool IsSaved(Type scriptClass)
        {
            return FindSavableInterface(scriptClass) != null;
        }

        /// <summary>The ISavable&lt;TData&gt; the class implements, whose CaptureData and RestoreData the bridge calls. Null if none.</summary>
        public static Type FindSavableInterface(Type scriptClass)
        {
            return FindInterface(scriptClass, typeof(ISavable<>));
        }

        /// <summary>The TFile of ISavable&lt;TData, TFile&gt;, or null when the class leaves the save file to the default.</summary>
        public static Type SaveFileClassOf(Type scriptClass)
        {
            Type savableInAFile = FindInterface(scriptClass, typeof(ISavable<,>));
            return savableInAFile?.GetGenericArguments()[1];
        }

        static Type FindInterface(Type scriptClass, Type genericInterface)
        {
            foreach (Type implemented in scriptClass.GetInterfaces())
            {
                if (implemented.IsGenericType && implemented.GetGenericTypeDefinition() == genericInterface)
                {
                    return implemented;
                }
            }
            return null;
        }
    }
}
