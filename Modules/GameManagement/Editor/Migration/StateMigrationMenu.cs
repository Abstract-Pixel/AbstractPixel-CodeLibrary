using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Menu items for the migration: a dry run that only logs, and an apply that asks first.</summary>
    public static class StateMigrationMenu
    {
        private const string MENU_ROOT = "Tools/Abstract Pixel/Game Management/";

        [MenuItem(MENU_ROOT + "Migrate State Assets (Dry Run)")]
        private static void DryRun()
        {
            Debug.Log("[Game management] Migration dry run, nothing changed:\n" + StateMigrator.Plan(FindAllStates()));
        }

        [MenuItem(MENU_ROOT + "Migrate State Assets (Apply)")]
        private static void ApplyAfterConfirm()
        {
            StateMigrationPlan plan = StateMigrator.Plan(FindAllStates());
            GameManagementSettings settings = GameManagementSettingsSource.Find();
            GameStateCatalog catalog = settings != null ? settings.Catalog : null;
            bool confirmed = EditorUtility.DisplayDialog("Migrate state assets",
                $"Write new effects into {plan.CatalogOrder.Count} states and reorder the catalog? {plan.Problems.Count} problems need a person afterwards (see the console). Undo is recorded.",
                "Migrate", "Cancel");
            if (!confirmed)
            {
                return;
            }
            StateMigrator.Apply(plan, catalog);
            Debug.Log("[Game management] Migration applied:\n" + plan);
        }

        private static List<StateSO> FindAllStates()
        {
            List<StateSO> states = new List<StateSO>();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(StateSO)))
            {
                StateSO state = AssetDatabase.LoadAssetAtPath<StateSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (state != null)
                {
                    states.Add(state);
                }
            }
            return states;
        }
    }
}
