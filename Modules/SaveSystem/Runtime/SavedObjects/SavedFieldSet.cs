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
        sealed class SavedField
        {
            public FieldInfo Field;
            public string Key;
            public string FormerlyKey;
        }

        readonly List<SavedField> fields = new List<SavedField>();

        public SavedFieldSet(Type savedType)
        {
            const BindingFlags declaredInstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type type = savedType; type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(declaredInstanceFields))
                {
                    SaveAttribute saved = field.GetCustomAttribute<SaveAttribute>();
                    if (saved != null)
                    {
                        fields.Add(new SavedField { Field = field, Key = field.Name, FormerlyKey = saved.Formerly });
                    }
                }
            }
        }

        /// <summary>True when the class has at least one [Save] field.</summary>
        public bool HasAnyField => fields.Count > 0;

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
        public void Restore(object instance, JObject data, SaveJson serializer)
        {
            foreach (SavedField field in fields)
            {
                if (!TryFindToken(data, field, out JToken token))
                {
                    continue;
                }
                object value = token.Type == JTokenType.Null ? null : serializer.FromToken(token, field.Field.FieldType);
                field.Field.SetValue(instance, value);
            }
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
