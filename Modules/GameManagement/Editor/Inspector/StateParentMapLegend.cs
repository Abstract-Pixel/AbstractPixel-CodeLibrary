using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Under the table: the map names too long for its Input column, each in full in a numbered list, each list once.</summary>
    internal class StateParentMapLegend : VisualElement
    {
        public void Show(List<KeyValuePair<int, StateEffectCell>> _rows)
        {
            Clear();
            style.display = _rows.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            List<KeyValuePair<int, StateEffectCell>> left = new List<KeyValuePair<int, StateEffectCell>>(_rows);
            while (left.Count > 0)
            {
                StateEffectCell first = left[0].Value;
                List<KeyValuePair<int, StateEffectCell>> same = left
                    .Where(_row => StateEffectValues.Same(StateEffectKind.Input, first.Value, _row.Value.Value))
                    .ToList();
                Add(Group(same.Select(_row => _row.Key).ToList(), first));
                left.RemoveAll(same.Contains);
            }
        }

        private static VisualElement Group(List<int> _parents, StateEffectCell _cell)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("si-maps");
            group.EnableInClassList("si-maps-changed", _cell.IsChanged);
            string over = (_parents.Count == 1 ? "parent " : "parents ") + string.Join(", ", _parents);
            Label title = new Label("Input maps turned on over " + over);
            title.AddToClassList("si-maps-title");
            group.Add(title);
            for (int i = 0; i < _cell.ListedMaps.Count; i++)
            {
                VisualElement line = new VisualElement();
                line.AddToClassList("si-maps-line");
                Label number = new Label((i + 1) + ".");
                number.AddToClassList("si-maps-number");
                line.Add(number);
                Label name = new Label(_cell.ListedMaps[i]) { enableRichText = false };
                name.AddToClassList("si-maps-name");
                line.Add(name);
                group.Add(line);
            }
            return group;
        }
    }
}
