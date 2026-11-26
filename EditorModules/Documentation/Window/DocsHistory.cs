using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// Back and Forward, like a browser: a list of topic ids and the index of the current one. Serializable, so the
    /// window keeps it across a domain reload.
    /// </summary>
    [Serializable]
    public class DocsHistory
    {
        [SerializeField] private List<string> topics = new List<string>();
        [SerializeField] private int index = -1;
        private const int MaxEntries = 100;

        public string Current
        {
            get { return index >= 0 && index < topics.Count ? topics[index] : null; }
        }

        public bool CanGoBack
        {
            get { return index > 0; }
        }

        public bool CanGoForward
        {
            get { return index >= 0 && index < topics.Count - 1; }
        }

        public int Count
        {
            get { return topics.Count; }
        }

        /// <summary>Visits a topic: drops anything after the current one, then adds it (not twice in a row).</summary>
        public void Visit(string _topicId)
        {
            if (Current == _topicId)
            {
                return;
            }
            int firstToDrop = index + 1;
            if (firstToDrop < topics.Count)
            {
                topics.RemoveRange(firstToDrop, topics.Count - firstToDrop);
            }
            topics.Add(_topicId);
            if (topics.Count > MaxEntries)
            {
                topics.RemoveAt(0);
            }
            index = topics.Count - 1;
        }

        /// <summary>The last pages visited up to the current one, one entry per page, oldest first.</summary>
        public List<string> RecentPages(int _max)
        {
            List<string> pages = new List<string>();
            List<string> recent = new List<string>();
            for (int at = index; at >= 0 && recent.Count < _max; at--)
            {
                string topic = topics[at];
                int hash = topic.IndexOf('#');
                string page = hash < 0 ? topic : topic.Substring(0, hash);
                if (!pages.Contains(page))
                {
                    pages.Add(page);
                    recent.Insert(0, topic);
                }
            }
            return recent;
        }

        public string Back()
        {
            if (CanGoBack)
            {
                index--;
            }
            return Current;
        }

        public string Forward()
        {
            if (CanGoForward)
            {
                index++;
            }
            return Current;
        }
    }
}
