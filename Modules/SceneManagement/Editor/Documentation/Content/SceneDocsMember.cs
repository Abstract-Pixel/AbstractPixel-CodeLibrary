using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// One row of a member table: its kind, the modifiers as written ("public static"), its type, its parameters (methods
    /// only), its accessors (properties only), and its comment split into a one-line summary and a list of remarks.
    /// </summary>
    public class SceneDocsMember
    {
        /// <summary>"property", "field", "method", "event" or "value" (an enum value).</summary>
        public string Kind { get; private set; }
        public string Name { get; private set; }
        public List<string> Modifiers { get; private set; } = new List<string>();

        /// <summary>The return type of a method, or the type of a property, field or event. Empty for an enum value.</summary>
        public string TypeText { get; set; } = "";

        /// <summary>"SceneGroup _sceneGroup" for a method; null for every other kind.</summary>
        public string Parameters { get; set; }

        /// <summary>"get;" or "get; private set;" for a property; null for every other kind.</summary>
        public string Accessors { get; set; }

        public string Summary { get; set; } = "";
        public List<string> Remarks { get; private set; } = new List<string>();

        /// <summary>The names of the method's parameters, so the source line of each overload can be told apart.</summary>
        public List<string> ParameterNames { get; private set; } = new List<string>();

        public SceneDocsMember(string _kind, string _name)
        {
            Kind = _kind;
            Name = _name;
        }

        /// <summary>The anchor of this member's row on its type page: "member-" and the lowercase name.</summary>
        public static string AnchorFor(string _memberName)
        {
            return "member-" + _memberName.ToLowerInvariant();
        }
    }
}
