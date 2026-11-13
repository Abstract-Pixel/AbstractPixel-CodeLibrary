using System;
using System.Reflection;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The mistakes a [Save] field or a saved class can make, each with its exact fix. Shared by the editor's
    /// after-compile scan, the play mode guard, the build check and the runtime safety net, so all four report
    /// the same thing the same way.
    /// </summary>
    internal static class SavedFieldRules
    {
        /// <summary>Null when the field is fine to save. Otherwise the error naming the type, the field and the fix.</summary>
        public static string ProblemWith(FieldInfo field)
        {
            string ownerAndField = $"{field.DeclaringType.Name}.{field.Name}";

            if (field.IsStatic || field.IsInitOnly)
            {
                return $"[SaveSystem] {ownerAndField} is marked [Save] but is static, const or readonly, so it can't hold its own value per instance. Remove [Save] from it.";
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
            {
                return $"[SaveSystem] {ownerAndField} is marked [Save] but holds a {field.FieldType.Name}, which can't be saved. Save something that finds it again instead, such as its name or save ID.";
            }
            if (CannotBeWrittenAndReadBack(field.FieldType))
            {
                return $"[SaveSystem] {ownerAndField} is marked [Save] but its type ({field.FieldType.Name}) can't be written and read back. Save plain data instead: numbers, text, bools, enums, Vector2, Vector3, Quaternion, Color, lists, arrays, dictionaries or your own [Serializable] class.";
            }
            return null;
        }

        /// <summary>Null when the class is fine. Otherwise the error for a class with both [Save] fields and ISavable.</summary>
        public static string ProblemWithClass(Type savedClass)
        {
            if (SavedScript.IsSaved(savedClass))
            {
                return $"[SaveSystem] {savedClass.Name} has both [Save] fields and ISavable. Pick one: [Save] fields for plain data, or ISavable<TData> for data that needs its own CaptureData and RestoreData.";
            }
            return null;
        }

        static bool CannotBeWrittenAndReadBack(Type fieldType)
        {
            return typeof(Delegate).IsAssignableFrom(fieldType)
                || fieldType.IsInterface
                || fieldType.IsAbstract
                || fieldType == typeof(object);
        }
    }
}
