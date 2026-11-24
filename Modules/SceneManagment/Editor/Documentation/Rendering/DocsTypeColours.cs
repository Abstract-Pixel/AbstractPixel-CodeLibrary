using System;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The colour for a type name by its kind (class, struct, enum, interface, delegate), as an IDE shows it.</summary>
    /// <remarks>
    /// - A name is looked up once in the library's namespaces and Unity's and .NET's common ones, then remembered.
    /// - An unknown name gets the class colour, the most common case.
    /// </remarks>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsTypeColours
    {
        private static readonly Dictionary<string, Type> found = new Dictionary<string, Type>();
        private static readonly string[] Namespaces =
        {
            "AbstractPixel.SceneManagement.", "AbstractPixel.SceneManagement.SceneScreens.", "AbstractPixel.SceneManagement.Editor.",
            "AbstractPixel.SceneTransitions.", "AbstractPixel.Core.", "AbstractPixel.Core.Editor.", ""
        };
        private static readonly Type[] AssemblyAnchors =
        {
            typeof(UnityEngine.Object), typeof(UnityEngine.UIElements.VisualElement), typeof(System.Threading.Tasks.Task),
            typeof(UnityEngine.Events.UnityEvent), typeof(UnityEngine.Playables.PlayableDirector)
        };
        private static readonly string[] UnityNamespaces = { "UnityEngine.", "System.", "System.Threading.Tasks.", "UnityEngine.UIElements.", "UnityEngine.Events.", "UnityEngine.Playables.", "UnityEngine.SceneManagement." };

        /// <summary>The kind colour for a type named this, or null when no such type is known.</summary>
        public static string For(string _simpleName)
        {
            Type type = Find(_simpleName);
            if (type == null)
            {
                return null;
            }
            return ColourOf(type);
        }

        public static string ColourOf(Type _type)
        {
            if (_type.IsEnum)
            {
                return DocsTheme.Enum;
            }
            if (_type.IsInterface)
            {
                return DocsTheme.Interface;
            }
            if (_type.IsValueType)
            {
                return DocsTheme.Struct;
            }
            if (typeof(Delegate).IsAssignableFrom(_type))
            {
                return DocsTheme.Delegate;
            }
            return DocsTheme.Class;
        }

        private static Type Find(string _simpleName)
        {
            if (string.IsNullOrEmpty(_simpleName))
            {
                return null;
            }
            Type cached;
            if (found.TryGetValue(_simpleName, out cached))
            {
                return cached;
            }
            Type type = null;
            foreach (string space in Namespaces)
            {
                type = SceneDocsMembers.FindType(space + _simpleName);
                if (type != null)
                {
                    break;
                }
            }
            if (type == null)
            {
                type = FindInUnityAndDotNet(_simpleName);
            }
            found[_simpleName] = type;
            return type;
        }

        private static Type FindInUnityAndDotNet(string _simpleName)
        {
            foreach (Type anchor in AssemblyAnchors)
            {
                foreach (string space in UnityNamespaces)
                {
                    Type type = anchor.Assembly.GetType(space + _simpleName, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}
