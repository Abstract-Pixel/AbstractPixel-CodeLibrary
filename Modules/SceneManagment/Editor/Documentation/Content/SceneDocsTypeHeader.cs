using System;
using System.Text;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The top of a type page in the new layout: the declaration as written ("public static class SceneActions"), then
    /// its namespace, assembly and file as fact rows.
    /// </summary>
    public static class SceneDocsTypeHeader
    {
        public static VisualElement Render(string _fullName, DocsRenderContext _context)
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-type-header");
            Type type = SceneDocsMembers.FindType(_fullName);
            if (type == null)
            {
                header.Add(new Label("No loaded type named " + _fullName));
                return header;
            }
            Label declaration = new Label(DeclarationRich(SceneDocsDeclarations.TypeDeclaration(type), type.Name));
            declaration.enableRichText = true;
            declaration.selection.isSelectable = true;
            declaration.AddToClassList("docs-type-declaration");
            DocsCodeBlock.ApplyMonoFont(declaration);
            header.Add(declaration);
            VisualElement facts = new VisualElement();
            facts.AddToClassList("docs-facts");
            facts.Add(DocsFactRows.Row("Namespace", "`" + type.Namespace + "`", _context));
            facts.Add(DocsFactRows.Row("Assembly", "`" + type.Assembly.GetName().Name + "`", _context));
            string file = SceneDocsSource.FileDeclaring(type.Name);
            if (file != null)
            {
                facts.Add(DocsFactRows.Row("File", "`" + file.Substring(SceneDocsPaths.Library.Length) + "`", _context));
            }
            header.Add(facts);
            return header;
        }

        // Every word but the name takes the keyword colour.
        private static string DeclarationRich(string _declaration, string _name)
        {
            StringBuilder text = new StringBuilder();
            foreach (string word in _declaration.Split(' '))
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }
                if (word == _name)
                {
                    // The declared name takes its kind's colour: light green for an enum, teal for a class, and so on.
                    string kindColour = DocsTypeColours.For(word) ?? DocsTheme.Class;
                    text.Append("<color=").Append(kindColour).Append("><b>").Append(word).Append("</b></color>");
                    continue;
                }
                text.Append("<color=").Append(DocsTheme.Keyword).Append(">").Append(word).Append("</color>");
            }
            return text.ToString();
        }
    }
}
