namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Topic ids that code opens with <see cref="SceneSystemDocs.Open"/>. An id is the page path under Pages/ without
    /// ".md", then "#" and the heading's slug. Ids never change once code uses them; a test checks each one resolves.
    /// </summary>
    public static class SceneSystemDocTopics
    {
        public const string Home = "home";
    }
}
