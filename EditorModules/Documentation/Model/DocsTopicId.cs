namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>A topic id is a page id with an optional "#" and a heading id.</summary>
    public static class DocsTopicId
    {
        /// <summary>Splits "guide/preload#minimum-time" into its page and its anchor (empty when none).</summary>
        public static void Split(string _topicId, out string _page, out string _anchor)
        {
            string id = (_topicId ?? "").Trim();
            int hash = id.IndexOf('#');
            _page = hash < 0 ? id : id.Substring(0, hash);
            _anchor = hash < 0 ? "" : id.Substring(hash + 1);
            _page = _page.ToLowerInvariant();
        }
    }
}
