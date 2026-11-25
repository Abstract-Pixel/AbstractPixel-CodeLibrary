namespace AbstractPixel.Core
{
    /// <summary>
    /// A number that tells Unity objects apart for the length of an Editor or game session. Unity 6.7 and 7 replace
    /// Object.GetInstanceID with GetEntityId (GetInstanceID is an error there), so this one call works on all.
    /// </summary>
    public static class UnityObjectIds
    {
        public static int Of(UnityEngine.Object _object)
        {
            if (_object == null)
            {
                return 0;
            }
#if UNITY_6000_7_OR_NEWER
            return _object.GetEntityId().GetHashCode();
#else
            return _object.GetInstanceID();
#endif
        }
    }
}
