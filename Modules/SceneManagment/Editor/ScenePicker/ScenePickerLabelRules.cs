namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A row's label button (session 5A2, decision 3): a group name selects that group, a scene set name selects that
    /// set, "Shared by X" selects X (the first if several), and "No group" is plain grey text with no hover or click.
    /// </summary>
    public static class ScenePickerLabelRules
    {
        public static bool IsClickable(ScenePickerRow _row)
        {
            if (_row == null || _row.IsHeading)
            {
                return false;
            }
            return _row.LabelKind != ScenePickerLabelKind.None && !string.IsNullOrEmpty(_row.LabelGuid);
        }

        /// <summary>
        /// "Select level2Group": what a click selects. A label naming several ("Shared by A, B") gives its full text on
        /// the first line, since it may be cut short. Empty when the label is not a button.
        /// </summary>
        public static string Tooltip(ScenePickerRow _row)
        {
            if (!IsClickable(_row))
            {
                return string.Empty;
            }
            string select = "Select " + _row.LabelTargetName;
            bool namesSeveral = !string.IsNullOrEmpty(_row.Detail) && _row.Detail.Contains(", ");
            if (namesSeveral)
            {
                return _row.Detail + "\n" + select;
            }
            return select;
        }
    }
}
