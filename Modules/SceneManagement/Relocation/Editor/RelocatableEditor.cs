using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The relocatable inspector: its label, instance and Wait for ground below, then where it goes ("Goes to:
    /// Crates near the bridge, Instance 5" with Frame) or why it would stay held, and "Add spawn point to this scene
    /// group". Its events are folded away.
    /// </summary>
    [CustomEditor(typeof(Relocatable))]
    [CanEditMultipleObjects]
    internal sealed class RelocatableEditor : UnityEditor.Editor
    {
        private VisualElement goesTo;
        private string goesToShown;

        public override VisualElement CreateInspectorGUI()
        {
            goesToShown = null;
            VisualElement root = new VisualElement();
            root.Add(SceneSystemHelp.HelpRow("Moves this object to a spawn point position with the same label on every scene change.", SceneSystemDocTopics.RelocatableInspector));
            VisualElement card = EditorUI.Card();
            card.Add(RelocationLabelField.Create(serializedObject.FindProperty("label")));

            PropertyField instance = new PropertyField(serializedObject.FindProperty("instance"), "Instance");
            instance.tooltip = "Picks one spawn point position with this label. 0 takes the next free one in Hierarchy order.";
            card.Add(instance);
            PropertyField waitForGround = new PropertyField(serializedObject.FindProperty("waitForGroundBelow"), "Wait for ground below");
            waitForGround.tooltip = "Stays held, with the screen covered, until ground is found under its position (3 s at most). It never moves the object onto the ground.";
            card.Add(waitForGround);

            goesTo = new VisualElement();
            goesTo.style.marginTop = 6;
            card.Add(goesTo);

            Button addSpawnPoint = EditorUI.Button("Add spawn point to this scene group", "Adds a spawn point with this label at the Scene view pivot, in the open main scene.", AddSpawnPoint);
            addSpawnPoint.style.marginTop = 6;
            addSpawnPoint.style.alignSelf = Align.FlexStart;
            card.Add(addSpawnPoint);
            root.Add(card);

            Foldout events = new Foldout();
            events.text = "Events";
            events.value = false;
            events.style.marginTop = 6;
            events.Add(new PropertyField(serializedObject.FindProperty("onRelocated"), "Relocated"));
            events.Add(new PropertyField(serializedObject.FindProperty("onReleased"), "Released"));
            root.Add(events);

            root.Bind(serializedObject);
            root.TrackSerializedObjectValue(serializedObject, _changed => RelocationEditorModel.MarkDirty());
            root.schedule.Execute(RefreshGoesTo).Every(400);
            RefreshGoesTo();
            return root;
        }

        private void RefreshGoesTo()
        {
            Relocatable relocatable = target as Relocatable;
            if (relocatable == null || goesTo == null)
            {
                return;
            }
            RelocationEditorModel model = RelocationEditorModel.Current;
            string shownNow = Describe(model, relocatable);
            if (shownNow == goesToShown)
            {
                return;
            }
            goesToShown = shownNow;
            goesTo.Clear();

            if (model.GoesTo.TryGetValue(relocatable, out RelocationEditorModel.Position position))
            {
                VisualElement row = EditorUI.StatusRow(EditorUI.Green, shownNow, "Where Relocate puts it in the open main scene, worked out like Play.");
                row.Add(EditorUI.Spacer());
                row.Add(EditorUI.Button("Frame", "Selects the spawn point and frames this position.", () => FramePosition(position)));
                goesTo.Add(row);
                return;
            }
            goesTo.Add(EditorUI.StatusRow(EditorUI.Red, shownNow, "Relocate would leave it held, with an error, in the open main scene."));
        }

        private static string Describe(RelocationEditorModel _model, Relocatable _relocatable)
        {
            if (_model.GoesTo.TryGetValue(_relocatable, out RelocationEditorModel.Position position))
            {
                return "Goes to: " + position.SpawnPoint.name + ", Instance " + position.Address.Instance;
            }
            if (_model.NoPositionReason.TryGetValue(_relocatable, out string reason))
            {
                return "Stays held: " + reason;
            }
            return "Stays held: no spawn point in the open main scene '" + _model.MainScene.name + "'.";
        }

        private static void FramePosition(RelocationEditorModel.Position _position)
        {
            SpawnPointPositionSelection.Select(_position.SpawnPoint, _position.Address.Position, false);
            SpawnPointEditing.FrameSelected();
        }

        private void AddSpawnPoint()
        {
            Relocatable relocatable = target as Relocatable;
            if (relocatable == null)
            {
                return;
            }
            SpawnPointCreation.AddFor(relocatable);
        }
    }
}
