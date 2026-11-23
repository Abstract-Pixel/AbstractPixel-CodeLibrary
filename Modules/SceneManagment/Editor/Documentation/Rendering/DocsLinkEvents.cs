using System;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The one place that uses UI Toolkit's rich-text link events (UnityEngine.UIElements.Experimental), so a later API
    /// change touches one file. Click calls the click handler with the link id; hover sets the label's tooltip.
    /// </summary>
    public static class DocsLinkEvents
    {
        public static void Register(TextElement _label, Action<string> _clicked, Func<string, string> _tooltipFor)
        {
            _label.RegisterCallback<PointerUpLinkTagEvent>(_event => _clicked(_event.linkID));
            _label.RegisterCallback<PointerOverLinkTagEvent>(_event => _label.tooltip = TooltipOrEmpty(_tooltipFor, _event.linkID));
            _label.RegisterCallback<PointerOutLinkTagEvent>(_event => _label.tooltip = "");
        }

        private static string TooltipOrEmpty(Func<string, string> _tooltipFor, string _linkId)
        {
            if (_tooltipFor == null)
            {
                return "";
            }
            string tooltip = _tooltipFor(_linkId);
            return tooltip ?? "";
        }
    }
}
