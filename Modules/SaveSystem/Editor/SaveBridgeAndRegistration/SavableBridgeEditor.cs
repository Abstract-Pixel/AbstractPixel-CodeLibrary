using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// The Savable Bridge's inspector: one SavedObjectCard per saved script on the GameObject, in component order.
    /// </summary>
    [CustomEditor(typeof(SavableBridge))]
    public class SavableBridgeEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            SavableBridge bridge = (SavableBridge)target;
            VisualElement cards = new VisualElement();
            RebuildCards(bridge, cards);

            VisualElement root = new VisualElement();
            root.Add(cards);
            root.TrackPropertyValue(serializedObject.FindProperty("savableTargets"), _ => RebuildCards(bridge, cards));
            return root;
        }

        static void RebuildCards(SavableBridge bridge, VisualElement cards)
        {
            cards.Clear();

            List<SavableTarget> targets = bridge.SavableTargets.Where(t => t != null && t.Script != null).ToList();
            if (targets.Count == 0)
            {
                cards.Add(new Label("Nothing on this GameObject is saved. You can remove this Savable Bridge."));
                return;
            }

            bool initiallyOpen = targets.Count <= 2;
            Dictionary<Type, int> occurrencesOfType = targets.GroupBy(t => t.Script.GetType())
                .ToDictionary(group => group.Key, group => group.Count());
            Dictionary<Type, int> seenSoFar = new Dictionary<Type, int>();
            SaveJson serializer = new SaveJson();

            foreach (SavableTarget savableTarget in targets)
            {
                Type scriptType = savableTarget.Script.GetType();
                string numberSuffix = "";
                if (occurrencesOfType[scriptType] > 1)
                {
                    seenSoFar.TryGetValue(scriptType, out int seen);
                    seen += 1;
                    seenSoFar[scriptType] = seen;
                    numberSuffix = $" ({seen})";
                }

                SavedObjectDescription description = SavedObjectDescription.ForBridgeTarget(bridge, savableTarget, numberSuffix, serializer);
                SavedObjectCard card = new SavedObjectCard();
                card.Bind(description, initiallyOpen);
                cards.Add(card);
            }
        }
    }
}
