using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Adds "Held saved objects" to the bottom of a GameObject's inspector header, in play mode, listing the
    /// registered plain objects its components hold a reference to (the same match as the Hierarchy badge's
    /// extension and the Data foldout). Not a Savable Bridge's own saved scripts: those already have their own
    /// cards.
    /// </summary>
    [InitializeOnLoad]
    internal static class HeldSavedObjectsHeader
    {
        static bool showData;

        static HeldSavedObjectsHeader()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedDefaultHeaderGUI;
        }

        static void OnFinishedDefaultHeaderGUI(UnityEditor.Editor editor)
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            // Only the GameObject's own header draws this, once, not every component's header on it.
            GameObject gameObject = editor.target as GameObject;
            if (gameObject == null)
            {
                return;
            }

            List<SavedObjectEntry> held = HeldEntriesOn(gameObject);
            if (held.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            showData = EditorGUILayout.Foldout(showData, $"Held saved objects ({held.Count})", true);
            if (!showData)
            {
                return;
            }

            SaveJson serializer = new SaveJson();
            foreach (SavedObjectEntry entry in held)
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(entry.SavedObject.GetType().Name), LiveValueText(entry, serializer));
            }
        }

        // A field holding an unassigned Unity object reference can throw when touched; that is treated as
        // "nothing held", never allowed to break the inspector.
        static List<SavedObjectEntry> HeldEntriesOn(GameObject gameObject)
        {
            try
            {
                IEnumerable<SavedObjectEntry> allEntries = SaveActions.Session?.SavedObjects.AllEntries;
                return allEntries == null ? new List<SavedObjectEntry>() : HeldSavedObjectMatcher.HeldBy(gameObject, allEntries.ToList());
            }
            catch (Exception exception) when (exception is UnassignedReferenceException || exception is MissingReferenceException || exception is InvalidOperationException)
            {
                return new List<SavedObjectEntry>();
            }
        }

        static string LiveValueText(SavedObjectEntry entry, SaveJson serializer)
        {
            try
            {
                object captured = entry.Capture();
                JToken token = captured as JToken ?? serializer.ToToken(captured);
                return token.ToString(Newtonsoft.Json.Formatting.None);
            }
            catch (Exception exception)
            {
                return $"Couldn't read the values: {exception.Message}";
            }
        }
    }
}
