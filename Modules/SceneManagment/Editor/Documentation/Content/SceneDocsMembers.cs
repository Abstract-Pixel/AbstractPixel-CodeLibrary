using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The member list of a type page, built by reflection: public members declared by the type, grouped by kind and
    /// sorted by name, each with its kind badge and the /// summary line read from the source file.
    /// </summary>
    public static class SceneDocsMembers
    {
        private const BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>The type with this full name in any loaded assembly, or null.</summary>
        public static Type FindType(string _fullName)
        {
            // The project's own assemblies, as Unity lists them (AppDomain can list assemblies already unloaded).
            foreach (UnityEditor.Compilation.Assembly assembly in UnityEditor.Compilation.CompilationPipeline.GetAssemblies())
            {
                Type type = Type.GetType(_fullName + ", " + assembly.name, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>Kind word for a member ("method", "property", "event", "field", "value").</summary>
        public static List<KeyValuePair<string, string>> MembersOf(Type _type)
        {
            List<KeyValuePair<string, string>> members = new List<KeyValuePair<string, string>>();
            if (_type.IsEnum)
            {
                foreach (string name in Enum.GetNames(_type))
                {
                    members.Add(new KeyValuePair<string, string>("value", name));
                }
                return members;
            }
            foreach (PropertyInfo property in _type.GetProperties(Declared))
            {
                members.Add(new KeyValuePair<string, string>("property", property.Name));
            }
            foreach (EventInfo item in _type.GetEvents(Declared))
            {
                members.Add(new KeyValuePair<string, string>("event", item.Name));
            }
            foreach (FieldInfo field in _type.GetFields(Declared))
            {
                members.Add(new KeyValuePair<string, string>("field", field.Name));
            }
            HashSet<string> seenMethods = new HashSet<string>();
            foreach (MethodInfo method in _type.GetMethods(Declared))
            {
                bool isPlain = !method.IsSpecialName && seenMethods.Add(method.Name);
                if (isPlain)
                {
                    members.Add(new KeyValuePair<string, string>("method", method.Name));
                }
            }
            members.Sort((_a, _b) => string.CompareOrdinal(_a.Key + _a.Value, _b.Key + _b.Value));
            return members;
        }
    }
}
