namespace AbstractPixel.Core
{
    /// <summary>
    /// A number that tells Unity objects apart for the length of an Editor or game session. Unity 7 replaces
    /// Object.GetInstanceID with GetEntityId (GetInstanceID is an error there), so this one call works on both.
    /// </summary>
    public static class UnityObjectIds
    {
        public static int Of(UnityEngine.Object _object)
        {
            if (_object == null)
            {
                return 0;
            }
#if UNITY_7000_0_OR_NEWER
            return _object.GetEntityId().GetHashCode();
#else
            return _object.GetInstanceID();
#endif
        }
    }
}
