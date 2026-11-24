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
            if (_target.StartsWith(SceneDocsTypeIndex.Scheme))
            {
                return TypeTopic(_target);
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

        // "type:SceneActions.LoadingProgress" opens the SceneActions page at the LoadingProgress row.
        private static string TypeTopic(string _target)
        {
            string typeName;
            string memberName;
            SceneDocsTypeIndex.Split(_target, out typeName, out memberName);
            string page = "types/" + typeName.ToLowerInvariant();
            return memberName.Length == 0 ? page : page + "#" + SceneDocsMember.AnchorFor(memberName);
        }

        public string CodeLinkTarget(string _code, string _fromPagePath)
        {
            return SceneDocsTypeIndex.TargetFor(_code, TypeOfPage(_fromPagePath));
        }

        // "types/sceneactions.md" is the SceneActions page; any other page has no type of its own.
        private static string TypeOfPage(string _pagePath)
        {
            if (_pagePath == null || !_pagePath.StartsWith("types/"))
            {
                return null;
            }
            string file = SceneDocsPageCache.AssetPathOf(_pagePath);
            return file == null ? null : System.IO.Path.GetFileNameWithoutExtension(file);
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
            if (_target != null && _target.StartsWith(SceneDocsTypeIndex.Scheme))
            {
                return TypeTooltip(_target);
            }
            return null;
        }

        // The member's one-line summary, or the type's first sentence.
        private static string TypeTooltip(string _target)
        {
            string typeName;
            string memberName;
            SceneDocsTypeIndex.Split(_target, out typeName, out memberName);
            Type type = SceneDocsTypeIndex.TypeNamed(typeName);
            if (type == null)
            {
                return null;
            }
            if (memberName.Length == 0)
            {
                string file = SceneDocsSource.FileDeclaring(type.Name);
                return file == null ? null : SceneDocsSource.FirstSentence(SceneDocsSource.SummaryAbove(file, type.Name));
            }
            SceneDocsMember member = SceneDocsMemberReader.Read(type).Find(_member => _member.Name == memberName);
            return member == null ? null : DocsInline.ToPlainText(member.Summary);
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
                case "members": return _context.Page.IsNewLayout ? SceneDocsMemberTables.Render(_directive.Argument(0), _context) : SceneDocsMembers.Render(_directive.Argument(0));
                case "type-header": return SceneDocsTypeHeader.Render(_directive.Argument(0), _context);
                case "decision": return SceneDocsReference.DecisionHeader(_context.Page.Path);
                case "code-map": return SceneDocsDiagram.CodeMap();
                case "adr-list": return _context.Page.IsNewLayout ? SceneDocsReference.DecisionList(navigate) : SceneDocsReference.AdrList(navigate);
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
