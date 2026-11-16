using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Scans every [Save] field in the project after each compile and logs one error per mistake, naming the
    /// type, the field and the fix. The play mode guard and the build check both read HasProblems from here,
    /// so a mistake is reported once and blocks both the same way.
    /// </summary>
    [InitializeOnLoad]
    public static class SavedFieldScan
    {
        public static bool HasProblems { get; private set; }

        static SavedFieldScan()
        {
            Rescan();
        }

        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            Rescan();
        }

        static void Rescan()
        {
            HasProblems = Scan(TypeCache.GetFieldsWithAttribute<SaveAttribute>());
        }

        /// <summary>
        /// Logs one error per mistake among the given fields and their declaring classes, and returns true if it
        /// found any. Takes the field list as a parameter so a test can hand it fabricated fields, instead of
        /// compiling a broken [Save] field into the project just to see this scan report it.
        /// </summary>
        internal static bool Scan(IEnumerable<FieldInfo> saveAttributeFields)
        {
            bool foundAProblem = false;
            HashSet<Type> classesWithSaveAttributeFields = new HashSet<Type>();

            foreach (FieldInfo field in saveAttributeFields)
            {
                classesWithSaveAttributeFields.Add(field.DeclaringType);
                string problem = SavedFieldRules.ProblemWith(field);
                if (problem != null)
                {
                    LogProblem(problem);
                    foundAProblem = true;
                }
            }

            foreach (Type savedClass in classesWithSaveAttributeFields)
            {
                string problem = SavedFieldRules.ProblemWithClass(savedClass);
                if (problem != null)
                {
                    LogProblem(problem);
                    foundAProblem = true;
                }
            }

            return foundAProblem;
        }

        static void LogProblem(string problem)
        {
            SaveLog.Error($"{problem} Play mode won't start, and builds will fail, until this is fixed.");
        }
    }
}
