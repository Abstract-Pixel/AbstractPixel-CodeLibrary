using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>What the migration would do, worked out without changing anything (the dry run).</summary>
    public class StateMigrationPlan
    {
        /// <summary>The states in their new catalog order, top (outranks all) first.</summary>
        public readonly List<StateSO> CatalogOrder = new List<StateSO>();

        /// <summary>One plain line per field the migration would write.</summary>
        public readonly List<string> Changes = new List<string>();

        /// <summary>Things a person must decide: priority ties, sub-states without parents.</summary>
        public readonly List<string> Problems = new List<string>();

        public override string ToString()
        {
            string order = string.Join(" > ", CatalogOrder.ConvertAll(_state => _state.name));
            return $"Catalog order: {order}\nChanges:\n  {string.Join("\n  ", Changes)}\nProblems:\n  {string.Join("\n  ", Problems)}";
        }
    }
}
