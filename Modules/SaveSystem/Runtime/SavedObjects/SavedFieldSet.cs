using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Every <see cref="SaveAttribute"/> field on one class, public or private, inherited ones included. Captures
    /// them into a JSON object keyed by field name, and restores them back. Built once per registration and held
    /// by the entry's capture and restore delegates, never cached in a static.
    /// </summary>
    internal sealed class SavedFieldSet
    {
        /// <summary>One reflected [Save] field: the field itself, the key its data is stored under, and its former key.</summary>
        internal readonly struct SavedField
        {
            public readonly FieldInfo Field;
            public readonly string Key;
            public readonly string FormerlyKey;

            public SavedField(FieldInfo field, string key, string formerlyKey)
            {
                Field = field;
                Key = key;
                FormerlyKey = formerlyKey;
            }
        }

        readonly List<SavedField> fields = new List<SavedField>();

        /// <summary>One error per invalid [Save] field found while building this set, naming the type, the field and the fix.</summary>
        public List<string> Problems { get; } = new List<string>();

        /// <summary>True when every [Save] field on the class is valid. An invalid field is left out of Capture and Restore.</summary>
        public bool IsValid => Problems.Count == 0;

        /// <summary>True when the class has at least one [Save] field, valid or not. SavedScript.IsSaved asks this.</summary>
        public bool HasAnyField => fields.Count > 0 || Problems.Count > 0;

        /// <summary>Every valid field, for the bridge inspector's card. Built once in the constructor; never reflected twice.</summary>
        public IReadOnlyList<SavedField> Fields => fields;

        public SavedFieldSet(Type savedType)
        {
            const BindingFlags declaredInstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type type = savedType; type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(declaredInstanceFields))
                {
                    SaveAttribute saved = field.GetCustomAttribute<SaveAttribute>();
                    if (saved == null)
                    {
                        continue;
                    }

                    string problem = SavedFieldRules.ProblemWith(field);
                    if (problem != null)
                    {
                        Problems.Add(problem);
                        continue;
                    }
                    fields.Add(new SavedField(field, field.Name, saved.Formerly));
                }
            }
        }

        /// <summary>Reads every field's current value into a JSON object keyed by field name.</summary>
        public JObject Capture(object instance, SaveJson serializer)
        {
            JObject data = new JObject();
            foreach (SavedField field in fields)
            {
                object value = field.Field.GetValue(instance);
                data[field.Key] = value == null ? JValue.CreateNull() : serializer.ToToken(value);
            }
            return data;
        }

        /// <summary>
        /// Writes every field this data has a value for. A field missing from the data (new, or stored under its
        /// Formerly name) keeps its current value, so a new field never breaks an old save.
        /// </summary>
        public void Restore(object instance, JToken data, SaveJson serializer)
        {
            JObject valuesByFieldName = data as JObject;
            if (valuesByFieldName == null)
            {
                RestoreSingleValue(instance, data, serializer);
                return;
            }
            foreach (SavedField field in fields)
            {
                if (!TryFindToken(valuesByFieldName, field, out JToken token))
                {
                    continue;
                }
                SetFromToken(instance, field, token, serializer);
            }
        }

        // An ISavable<int> (or any single-value ISavable) saved a bare value under this save ID. A class switched to
        // [Save] fields can read it only when exactly one field could have meant it; anything else would be a guess.
        void RestoreSingleValue(object instance, JToken data, SaveJson serializer)
        {
            if (fields.Count != 1)
            {
                throw new InvalidOperationException($"The saved data is a single value, the way an ISavable<T> saves it, but {instance.GetType().Name} has {fields.Count} [Save] fields, so there is no telling which one it belongs to. To keep players' saved data, give {instance.GetType().Name} exactly one [Save] field until their saves have been written again, or go back to ISavable<T>.");
            }
            SetFromToken(instance, fields[0], data, serializer);
        }

        static void SetFromToken(object instance, SavedField field, JToken token, SaveJson serializer)
        {
            object value = token.Type == JTokenType.Null ? null : serializer.FromToken(token, field.Field.FieldType);
            field.Field.SetValue(instance, value);
        }

        static bool TryFindToken(JObject data, SavedField field, out JToken token)
        {
            if (data.TryGetValue(field.Key, out token))
            {
                return true;
            }
            return field.FormerlyKey != null && data.TryGetValue(field.FormerlyKey, out token);
        }
    }
}
