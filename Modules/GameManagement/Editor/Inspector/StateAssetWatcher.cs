using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>An invisible element that reports a change to any of the assets a view shows, wherever the change came from.</summary>
    internal class StateAssetWatcher : VisualElement
    {
        private readonly Action changed;
        private readonly HashSet<Object> watched = new HashSet<Object>();

        public StateAssetWatcher(Action _changed)
        {
            changed = _changed;
            style.display = DisplayStyle.None;
        }

        public void Watch(IEnumerable<Object> _assets)
        {
            HashSet<Object> wanted = new HashSet<Object>(_assets);
            wanted.RemoveWhere(_asset => _asset == null);
            if (watched.SetEquals(wanted))
            {
                return;
            }
            watched.Clear();
            watched.UnionWith(wanted);
            Clear();
            foreach (Object asset in wanted)
            {
                // One element per asset: an element tracks only one object.
                VisualElement tracker = new VisualElement();
                Add(tracker);
                tracker.TrackSerializedObjectValue(new SerializedObject(asset), _data => changed());
            }
        }
    }
}
