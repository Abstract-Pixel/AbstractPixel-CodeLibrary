using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The spawn point inspector: its label first, then the number of positions and how many relocatables reach them,
    /// the problems with a Select fix, the selected position's instance, and the compact key list.
    /// </summary>
    [CustomEditor(typeof(SpawnPoint))]
    internal sealed class SpawnPointEditor : UnityEditor.Editor
    {
        private VisualElement status;
        private VisualElement selectedPosition;
        private string statusShown;
        private string selectedPositionShown;

        public override VisualElement CreateInspectorGUI()
        {
            // Unity may build the inspector again on the same editor, so what was shown before starts empty.
            statusShown = null;
            selectedPositionShown = null;
            VisualElement root = new VisualElement();
            VisualElement card = SceneSystemUI.Card();
            card.Add(RelocationLabelField.Create(serializedObject.FindProperty("label")));
            status = new VisualElement();
            card.Add(status);
            selectedPosition = new VisualElement();
            card.Add(selectedPosition);
            root.Add(card);

            VisualElement controls = SceneSystemUI.Card();
            controls.style.marginTop = 8;
            controls.Add(new RelocationShortcutList(150f));
            root.Add(controls);

            root.Bind(serializedObject);
            root.TrackSerializedObjectValue(serializedObject, _changed => RelocationEditorModel.MarkDirty());
            root.schedule.Execute(Refresh).Every(400);
            Refresh();
            return root;
        }

        private void Refresh()
        {
            SpawnPoint spawnPoint = target as SpawnPoint;
            if (spawnPoint == null || status == null)
            {
                return;
            }
            RelocationEditorModel model = RelocationEditorModel.Current;
            List<RelocationEditorModel.Position> positions = model.PositionsOf(spawnPoint);
            List<RelocationEditorModel.Problem> problems = new List<RelocationEditorModel.Problem>();
            foreach (RelocationEditorModel.Problem problem in model.Problems)
            {
                if (problem.Target == spawnPoint)
                {
                    problems.Add(problem);
                }
            }

            // Rebuilt only when what it shows changed, so a field being typed in keeps its focus.
            string statusNow = CountText(positions);
            foreach (RelocationEditorModel.Problem problem in problems)
            {
                statusNow += "|" + problem.Message;
            }
            if (statusNow != statusShown)
            {
                statusShown = statusNow;
                status.Clear();
                status.Add(CountLine(positions));
                foreach (RelocationEditorModel.Problem problem in problems)
                {
                    status.Add(ProblemRow(problem));
                }
            }
            RefreshSelectedPosition(spawnPoint, positions);
        }

        private static string CountText(List<RelocationEditorModel.Position> _positions)
        {
            int reached = 0;
            foreach (RelocationEditorModel.Position position in _positions)
            {
                if (position.Match != null)
                {
                    reached++;
                }
            }
            return _positions.Count + "/" + reached;
        }

        // The most important line: how many positions, and how many relocatables reach them.
        private static VisualElement CountLine(List<RelocationEditorModel.Position> _positions)
        {
            int reached = 0;
            foreach (RelocationEditorModel.Position position in _positions)
            {
                if (position.Match != null)
                {
                    reached++;
                }
            }
            VisualElement line = SceneSystemUI.Row();
            line.style.marginTop = 6;
            Label count = new Label(_positions.Count + " " + Plural(_positions.Count, "position", "positions"));
            count.style.fontSize = 15;
            count.style.unityFontStyleAndWeight = FontStyle.Bold;
            line.Add(count);
            Label reachedText = SceneSystemUI.Hint(" · " + reached + " reached by relocatables");
            reachedText.style.marginLeft = 4;
            line.Add(reachedText);
            return line;
        }

        private static VisualElement ProblemRow(RelocationEditorModel.Problem _problem)
        {
            Color colour = SceneSystemUI.Yellow;
            if (_problem.IsError)
            {
                colour = SceneSystemUI.Red;
            }
            return SceneSystemUI.StatusRow(colour, _problem.Message, string.Empty);
        }

        private void RefreshSelectedPosition(SpawnPoint _spawnPoint, List<RelocationEditorModel.Position> _positions)
        {
            bool oneSelected = SpawnPointPositionSelection.SpawnPoint == _spawnPoint && SpawnPointPositionSelection.Positions.Count == 1;
            int number = -1;
            if (oneSelected)
            {
                number = SpawnPointPositionSelection.Positions[0];
            }
            bool numberIsValid = number >= 0 && number < _positions.Count;
            string shownNow = "none";
            if (numberIsValid)
            {
                shownNow = number + "/" + _positions[number].Address.Instance + "/" + Mathf.RoundToInt(_positions[number].Slope);
            }
            if (shownNow == selectedPositionShown)
            {
                return;
            }
            selectedPositionShown = shownNow;
            selectedPosition.Clear();
            if (!numberIsValid)
            {
                return;
            }
            RelocationEditorModel.Position position = _positions[number];
            VisualElement row = SceneSystemUI.Row();
            row.style.marginTop = 6;
            IntegerField instance = new IntegerField("Selected: Instance");
            instance.tooltip = "The instance of the selected position. A relocatable with this instance set goes here.";
            instance.SetValueWithoutNotify(position.Address.Instance);
            instance.style.flexGrow = 1;
            instance.RegisterValueChangedCallback(_change =>
            {
                Undo.RecordObject(_spawnPoint, "Change spawn point position instance");
                _spawnPoint.SetInstance(number, _change.newValue);
                EditorUtility.SetDirty(_spawnPoint);
                RelocationEditorModel.MarkDirty();
            });
            row.Add(instance);
            if (position.HasGround)
            {
                Label slope = SceneSystemUI.Hint(Mathf.RoundToInt(position.Slope) + "°");
                slope.style.marginLeft = 6;
                row.Add(slope);
            }
            selectedPosition.Add(row);
        }

        private static string Plural(int _count, string _one, string _many)
        {
            if (_count == 1)
            {
                return _one;
            }
            return _many;
        }
    }
}
