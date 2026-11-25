using AbstractPixel.EditorModules.Documentation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Which inline code names a documented type or one of its members, so it can link to the type's page. The
    /// documented types are the ones with a page under types/, found by that page's members line. Built once per
    /// domain load.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsTypeIndex
    {
        public const string Scheme = "type:";
        private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static readonly Regex MembersLine = new Regex(@"<!--\s*members\s+(\S+)\s*-->");
        private static readonly Regex Identifier = new Regex(@"^[A-Za-z_]\w*$");
        private static Dictionary<string, Type> types;

        /// <summary>The documented type with this short name ("SceneActions"), or null.</summary>
        public static Type TypeNamed(string _shortName)
        {
            Type type;
            return Types().TryGetValue(_shortName ?? "", out type) ? type : null;
        }

        /// <summary>
        /// The link target for a code span: "type:SceneActions" or "type:SceneActions.LoadingProgress", or null when it
        /// names nothing documented. A call's arguments are ignored ("SceneActions.ChangeTo(group)"). A bare member
        /// name links when <paramref name="_currentType"/> (the type page it is written on) has that member.
        /// </summary>
        public static string TargetFor(string _code, string _currentType = null)
        {
            string code = (_code ?? "").Trim();
            int call = code.IndexOf('(');
            if (call >= 0)
            {
                code = code.Substring(0, call);
            }
            string[] parts = code.Split('.');
            foreach (string part in parts)
            {
                if (!Identifier.IsMatch(part))
                {
                    return null;
                }
            }
            string last = parts[parts.Length - 1];
            if (TypeNamed(last) != null)
            {
                return Scheme + last;
            }
            string owner = parts.Length >= 2 ? parts[parts.Length - 2] : _currentType;
            Type ownerType = TypeNamed(owner);
            if (ownerType == null || !HasMember(ownerType, last))
            {
                return null;
            }
            return Scheme + owner + "." + last;
        }

        /// <summary>Splits "type:SceneActions.LoadingProgress" into its type and member (empty when none).</summary>
        public static void Split(string _target, out string _typeName, out string _memberName)
        {
            string body = _target.StartsWith(Scheme) ? _target.Substring(Scheme.Length) : _target;
            int dot = body.IndexOf('.');
            _typeName = dot < 0 ? body : body.Substring(0, dot);
            _memberName = dot < 0 ? "" : body.Substring(dot + 1);
        }

        private static bool HasMember(Type _type, string _name)
        {
            if (_type.IsEnum)
            {
                return Array.IndexOf(Enum.GetNames(_type), _name) >= 0;
            }
            return _type.GetMember(_name, AnyMember).Length > 0;
        }

        private static Dictionary<string, Type> Types()
        {
            if (types != null)
            {
                return types;
            }
            types = new Dictionary<string, Type>();
            foreach (string path in SceneDocsTopics.AllPagePaths())
            {
                bool isTypePage = path.StartsWith(SceneDocsPaths.Pages + "types/");
                if (!isTypePage)
                {
                    continue;
                }
                Match line = MembersLine.Match(DocsFiles.Read(path) ?? "");
                Type type = line.Success ? DocsTypeFinder.FindType(line.Groups[1].Value) : null;
                if (type != null)
                {
                    types[type.Name] = type;
                }
            }
            return types;
        }
    }
}
