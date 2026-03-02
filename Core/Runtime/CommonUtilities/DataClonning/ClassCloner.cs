using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    public static class ClassCloner
    {
        // Constraint: T must be a class, have a default constructor, and implement ICopyable
        public static T CloneClass<T>(T original) where T : class, ICopyable<T>, new()
        {
            if (original == null)
            {
                Debug.LogError("Original object is null. Cannot clone.");
                return null;
            }
            T newCopy = new T();

            // 2. JSON Magic: Copies Value Types (Curves, Colors, Ints, Strings)
            // JsonUtility naturally ignores Unity Objects (Transforms, MonoBehaviours), so it won't break the refs.
            string jsonSnapshot = JsonUtility.ToJson(original);
            JsonUtility.FromJsonOverwrite(jsonSnapshot, newCopy);

            // 3. Interface Magic: Manually copy the Scene References
            newCopy.CopyReferencesFrom(original);

            return newCopy;
        }
    }
}
