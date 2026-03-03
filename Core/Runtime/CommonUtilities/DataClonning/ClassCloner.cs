using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>Provides functionality to clone instances of classes that implement the ICopyable interface.</summary>
    /// <remarks>ClassCloner is commonly used to create deep copies of objects that implement the ICopyable interface,
    /// allowing for safe duplication of objects without affecting the original instance. This is particularly useful
    /// in scenarios where modifications to a copy should not impact the original object.</remarks>
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
