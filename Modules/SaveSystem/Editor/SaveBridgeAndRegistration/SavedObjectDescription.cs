using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Everything one SavedObjectCard needs to draw: built here from a Savable Bridge's saved script (T1), or in
    /// phase T5 from a registered saved object. Plain data; SavedObjectCard turns it into a VisualElement.
    /// </summary>
    internal sealed class SavedObjectDescription
    {
        /// <summary>One [Save] field's name, short type name and former name, for a field-row card.</summary>
        public readonly struct FieldRow
        {
            public readonly string Name;
            public readonly string ShortTypeName;
            public readonly string FormerlyName;
            public readonly FieldInfo Field;

            public FieldRow(string name, string shortTypeName, string formerlyName, FieldInfo field)
            {
                Name = name;
                ShortTypeName = shortTypeName;
                FormerlyName = formerlyName;
                Field = field;
            }
        }

        public string HeaderText;
        public string ViewDataKey;

        /// <summary>Non-null for a [Save]-field script. Null for an ISavable script.</summary>
        public IReadOnlyList<FieldRow> FieldRows;

        /// <summary>Non-null for an ISavable script: the TData type name. Null for a [Save]-field script.</summary>
        public string CustomDataTypeName;

        /// <summary>Reads the live values. Play mode only; may throw, which the card catches and reports.</summary>
        public Func<JToken> CaptureLiveValues;

        public Func<string> GetSaveId;
        public bool IsWaitingForSetSaveId;
        public bool ShowsSpawnedCopyNote;

        /// <summary>Builds the description for one saved script on a Savable Bridge's GameObject.</summary>
        public static SavedObjectDescription ForBridgeTarget(SavableBridge bridge, SavableTarget target, string numberSuffix, SaveJson serializer)
        {
            Type scriptType = target.Script.GetType();
            Type saveFileClass = SavedScript.SaveFileClassOf(scriptType);
            string saveFileName = saveFileClass != null ? saveFileClass.Name : "GameProfile";
            string scriptName = target.Identification.ClassName + numberSuffix;

            SavedObjectDescription description = new SavedObjectDescription
            {
                ViewDataKey = target.Identification.GUID,
                GetSaveId = () => bridge.GetSaveId(target),
                IsWaitingForSetSaveId = bridge.IsWaitingForSetSaveId,
                ShowsSpawnedCopyNote = IsPrefabAssetOrPrefabMode(bridge.gameObject)
            };

            Type savableInterface = SavedScript.FindSavableInterface(scriptType);
            if (savableInterface != null)
            {
                MethodInfo captureMethod = savableInterface.GetMethod("CaptureData");
                description.CustomDataTypeName = savableInterface.GetGenericArguments()[0].Name;
                description.HeaderText = $"{scriptName} · {saveFileName} · custom data";
                description.CaptureLiveValues = () => serializer.ToToken(InvokeCaptureData(captureMethod, target.Script));
            }
            else
            {
                SavedFieldSet fields = new SavedFieldSet(scriptType);
                description.FieldRows = fields.Fields.Select(ToFieldRow).ToList();
                description.HeaderText = $"{scriptName} · {saveFileName} · {DescribeFieldCount(fields)}";
                description.CaptureLiveValues = () => fields.Capture(target.Script, serializer);
            }

            return description;
        }

        static FieldRow ToFieldRow(SavedFieldSet.SavedField field)
        {
            return new FieldRow(field.Key, ShortTypeName(field.Field.FieldType), field.FormerlyKey, field.Field);
        }

        static string DescribeFieldCount(SavedFieldSet fields)
        {
            int count = fields.Fields.Count;
            return count == 1 ? "1 field" : $"{count} fields";
        }

        static bool IsPrefabAssetOrPrefabMode(GameObject gameObject)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return true;
            }
            PrefabStage currentStage = PrefabStageUtility.GetCurrentPrefabStage();
            return currentStage != null && currentStage.IsPartOfPrefabContents(gameObject);
        }

        // Unwraps the reflection exception, so the card reports the script's own exception message.
        static object InvokeCaptureData(MethodInfo captureMethod, object script)
        {
            try
            {
                return captureMethod.Invoke(script, null);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        /// <summary>A short, readable type name: "int", "List&lt;string&gt;", "Vector3".</summary>
        internal static string ShortTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(long)) return "long";
            if (type.IsArray) return $"{ShortTypeName(type.GetElementType())}[]";
            if (!type.IsGenericType) return type.Name;

            string genericArguments = string.Join(", ", type.GetGenericArguments().Select(ShortTypeName));
            string nameWithoutArity = type.Name.Substring(0, type.Name.IndexOf('`'));
            return $"{nameWithoutArity}<{genericArguments}>";
        }
    }
}
