using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Every setup problem the save system can find right now, merged from the checks that used to run on their
    /// own: an invalid [Save] field, [Save] fields mixed with ISavable, a save file with no config row, a stale
    /// GameSaveFiles.cs, and a saved script with no Savable Bridge in an open scene. SetupPlayModeGuard blocks play
    /// mode on these; the Hierarchy badge and SetupProblemsWindow read them too. Cached until something that could
    /// change the answer happens.
    /// </summary>
    [InitializeOnLoad]
    internal static class SetupCheck
    {
        static List<SetupProblem> cachedProblems;

        static SetupCheck()
        {
            EditorApplication.hierarchyChanged += Forget;
            ObjectChangeEvents.changesPublished += OnObjectChanged;
        }

        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            Forget();
        }

        internal static IReadOnlyList<SetupProblem> Problems
        {
            get
            {
                if (cachedProblems == null)
                {
                    cachedProblems = Compute();
                }
                return cachedProblems;
            }
        }

        /// <summary>Every problem on this GameObject's own saved scripts, for the Hierarchy badge.</summary>
        internal static IReadOnlyList<SetupProblem> ProblemsOn(GameObject gameObject)
        {
            return Problems.Where(problem => IsOn(problem, gameObject)).ToList();
        }

        /// <summary>Clears the cache, so the next read computes it again. The config inspector's own edits reach this through ObjectChangeEvents.</summary>
        internal static void Forget()
        {
            cachedProblems = null;
        }

        static bool IsOn(SetupProblem problem, GameObject gameObject)
        {
            if (ReferenceEquals(problem.Target, gameObject))
            {
                return true;
            }
            bool scriptTypeIsAComponent = problem.ScriptType != null && typeof(Component).IsAssignableFrom(problem.ScriptType);
            return scriptTypeIsAComponent && gameObject.GetComponent(problem.ScriptType) != null;
        }

        static void OnObjectChanged(ref ObjectChangeEventStream stream)
        {
            Forget();
        }

        static List<SetupProblem> Compute()
        {
            List<SetupProblem> problems = new List<SetupProblem>();
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();

            problems.AddRange(ProblemsFromFields(TypeCache.GetFieldsWithAttribute<SaveAttribute>()));

            IEnumerable<Type> savedTypes = TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .Where(type => !IsTestAssembly(type) && SavedScript.IsSaved(type));
            problems.AddRange(MissingConfigRowProblems(savedTypes, config));

            SetupProblem staleGameSaveFiles = StaleGameSaveFilesProblemOrNull(config);
            if (staleGameSaveFiles != null)
            {
                problems.Add(staleGameSaveFiles);
            }

            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                problems.AddRange(MissingBridgeProblems(SceneManager.GetSceneAt(index)));
            }

            return problems;
        }

        // Every method below takes its inputs as parameters instead of reading the project's real TypeCache, config
        // or scenes, so a test can hand it fabricated ones without touching global state other tests also run on.

        internal static List<SetupProblem> ProblemsFromFields(IEnumerable<FieldInfo> saveAttributeFields)
        {
            List<SetupProblem> problems = new List<SetupProblem>();
            HashSet<Type> classesWithSaveAttributeFields = new HashSet<Type>();

            foreach (FieldInfo field in saveAttributeFields)
            {
                classesWithSaveAttributeFields.Add(field.DeclaringType);
                string problem = SavedFieldRules.ProblemWith(field);
                if (problem != null)
                {
                    problems.Add(ForScriptType(field.DeclaringType, problem));
                }
            }

            foreach (Type savedClass in classesWithSaveAttributeFields)
            {
                string problem = SavedFieldRules.ProblemWithClass(savedClass);
                if (problem != null)
                {
                    problems.Add(ForScriptType(savedClass, problem));
                }
            }

            return problems;
        }

        internal static List<SetupProblem> MissingConfigRowProblems(IEnumerable<Type> savedTypes, SaveSystemConfigSO config)
        {
            List<SetupProblem> problems = new List<SetupProblem>();
            if (config == null)
            {
                return problems;
            }

            foreach (Type savedType in savedTypes)
            {
                string problem = SaveFileRowCheck.ProblemWith(savedType, config);
                if (problem != null)
                {
                    problems.Add(ForScriptType(savedType, problem));
                }
            }
            return problems;
        }

        internal static SetupProblem StaleGameSaveFilesProblemOrNull(SaveSystemConfigSO config)
        {
            if (config == null || !SaveFileListStatus.IsOutOfDate(config))
            {
                return null;
            }
            (string whatIsWrong, string howToFix) = Split(SaveFileListStatus.OutOfDateMessage);
            return new SetupProblem(null, config, whatIsWrong, howToFix);
        }

        internal static List<SetupProblem> MissingBridgeProblems(Scene scene)
        {
            List<SetupProblem> problems = new List<SetupProblem>();
            if (!scene.isLoaded)
            {
                return problems;
            }

            List<GameObject> missing = SavableBridgeSceneCheck.FindMissingBridges(scene);
            if (missing.Count == 0)
            {
                return problems;
            }

            string message = SavableBridgeSceneCheck.Describe(scene, missing, "enter play mode again");
            (string whatIsWrong, string howToFix) = Split(message);
            foreach (GameObject gameObject in missing)
            {
                Type scriptType = gameObject.GetComponents<MonoBehaviour>()
                    .FirstOrDefault(script => script != null && SavedScript.IsSaved(script.GetType()))
                    ?.GetType();
                problems.Add(new SetupProblem(scriptType, gameObject, whatIsWrong, howToFix));
            }
            return problems;
        }

        static bool IsTestAssembly(Type type)
        {
            return type.Assembly.GetName().Name.EndsWith("Tests", StringComparison.Ordinal);
        }

        static SetupProblem ForScriptType(Type scriptType, string message)
        {
            (string whatIsWrong, string howToFix) = Split(message);
            return new SetupProblem(scriptType, FindScriptAsset(scriptType), whatIsWrong, howToFix);
        }

        /// <summary>The MonoScript asset for a saved script's class, for a "open the script" button. Null if none is found.</summary>
        internal static UnityEngine.Object FindScriptAsset(Type scriptType)
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:MonoScript {scriptType.Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == scriptType)
                {
                    return script;
                }
            }
            return null;
        }

        // Every message these checks return ends with its fix as the last sentence, so splitting there separates
        // what is wrong from how to fix it without the checks needing to say so twice.
        static (string WhatIsWrong, string HowToFix) Split(string message)
        {
            const string prefix = "[SaveSystem] ";
            string trimmed = message.StartsWith(prefix, StringComparison.Ordinal) ? message.Substring(prefix.Length) : message;
            int splitIndex = trimmed.LastIndexOf(". ", StringComparison.Ordinal);
            if (splitIndex < 0)
            {
                return (trimmed, string.Empty);
            }
            return (trimmed.Substring(0, splitIndex + 1), trimmed.Substring(splitIndex + 2));
        }
    }
}
