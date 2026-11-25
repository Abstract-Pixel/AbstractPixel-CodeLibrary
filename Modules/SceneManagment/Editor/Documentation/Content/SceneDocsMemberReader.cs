using AbstractPixel.EditorModules.Documentation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Reads a type's public and protected members for its member tables: one row per overload, with modifiers taken
    /// from the source line when it can be found (so they read exactly as written) and from reflection otherwise.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsMemberReader
    {
        private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static readonly string[] KindOrder = { "property", "field", "method", "event", "value" };

        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
        {
            { typeof(void), "void" }, { typeof(bool), "bool" }, { typeof(int), "int" }, { typeof(float), "float" },
            { typeof(double), "double" }, { typeof(long), "long" }, { typeof(string), "string" }, { typeof(object), "object" },
            { typeof(byte), "byte" }, { typeof(char), "char" }, { typeof(uint), "uint" }, { typeof(short), "short" }
        };

        /// <summary>Every member a game can reach, sorted by kind (properties, fields, methods, events) and then by name.</summary>
        public static List<SceneDocsMember> Read(Type _type)
        {
            List<SceneDocsMember> members = new List<SceneDocsMember>();
            if (_type.IsEnum)
            {
                AddEnumValues(_type, members);
                return members;
            }
            foreach (PropertyInfo property in _type.GetProperties(Declared))
            {
                AddIfVisible(members, Property(property), Visible(property.GetMethod) || Visible(property.SetMethod));
            }
            foreach (FieldInfo field in _type.GetFields(Declared))
            {
                bool isVisible = (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly) && !field.IsSpecialName && !field.Name.Contains("<");
                AddIfVisible(members, Field(field), isVisible);
            }
            foreach (MethodInfo method in _type.GetMethods(Declared))
            {
                AddIfVisible(members, Method(method), Visible(method) && !method.IsSpecialName);
            }
            foreach (EventInfo item in _type.GetEvents(Declared))
            {
                AddIfVisible(members, Event(item), Visible(item.AddMethod));
            }
            ReadSource(_type, members);
            members.Sort(Compare);
            return members;
        }

        private static void AddIfVisible(List<SceneDocsMember> _members, SceneDocsMember _member, bool _isVisible)
        {
            if (_isVisible)
            {
                _members.Add(_member);
            }
        }

        private static bool Visible(MethodBase _method)
        {
            return _method != null && (_method.IsPublic || _method.IsFamily || _method.IsFamilyOrAssembly);
        }

        private static int Compare(SceneDocsMember _a, SceneDocsMember _b)
        {
            int kind = Array.IndexOf(KindOrder, _a.Kind).CompareTo(Array.IndexOf(KindOrder, _b.Kind));
            return kind != 0 ? kind : string.CompareOrdinal(_a.Name, _b.Name);
        }

        private static SceneDocsMember Property(PropertyInfo _property)
        {
            SceneDocsMember member = new SceneDocsMember("property", _property.Name);
            member.TypeText = FriendlyName(_property.PropertyType);
            // A property with a hidden getter and no setter still needs an accessor to read its modifiers from.
            MethodInfo widest = Visible(_property.GetMethod) ? _property.GetMethod : _property.SetMethod;
            if (widest == null)
            {
                widest = _property.GetMethod;
            }
            member.Modifiers.AddRange(MethodModifiers(widest));
            member.Accessors = Accessor("get", _property.GetMethod, widest) + Accessor("set", _property.SetMethod, widest);
            return member;
        }

        // "get; " or "private set; " when the accessor is less visible than the property; nothing when there is none.
        private static string Accessor(string _word, MethodInfo _accessor, MethodInfo _widest)
        {
            if (_accessor == null)
            {
                return "";
            }
            string access = AccessWord(_accessor);
            string prefix = access == AccessWord(_widest) ? "" : access + " ";
            string separator = _word == "set" ? " " : "";
            return separator + prefix + _word + ";";
        }

        private static SceneDocsMember Field(FieldInfo _field)
        {
            SceneDocsMember member = new SceneDocsMember("field", _field.Name);
            member.TypeText = FriendlyName(_field.FieldType);
            member.Modifiers.Add(_field.IsPublic ? "public" : "protected");
            if (_field.IsLiteral)
            {
                member.Modifiers.Add("const");
                return member;
            }
            if (_field.IsStatic)
            {
                member.Modifiers.Add("static");
            }
            if (_field.IsInitOnly)
            {
                member.Modifiers.Add("readonly");
            }
            return member;
        }

        private static SceneDocsMember Method(MethodInfo _method)
        {
            SceneDocsMember member = new SceneDocsMember("method", _method.Name);
            member.TypeText = FriendlyName(_method.ReturnType);
            member.Modifiers.AddRange(MethodModifiers(_method));
            List<string> parameters = new List<string>();
            foreach (ParameterInfo parameter in _method.GetParameters())
            {
                parameters.Add(ParameterText(parameter));
                member.ParameterNames.Add(parameter.Name);
            }
            member.Parameters = string.Join(", ", parameters);
            return member;
        }

        private static SceneDocsMember Event(EventInfo _event)
        {
            SceneDocsMember member = new SceneDocsMember("event", _event.Name);
            member.TypeText = FriendlyName(_event.EventHandlerType);
            member.Modifiers.AddRange(MethodModifiers(_event.AddMethod));
            return member;
        }

        private static void AddEnumValues(Type _type, List<SceneDocsMember> _members)
        {
            string[] lines = SceneDocsDeclarations.LinesOf(SceneDocsSource.FileDeclaring(_type.Name));
            foreach (string name in Enum.GetNames(_type))
            {
                SceneDocsMember member = new SceneDocsMember("value", name);
                int line = SceneDocsDeclarations.MemberLine(lines, name, null, true);
                if (line >= 0)
                {
                    string summary;
                    List<string> remarks;
                    SceneDocsDeclarations.CommentAbove(lines, line, out summary, out remarks);
                    member.Summary = summary;
                    member.Remarks.AddRange(remarks);
                }
                _members.Add(member);
            }
        }

        private static List<string> MethodModifiers(MethodInfo _method)
        {
            List<string> modifiers = new List<string>();
            modifiers.Add(AccessWord(_method));
            if (_method.IsStatic)
            {
                modifiers.Add("static");
            }
            bool inInterface = _method.DeclaringType != null && _method.DeclaringType.IsInterface;
            if (!inInterface && _method.IsAbstract)
            {
                modifiers.Add("abstract");
            }
            bool isOverridable = !inInterface && !_method.IsAbstract && _method.IsVirtual && !_method.IsFinal;
            if (isOverridable)
            {
                modifiers.Add(_method.GetBaseDefinition() != _method ? "override" : "virtual");
            }
            if (_method.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
            {
                modifiers.Add("async");
            }
            return modifiers;
        }

        private static string AccessWord(MethodBase _method)
        {
            if (_method.IsPublic)
            {
                return "public";
            }
            if (_method.IsFamilyOrAssembly)
            {
                return "protected internal";
            }
            if (_method.IsFamily)
            {
                return "protected";
            }
            return _method.IsAssembly ? "internal" : "private";
        }

        // Modifiers as written and the comment, from the line that declares each member.
        private static void ReadSource(Type _type, List<SceneDocsMember> _members)
        {
            string[] lines = SceneDocsDeclarations.LinesOf(SceneDocsSource.FileDeclaring(_type.Name));
            foreach (SceneDocsMember member in _members)
            {
                List<string> names = member.Kind == "method" ? member.ParameterNames : null;
                int line = SceneDocsDeclarations.MemberLine(lines, member.Name, names, _type.IsInterface);
                if (line < 0)
                {
                    continue;
                }
                List<string> written = SceneDocsDeclarations.ModifiersOn(lines[line]);
                if (written.Count > 0)
                {
                    member.Modifiers.Clear();
                    member.Modifiers.AddRange(written);
                }
                string summary;
                List<string> remarks;
                SceneDocsDeclarations.CommentAbove(lines, line, out summary, out remarks);
                member.Summary = summary;
                member.Remarks.AddRange(remarks);
            }
        }

        private static string ParameterText(ParameterInfo _parameter)
        {
            StringBuilder text = new StringBuilder();
            if (_parameter.IsOut)
            {
                text.Append("out ");
            }
            else if (_parameter.ParameterType.IsByRef)
            {
                text.Append("ref ");
            }
            if (_parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                text.Append("params ");
            }
            text.Append(FriendlyName(_parameter.ParameterType)).Append(' ').Append(_parameter.Name);
            if (_parameter.HasDefaultValue)
            {
                text.Append(" = ").Append(DefaultText(_parameter.DefaultValue));
            }
            return text.ToString();
        }

        private static string DefaultText(object _value)
        {
            if (_value == null)
            {
                return "null";
            }
            if (_value is string)
            {
                return "\"" + _value + "\"";
            }
            if (_value is bool)
            {
                return (bool)_value ? "true" : "false";
            }
            if (_value is Enum)
            {
                return _value.GetType().Name + "." + _value;
            }
            return Convert.ToString(_value, CultureInfo.InvariantCulture);
        }

        /// <summary>A type as C# writes it: "void", "Task&lt;SceneChangeResult&gt;", "string[]", "int?".</summary>
        public static string FriendlyName(Type _type)
        {
            if (_type.IsByRef)
            {
                return FriendlyName(_type.GetElementType());
            }
            string alias;
            if (Aliases.TryGetValue(_type, out alias))
            {
                return alias;
            }
            if (_type.IsArray)
            {
                return FriendlyName(_type.GetElementType()) + "[" + new string(',', _type.GetArrayRank() - 1) + "]";
            }
            Type nullable = Nullable.GetUnderlyingType(_type);
            if (nullable != null)
            {
                return FriendlyName(nullable) + "?";
            }
            if (!_type.IsGenericType)
            {
                return _type.Name;
            }
            string name = _type.Name.Substring(0, _type.Name.IndexOf('`'));
            List<string> arguments = new List<string>();
            foreach (Type argument in _type.GetGenericArguments())
            {
                arguments.Add(FriendlyName(argument));
            }
            return name + "<" + string.Join(", ", arguments) + ">";
        }
    }
}
