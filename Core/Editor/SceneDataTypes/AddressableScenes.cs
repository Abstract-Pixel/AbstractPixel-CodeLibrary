namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Holds the registered &lt;see cref="IAddressableSceneProvider"/&gt;. Without one, Addressables is treated as not
    /// installed: no scene is Addressable and the scene field offers only "Add to build".
    /// </summary>
    public static class AddressableScenes
    {
        public static IAddressableSceneProvider Provider { get; private set; }

        public static bool IsInstalled
        {
            get { return Provider != null; }
        }

        public static void Register(IAddressableSceneProvider _provider)
        {
            Provider = _provider;
        }
    }
}
