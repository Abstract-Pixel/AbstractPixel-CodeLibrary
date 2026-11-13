namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Decides when a Savable Bridge needs a new GUID. The GUID is its save ID, so it changes only for a bridge that
    /// has none or is a copy of another bridge, never because its scene was saved, saved under another name, or renamed.
    /// </summary>
    internal static class SavableBridgeIdentity
    {
        // "GlobalObjectId_V1-<kind>-<scene GUID>-<object ID>-<prefab ID>". A scene that was never saved gives every
        // object kind 0 and zeros, so its ids can't tell one object from another.
        const char PartSeparator = '-';
        const int KindPart = 1;
        const int ScenePart = 2;
        const string NotSavedKind = "0";

        public static bool NeedsNewGuid(string uniqueId, string lastKnownGlobalObjectId, string currentGlobalObjectId)
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                return true;
            }
            bool hasNothingToCompare = !IsFromASavedScene(lastKnownGlobalObjectId) || !IsFromASavedScene(currentGlobalObjectId);
            if (hasNothingToCompare)
            {
                return false;
            }
            return !IsTheSameObjectInAnyScene(lastKnownGlobalObjectId, currentGlobalObjectId);
        }

        /// <summary>True for an id of an object in a saved scene, the only kind worth remembering.</summary>
        public static bool IsFromASavedScene(string globalObjectId)
        {
            if (string.IsNullOrEmpty(globalObjectId))
            {
                return false;
            }
            string[] parts = globalObjectId.Split(PartSeparator);
            return parts.Length > ScenePart && parts[KindPart] != NotSavedKind;
        }

        // Only the scene GUID may differ: a copy of the bridge has a new object ID.
        static bool IsTheSameObjectInAnyScene(string lastKnown, string current)
        {
            string[] lastKnownParts = lastKnown.Split(PartSeparator);
            string[] currentParts = current.Split(PartSeparator);
            if (lastKnownParts.Length != currentParts.Length)
            {
                return false;
            }
            for (int part = 0; part < currentParts.Length; part++)
            {
                if (part != ScenePart && lastKnownParts[part] != currentParts[part])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
