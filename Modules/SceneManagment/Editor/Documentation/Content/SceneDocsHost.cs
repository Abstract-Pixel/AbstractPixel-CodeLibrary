using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The scene documentation's answers to the renderer: links open topics in the window, terms show glossary
    /// definitions, and directives become action chips, the timeline, "Your project", members, the code map, the
    /// design decisions list and the parts diagram.
    /// </summary>
    public class SceneDocsHost : IDocsHost
    {
        private readonly Action<string> navigate;
        private Dictionary<string, string> termTopics;

        public SceneDocsHost(Action<string> _navigate)
        {
            navigate = _navigate;
        }

        public void OpenLink(string _target, string _fromPagePath)
        {
            string topic = TopicOfTarget(_target, _fromPagePath);
            if (topic == null)
            {
                return;
            }
            if (topic.StartsWith("http"))
            {
                Application.OpenURL(topic);
                return;
            }
            navigate(topic);
        }

        /// <summary>The topic a link target goes to (or an http URL), or null when it goes nowhere.</summary>
        public string TopicOfTarget(string _target, string _fromPagePath)
        {
            if (string.IsNullOrEmpty(_target))
            {
                return null;
            }
            if (_target.StartsWith("http://") || _target.StartsWith("https://"))
            {
                return _target;
            }
            if (_target.StartsWith("term:"))
            {
                string topic;
                bool known = TermTopics().TryGetValue(_target.Substring(5), out topic);
                return known ? topic : null;
            }
            string fromAsset = SceneDocsPageCache.AssetPathOf(_fromPagePath);
            if (fromAsset == null)
            {
                return null;
            }
            return SceneDocsContents.TopicOfLink(_target, fromAsset);
        }

        private Dictionary<string, string> TermTopics()
        {
            if (termTopics == null)
            {
                termTopics = SceneDocsContents.TermTopics();
            }
            return termTopics;
        }

        public string TooltipFor(string _target)
        {
            if (_target != null && _target.StartsWith("term:"))
            {
                return SceneGlossary.TooltipFor(_target.Substring(5));
            }
            return null;
        }

        public Texture2D LoadImage(string _imagePath, string _fromPagePath)
        {
            string fromAsset = SceneDocsPageCache.AssetPathOf(_fromPagePath);
            if (fromAsset == null)
            {
                return null;
            }
            string assetPath = SceneDocsPaths.Combine(SceneDocsPaths.FolderOf(fromAsset), _imagePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        public string ResolveSourcePath(string _source, string _fromPagePath)
        {
            if (_source.StartsWith("lib:"))
            {
                return SceneDocsPaths.Library + _source.Substring(4);
            }
            string fromAsset = SceneDocsPageCache.AssetPathOf(_fromPagePath);
            if (fromAsset == null)
            {
                return null;
            }
            return SceneDocsPaths.Combine(SceneDocsPaths.FolderOf(fromAsset), _source);
        }

        public VisualElement RenderDirective(DocsDirective _directive, DocsRenderContext _context)
        {
            switch (_directive.Name)
            {
                case "action": return ActionChip(_directive);
                case "timeline": return new SceneDocsTimelineView(navigate);
                case "your-project": return SceneDocsYourProject.Render(_directive.Argument(0));
                case "members": return SceneDocsMembers.Render(_directive.Argument(0));
                case "code-map": return SceneDocsDiagram.CodeMap();
                case "adr-list": return SceneDocsReference.AdrList(navigate);
                case "diagram": return SceneDocsDiagram.Parts(navigate);
                default: return null;
            }
        }

        private static VisualElement ActionChip(DocsDirective _directive)
        {
            string kind = _directive.Argument(0);
            string label = SceneDocsActions.LabelOf(_directive);
            Button chip = DocsChip.Create(SceneDocsActions.IconOf(kind), label, SceneDocsActions.TooltipOf(_directive), () => SceneDocsActions.Run(_directive));
            chip.userData = _directive;
            chip.AddToClassList("docs-action");
            return chip;
        }
    }
}
