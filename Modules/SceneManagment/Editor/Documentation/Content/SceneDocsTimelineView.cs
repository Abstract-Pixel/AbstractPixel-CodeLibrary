using System;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// "What just happened": one row per scene message or screen state since Play started, newest at the bottom, each
    /// with its time, kind badge, message, why it fired and a link to its guide section.
    /// </summary>
    public class SceneDocsTimelineView : VisualElement
    {
        private readonly Action<string> open;
        private readonly ScrollView list;
        private readonly Label empty;

        public SceneDocsTimelineView(Action<string> _open)
        {
            open = _open;
            AddToClassList("docs-timeline");
            Label title = new Label("What just happened");
            title.AddToClassList("docs-timeline-title");
            Add(title);
            empty = new Label("Press Play: each scene message appears here as it fires.");
            empty.AddToClassList("docs-timeline-empty");
            Add(empty);
            list = new ScrollView(ScrollViewMode.Vertical);
            list.AddToClassList("docs-timeline-list");
            Add(list);
            foreach (SceneDocsTimelineRow row in SceneDocsTimeline.Rows)
            {
                AddRow(row);
            }
            RegisterCallback<AttachToPanelEvent>(_event => Listen(true));
            RegisterCallback<DetachFromPanelEvent>(_event => Listen(false));
            RefreshEmpty();
        }

        /// <summary>The number of rows shown (read by probes to find duplicates).</summary>
        public int RowCount
        {
            get { return list.childCount; }
        }

        private void Listen(bool _on)
        {
            SceneDocsTimeline.RowAdded -= OnRowAdded;
            SceneDocsTimeline.Cleared -= OnCleared;
            if (!_on)
            {
                return;
            }
            SceneDocsTimeline.RowAdded += OnRowAdded;
            SceneDocsTimeline.Cleared += OnCleared;
        }

        private void OnRowAdded(SceneDocsTimelineRow _row)
        {
            bool wasAtEnd = list.verticalScroller.value >= list.verticalScroller.highValue - 1f;
            AddRow(_row);
            RefreshEmpty();
            if (wasAtEnd)
            {
                list.schedule.Execute(() => list.scrollOffset = new UnityEngine.Vector2(0f, float.MaxValue));
            }
        }

        private void OnCleared()
        {
            list.Clear();
            RefreshEmpty();
        }

        private void RefreshEmpty()
        {
            empty.EnableInClassList("docs-hidden", list.childCount > 0);
        }

        private void AddRow(SceneDocsTimelineRow _row)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-timeline-row");
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-timeline-header");
            Label time = new Label(_row.Seconds.ToString("0.00") + " s");
            time.AddToClassList("docs-timeline-time");
            header.Add(time);
            bool isScreen = _row.Message.StartsWith("screen:");
            header.Add(DocsPill.Kind(isScreen ? "screen" : "message"));
            Label message = new Label(isScreen ? _row.Message.Substring(7) : _row.Message);
            message.AddToClassList("docs-timeline-message");
            DocsCodeBlock.ApplyMonoFont(message);
            header.Add(message);
            if (_row.Detail.Length > 0)
            {
                Label detail = new Label(_row.Detail);
                detail.AddToClassList("docs-timeline-detail");
                header.Add(detail);
            }
            row.Add(header);
            Label why = new Label(_row.Why);
            why.AddToClassList("docs-text");
            why.AddToClassList("docs-timeline-why");
            row.Add(why);
            string topic = _row.Topic;
            Button link = new Button(() => open(topic));
            link.text = "Learn more";
            link.tooltip = "Opens the guide section about this message";
            link.AddToClassList("docs-link-button");
            row.Add(link);
            list.Add(row);
        }
    }
}
