namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>One row of "What just happened": seconds since Play, the message, why it fired and its guide topic.</summary>
    public class SceneDocsTimelineRow
    {
        public float Seconds { get; private set; }
        public string Message { get; private set; }
        public string Detail { get; private set; }
        public string Why { get; private set; }
        public string Topic { get; private set; }

        public SceneDocsTimelineRow(float _seconds, string _message, string _detail)
        {
            Seconds = _seconds;
            Message = _message;
            Detail = _detail;
            Why = SceneDocsTimeline.WhyOf(_message);
            Topic = SceneDocsTimeline.TopicOf(_message);
        }
    }
}
