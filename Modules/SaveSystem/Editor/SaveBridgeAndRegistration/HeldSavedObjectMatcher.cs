using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Finds which registered saved objects a GameObject's components hold, one level deep: an instance field
    /// (public or private, auto-property backing fields included), or an element of an array or List&lt;&gt;. Used by
    /// the Hierarchy badge (a GameObject with no bridge can still hold one) and the inspector's Held Saved Objects
    /// header.
    /// </summary>
    internal static class HeldSavedObjectMatcher
    {
        const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        /// <summary>Every entry among <paramref name="entries"/> that a component on this GameObject holds a reference to.</summary>
        public static List<SavedObjectEntry> HeldBy(GameObject gameObject, IEnumerable<SavedObjectEntry> entries)
        {
            List<SavedObjectEntry> candidates = entries as List<SavedObjectEntry> ?? entries.ToList();
            List<SavedObjectEntry> held = new List<SavedObjectEntry>();
            if (candidates.Count == 0)
            {
                return held;
            }

            foreach (MonoBehaviour component in gameObject.GetComponents<MonoBehaviour>())
            {
                if (component == null)
                {
                    continue;
                }
                foreach (object value in InstanceValuesOf(component))
                {
                    SavedObjectEntry match = candidates.Find(entry => ReferenceEquals(entry.SavedObject, value));
                    if (match != null && !held.Contains(match))
                    {
                        held.Add(match);
                    }
                }
            }
            return held;
        }

        // Every instance field's value, plus each element of an array or List<> field. Stops at MonoBehaviour: its
        // own base classes hold Unity's internals, not anything a game registered.
        static IEnumerable<object> InstanceValuesOf(object instance)
        {
            for (Type type = instance.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(InstanceFields))
                {
                    object value;
                    try
                    {
                        value = field.GetValue(instance);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        continue;
                    }

                    // IList, not the broader IEnumerable: a Transform is IEnumerable too (it iterates its
                    // children), and asking an unassigned one for its children throws UnassignedReferenceException.
                    // Only a real List<> or array field is walked as a collection.
                    if (value is IList elements)
                    {
                        foreach (object element in elements)
                        {
                            if (element != null)
                            {
                                yield return element;
                            }
                        }
                    }
                    else
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}
