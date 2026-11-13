using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Finds the save file class a row stands for, so events can hand it out: <c>file == typeof(GameSaveFiles.Ghosts)</c>.
    /// It uses the class name and assembly the editor stored in the row. The cache belongs to one session, never a static.
    /// </summary>
    internal sealed class SaveFileClassFinder
    {
        readonly Dictionary<string, Type> classesByFullName = new Dictionary<string, Type>();
        readonly HashSet<string> reportedRows = new HashSet<string>();

        /// <summary>Remembers a class game code used, such as the TFile of Save&lt;TFile&gt;(), so it needs no lookup later.</summary>
        public void Remember(Type saveFileClass)
        {
            if (saveFileClass != null)
            {
                classesByFullName[saveFileClass.FullName] = saveFileClass;
            }
        }

        /// <summary>The row's class, or null if it can't be found. Logs one warning per row that can't be found.</summary>
        public Type Find(SaveFileDefinition definition)
        {
            if (string.IsNullOrEmpty(definition.ClassFullName))
            {
                return null;
            }
            if (classesByFullName.TryGetValue(definition.ClassFullName, out Type knownClass))
            {
                return knownClass;
            }

            // Built-in classes ship with the save system, so they never need a search.
            if (definition.IsBuiltIn)
            {
                Type builtInClass = SaveFileDefinition.ClassOf(definition.BuiltIn);
                classesByFullName[definition.ClassFullName] = builtInClass;
                return builtInClass;
            }

            string assemblyQualifiedName = $"{definition.ClassFullName}, {definition.ClassAssemblyName}";
            Type foundClass = Type.GetType(assemblyQualifiedName, false);
            if (foundClass != null)
            {
                classesByFullName[definition.ClassFullName] = foundClass;
                return foundClass;
            }

            bool isFirstReport = reportedRows.Add(definition.ClassFullName);
            if (isFirstReport)
            {
                Debug.LogWarning($"[SaveSystem] The save file '{definition.Name}' points at the class '{assemblyQualifiedName}', which can't be found, so its load and save events aren't raised. Select the save system config and press Write GameSaveFiles.cs.");
            }
            return null;
        }
    }
}
