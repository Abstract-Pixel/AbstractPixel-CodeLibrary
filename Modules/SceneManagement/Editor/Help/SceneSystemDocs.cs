using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The one way Scene System Editor code opens the scene documentation. The docs window lives in its own assembly,
    /// which references this one, so it sets <see cref="Handler"/> when the Editor loads. Pass a topic id from
    /// <see cref="SceneSystemDocTopics"/>.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneSystemDocs
    {
        /// <summary>Opens the scene documentation at a topic id. Set by the docs assembly at load.</summary>
        public static Action<string> Handler { get; set; }

        /// <summary>The menu item that opens the scene documentation at Home.</summary>
        public const string MenuPath = "Tools/External Project Tools/Scene System/Open Scene Documentation";

        /// <summary>Opens the scene documentation at this topic (a page path, then "#" and a heading id).</summary>
        public static void Open(string _topicId)
        {
            if (Handler == null)
            {
                Debug.LogWarning("[Scene documentation] The docs window is not loaded, so topic '" + _topicId + "' cannot open. Check the Console for compile errors.");
                return;
            }
            Handler(_topicId);
        }
    }
}
