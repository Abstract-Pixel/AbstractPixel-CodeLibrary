using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Fills the new state fields from the old ones: priority to catalog order, time, cursor and HUD to effects.</summary>
    /// <remarks>
    /// - <see cref="Plan"/> is the dry run and changes nothing. <see cref="Apply"/> writes the plan with Undo.
    /// - Sub-states keep any parents they have; one without parents is reported, because the old fields do not say which.
    /// </remarks>
    public static class StateMigrator
    {
        public static StateMigrationPlan Plan(IReadOnlyList<StateSO> _states)
        {
            StateMigrationPlan plan = new StateMigrationPlan();
            plan.CatalogOrder.AddRange(_states);
            plan.CatalogOrder.Sort(CompareForCatalog);
            ReportTies(plan);

            foreach (StateSO state in plan.CatalogOrder)
            {
                plan.Changes.Add($"{state.name}: time {state.GetExecutionTimeScaleTarget()} over {state.TimeScaleEaseDuration}s, cursor {CursorFor(state)}, HUD {(state.ShowGameUIOnExecution ? "shown" : "hidden")}");
                if (state.IsSubState && state.Parents.Count == 0)
                {
                    plan.Problems.Add($"Sub-state '{state.name}' has no parents. Add the states it can open over to its Parents list.");
                }
            }
            return plan;
        }

        /// <summary>Writes the plan's effects into each state and its order into the catalog, recording Undo.</summary>
        public static void Apply(StateMigrationPlan _plan, GameStateCatalog _catalog)
        {
            foreach (StateSO state in _plan.CatalogOrder)
            {
                Undo.RecordObject(state, "Migrate game state");
                WriteEffects(state);
                EditorUtility.SetDirty(state);
            }
            if (_catalog != null)
            {
                Undo.RecordObject(_catalog, "Migrate game state catalog");
                _catalog.States.Clear();
                _catalog.States.AddRange(_plan.CatalogOrder);
                EditorUtility.SetDirty(_catalog);
            }
        }

        public static GameCursorMode CursorFor(StateSO _state)
        {
            bool isLocked = _state.IsCursorLockedOnExecution;
            bool isVisible = _state.IsCursorVisibleOnExecution;
            if (isLocked)
            {
                return isVisible ? GameCursorMode.LockedVisible : GameCursorMode.LockedHidden;
            }
            return isVisible ? GameCursorMode.Free : GameCursorMode.FreeHidden;
        }

        private static void WriteEffects(StateSO _state)
        {
            StateEffectSettings effects = _state.Effects;
            effects.TimeScaleSource = EffectSource.Own;
            effects.TimeScale = _state.GetExecutionTimeScaleTarget();
            effects.TimeScaleEaseSeconds = _state.TimeScaleEaseDuration;
            effects.TimeScaleEaseCurve = new UnityEngine.AnimationCurve(_state.TimeScaleEaseCurve.keys);
            effects.CursorSource = EffectSource.Own;
            effects.Cursor = CursorFor(_state);
            effects.HudSource = EffectSource.Own;
            effects.HudVisible = _state.ShowGameUIOnExecution;
        }

        // Higher old priority goes higher in the catalog; ties fall back to the name so the result is stable.
        private static int CompareForCatalog(StateSO _first, StateSO _second)
        {
            int byPriority = _second.Priority.CompareTo(_first.Priority);
            return byPriority != 0 ? byPriority : string.CompareOrdinal(_first.name, _second.name);
        }

        private static void ReportTies(StateMigrationPlan _plan)
        {
            for (int i = 1; i < _plan.CatalogOrder.Count; i++)
            {
                StateSO above = _plan.CatalogOrder[i - 1];
                StateSO below = _plan.CatalogOrder[i];
                if (above.Priority == below.Priority)
                {
                    _plan.Problems.Add($"'{above.name}' and '{below.name}' share priority {above.Priority}; ordered by name. Drag them in the catalog if the other should win.");
                }
            }
        }
    }
}
