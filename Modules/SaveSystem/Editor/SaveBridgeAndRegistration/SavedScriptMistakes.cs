using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// The mistakes one saved script's class can show on its card: an invalid [Save] field, [Save] fields mixed
    /// with ISavable, or a save file with no row. Shared by SavedObjectDescription (T2's HelpBoxes) and the
    /// Hierarchy badge's "does this GameObject have a mistake" query (T4).
    /// </summary>
    internal static class SavedScriptMistakes
    {
        /// <summary>Every blocking mistake on this class: the field's own fieldSet, already reflected once by the caller.</summary>
        public static IReadOnlyList<string> BlockingMistakesOf(Type scriptType, SavedFieldSet fields)
        {
            List<string> mistakes = new List<string>(fields.Problems);
            if (fields.HasAnyField)
            {
                string classProblem = SavedFieldRules.ProblemWithClass(scriptType);
                if (classProblem != null)
                {
                    mistakes.Add(classProblem);
                }
            }
            return mistakes;
        }

        /// <summary>True when any saved script on this GameObject has a blocking mistake or a save file with no row.</summary>
        public static bool AnyMistakeOn(GameObject gameObject)
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            foreach (MonoBehaviour script in gameObject.GetComponents<MonoBehaviour>())
            {
                if (script == null)
                {
                    continue;
                }
                Type scriptType = script.GetType();
                if (!SavedScript.IsSaved(scriptType))
                {
                    continue;
                }

                SavedFieldSet fields = new SavedFieldSet(scriptType);
                if (BlockingMistakesOf(scriptType, fields).Count > 0)
                {
                    return true;
                }
                if (SaveFileRowCheck.ProblemWith(scriptType, config) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
