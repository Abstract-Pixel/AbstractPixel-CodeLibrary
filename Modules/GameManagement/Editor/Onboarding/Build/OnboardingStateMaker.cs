using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Makes the state assets the setup window asks for, with each menu's default effects, and puts them in the list.
    /// A name that is taken gets a new unique name; nothing existing is written over.</summary>
    internal class OnboardingStateMaker
    {
        private readonly string folder;
        private readonly GameStateCatalog catalog;
        private readonly OnboardingResult result;
        private readonly Dictionary<StateSO, int> ranks = new Dictionary<StateSO, int>();

        internal OnboardingStateMaker(string _folder, GameStateCatalog _catalog, OnboardingResult _result)
        {
            folder = _folder;
            catalog = _catalog;
            result = _result;
        }

        /// <summary>The listed state with this name (spacing and capitals ignored), or null.</summary>
        internal static StateSO FindListed(GameStateCatalog _catalog, string _name)
        {
            string wanted = OnboardingMenuInfo.Normalize(_name);
            foreach (StateSO state in _catalog.States)
            {
                if (state != null && OnboardingMenuInfo.Normalize(state.name) == wanted)
                {
                    return state;
                }
            }
            return null;
        }

        /// <summary>Makes the state, fills in its effects and parents, and lists it at the place its rank gives.</summary>
        internal StateSO Make(string _name, List<StateSO> _parents)
        {
            StateSO state = ScriptableObject.CreateInstance<StateSO>();
            ApplyDefaults(state, _name);
            state.Parents.AddRange(_parents);
            string wanted = folder + "/" + _name + ".asset";
            string path = AssetDatabase.GenerateUniqueAssetPath(wanted);
            AssetDatabase.CreateAsset(state, path);
            result.CreatedPaths.Add(path);
            if (path != wanted)
            {
                result.Renamed.Add("A file named '" + System.IO.Path.GetFileName(wanted) + "' was already there, so the state was saved as '" + System.IO.Path.GetFileName(path) + "'.");
            }
            result.NewStates.Add(state);
            Undo.RecordObject(catalog, "Set up game management");
            int rank = OnboardingMenuInfo.ListRank(_name);
            ranks[state] = rank;
            catalog.States.Insert(InsertIndex(rank), state);
            EditorUtility.SetDirty(catalog);
            return state;
        }

        // Above the first listed state that ranks lower; at the bottom when none does. States the window does not know keep their place.
        private int InsertIndex(int _rank)
        {
            if (_rank < 0)
            {
                return catalog.States.Count;
            }
            for (int index = 0; index < catalog.States.Count; index++)
            {
                int other = RankOf(catalog.States[index]);
                if (other > _rank)
                {
                    return index;
                }
            }
            return catalog.States.Count;
        }

        private int RankOf(StateSO _state)
        {
            if (_state == null)
            {
                return -1;
            }
            if (ranks.TryGetValue(_state, out int rank))
            {
                return rank;
            }
            return OnboardingMenuInfo.ListRank(_state.name);
        }

        private static void ApplyDefaults(StateSO _state, string _name)
        {
            StateEffectSettings effects = _state.Effects;
            if (_name == OnboardingMenuInfo.SettingsStateName)
            {
                return;
            }
            effects.TimeScaleSource = StateEffectSource.Own;
            effects.CursorSource = StateEffectSource.Own;
            effects.HudSource = StateEffectSource.Own;
            // These Own values were chosen here, so the inspector must never replace them with a parent's value later.
            effects.PickedOwnValues |= (1 << (int)StateEffectKind.TimeScale) | (1 << (int)StateEffectKind.Cursor) | (1 << (int)StateEffectKind.Hud);
            if (_name == OnboardingMenuInfo.PlayStateName)
            {
                effects.TimeScale = 1f;
                effects.Cursor = GameCursorMode.LockedHidden;
                effects.HudVisible = true;
                return;
            }
            effects.Cursor = GameCursorMode.Free;
            effects.HudVisible = false;
            effects.TimeScale = _name == "Main Menu" ? 1f : 0f;
            if (_name == "Game Over")
            {
                _state.StateExecutionDelay = 0.5f;
                effects.TimeScaleEaseSeconds = 0.5f;
            }
        }
    }
}
