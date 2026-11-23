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

        public static VisualElement Render(string _fullName)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-members");
            Type type = FindType(_fullName);
            if (type == null)
            {
                box.Add(new Label("No loaded type named " + _fullName));
                return box;
            }
            string file = SceneDocsSource.FileDeclaring(type.Name);
            List<KeyValuePair<string, string>> members = MembersOf(type);
            if (members.Count == 0)
            {
                Label none = new Label("No public members of its own.");
                none.AddToClassList("docs-text");
                box.Add(none);
                return box;
            }
            foreach (KeyValuePair<string, string> member in members)
            {
                box.Add(MemberRow(member.Key, member.Value, file));
            }
            return box;
        }

        private static VisualElement MemberRow(string _kind, string _name, string _file)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-member");
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-member-header");
            header.Add(DocsPill.Kind(_kind));
            Label name = new Label(_name);
            name.AddToClassList("docs-member-name");
            name.selection.isSelectable = true;
            DocsCodeBlock.ApplyMonoFont(name);
            header.Add(name);
            row.Add(header);
            string summary = _file == null ? "" : SceneDocsSource.SummaryAbove(_file, _name);
            if (summary.Length > 0)
            {
                Label text = new Label(summary);
                text.AddToClassList("docs-text");
                text.AddToClassList("docs-member-summary");
                text.selection.isSelectable = true;
                row.Add(text);
            }
            return row;
        }
    }
}
