using AbstractPixel.Core.Editor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A row's status button as plain data (session 5A2, decisions 4 to 8): its colour, what a click does and its two
    /// line tooltip (the status, then what a click does).
    /// <list type="bullet">
    /// <item>Scenes tab: green opens Build Profiles at the scene list in use; blue opens the Addressables Groups window;
    /// red with a group selects the group, whose group check shows the fix; red with no group or a scene set label opens
    /// the scene list, or the Addressables Groups window when new scenes go to Addressables; red while new scenes go to
    /// Addressables that has no settings or no default group opens the Addressables Groups window.</item>
    /// <item>Scene Groups tab: the chip shows the group check. Green or blue opens the Scene Groups Debugger at the
    /// group's row; red selects the group; red from a scene Addressables cannot take yet opens the Addressables Groups window.</item>
    /// <item>A sample scene or group is grey, "Sample: never goes into a build", and a click selects its sample scene group.</item>
    /// </list>
    /// </summary>
    public static class ScenePickerStatusRules
    {
        public const string SampleText = "Sample: never goes into a build";
        public const string BootGroupText = "Boot group, unused";

        public static ScenePickerChip Chip(ScenePickerRow _row)
        {
            if (_row == null || _row.IsHeading)
            {
                return ScenePickerChip.Problem;
            }
            if (_row.IsSample)
            {
                return ScenePickerChip.Sample;
            }
            bool isGroupWithProblems = _row.Kind == ScenePickerRowKind.SceneGroup && _row.ProblemCount > 0;
            if (isGroupWithProblems)
            {
                return ScenePickerChip.Problem;
            }
            bool isUnusedBootGroup = _row.Kind == ScenePickerRowKind.SceneGroup && _row.IsBootGroup;
            if (isUnusedBootGroup)
            {
                return ScenePickerChip.Unused;
            }
            return ChipForStatus(_row.Status);
        }

        public static ScenePickerClick Click(ScenePickerRow _row, ScenePickerProjectFacts _facts)
        {
            if (_row == null || _row.IsHeading)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.None);
            }
            ScenePickerProjectFacts facts = _facts ?? new ScenePickerProjectFacts();
            if (_row.IsSample)
            {
                return SelectGroupOrNothing(_row);
            }
            if (_row.Kind == ScenePickerRowKind.SceneGroup)
            {
                return GroupClick(_row, facts);
            }
            return SceneClick(_row, facts);
        }

        /// <summary>Two lines: the status, then what a click does.</summary>
        public static string Tooltip(ScenePickerRow _row, ScenePickerProjectFacts _facts)
        {
            if (_row == null || _row.IsHeading)
            {
                return string.Empty;
            }
            ScenePickerClick click = Click(_row, _facts);
            return StatusLine(_row) + "\n" + ClickLine(_row, click, _facts ?? new ScenePickerProjectFacts());
        }

        private static ScenePickerChip ChipForStatus(SceneStatus _status)
        {
            switch (_status)
            {
                case SceneStatus.InBuild:
                    return ScenePickerChip.InBuild;
                case SceneStatus.Addressable:
                    return ScenePickerChip.Addressable;
                default:
                    return ScenePickerChip.Problem;
            }
        }

        private static ScenePickerClick SceneClick(ScenePickerRow _row, ScenePickerProjectFacts _facts)
        {
            ScenePickerChip chip = Chip(_row);
            if (chip == ScenePickerChip.InBuild)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.OpenBuildProfilesSceneList);
            }
            if (chip == ScenePickerChip.Addressable)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.OpenAddressablesGroups);
            }
            // Red. Addressables that cannot take a scene yet: the group's fix would fail, so go where it is set up.
            if (_facts.AddressablesNeedsSetup)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.OpenAddressablesGroups);
            }
            if (!string.IsNullOrEmpty(_row.GroupGuid))
            {
                return new ScenePickerClick(ScenePickerClickKind.SelectAsset, _row.GroupGuid, _row.GroupName);
            }
            return WhereNewScenesGo(_facts);
        }

        private static ScenePickerClick GroupClick(ScenePickerRow _row, ScenePickerProjectFacts _facts)
        {
            ScenePickerChip chip = Chip(_row);
            if (chip != ScenePickerChip.Problem)
            {
                return new ScenePickerClick(ScenePickerClickKind.OpenDebuggerAtGroup, _row.AssetGuid, _row.Title);
            }
            // Neither the Inspector nor the Debugger can fix Addressables that has no settings or no default group.
            bool needsAddressablesSetup = _row.HasMakeAddressableProblem && !string.IsNullOrEmpty(_facts.AddressablesSetupProblem);
            if (needsAddressablesSetup)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.OpenAddressablesGroups);
            }
            return new ScenePickerClick(ScenePickerClickKind.SelectAsset, _row.AssetGuid, _row.Title);
        }

        private static ScenePickerClick WhereNewScenesGo(ScenePickerProjectFacts _facts)
        {
            if (_facts.NewScenesGoToAddressables)
            {
                return ScenePickerClick.Of(ScenePickerClickKind.OpenAddressablesGroups);
            }
            return ScenePickerClick.Of(ScenePickerClickKind.OpenBuildProfilesSceneList);
        }

        // A sample scene's click selects its sample scene group; a sample group row selects itself.
        private static ScenePickerClick SelectGroupOrNothing(ScenePickerRow _row)
        {
            if (_row.Kind == ScenePickerRowKind.SceneGroup)
            {
                return new ScenePickerClick(ScenePickerClickKind.SelectAsset, _row.AssetGuid, _row.Title);
            }
            if (string.IsNullOrEmpty(_row.GroupGuid))
            {
                return ScenePickerClick.Of(ScenePickerClickKind.None);
            }
            return new ScenePickerClick(ScenePickerClickKind.SelectAsset, _row.GroupGuid, _row.GroupName);
        }

        private static string StatusLine(ScenePickerRow _row)
        {
            if (_row.IsSample)
            {
                return SampleText;
            }
            if (Chip(_row) == ScenePickerChip.Unused)
            {
                return BootGroupText;
            }
            bool isGroupWithProblems = _row.Kind == ScenePickerRowKind.SceneGroup && _row.ProblemCount > 0;
            if (isGroupWithProblems)
            {
                return ProblemsLine(_row);
            }
            return StatusText(_row.Status);
        }

        // "2 problems. First: level 3 is Not in build."
        private static string ProblemsLine(ScenePickerRow _row)
        {
            string count = _row.ProblemCount == 1 ? "1 problem" : _row.ProblemCount + " problems";
            if (string.IsNullOrEmpty(_row.FirstProblem))
            {
                return count;
            }
            if (_row.ProblemCount == 1)
            {
                return count + ": " + _row.FirstProblem;
            }
            return count + ". First: " + _row.FirstProblem;
        }

        private static string ClickLine(ScenePickerRow _row, ScenePickerClick _click, ScenePickerProjectFacts _facts)
        {
            bool isProblem = Chip(_row) == ScenePickerChip.Problem;
            string fixPart = string.Empty;
            if (isProblem)
            {
                fixPart = _row.Status == SceneStatus.NotInBuild && _row.Kind == ScenePickerRowKind.Scene ? ", where you can add it" : ", where you can fix it";
            }
            switch (_click.Kind)
            {
                case ScenePickerClickKind.OpenBuildProfilesSceneList:
                    return "Click to open Build Profiles at " + SceneListName(_facts) + fixPart + ".";
                case ScenePickerClickKind.OpenAddressablesGroups:
                    if (isProblem && !string.IsNullOrEmpty(_facts.AddressablesSetupProblem) && (_facts.NewScenesGoToAddressables || _row.HasMakeAddressableProblem))
                    {
                        return "Click to open the Addressables Groups window: " + _facts.AddressablesSetupProblem;
                    }
                    return "Click to open the Addressables Groups window" + fixPart + ".";
                case ScenePickerClickKind.SelectAsset:
                    if (_row.IsSample)
                    {
                        return "Click to select " + _click.TargetName + ".";
                    }
                    return "Click to open " + _click.TargetName + " in the Inspector" + fixPart + ".";
                case ScenePickerClickKind.OpenDebuggerAtGroup:
                    return "Click to open the Scene Groups Debugger at " + _click.TargetName + ".";
                default:
                    return string.Empty;
            }
        }

        private static string SceneListName(ScenePickerProjectFacts _facts)
        {
            if (string.IsNullOrEmpty(_facts.SceneListProfileName))
            {
                return "the scene list";
            }
            return _facts.SceneListProfileName + "'s scene list";
        }

        public static string StatusText(SceneStatus _status)
        {
            switch (_status)
            {
                case SceneStatus.InBuild:
                    return "In build";
                case SceneStatus.Addressable:
                    return "Addressable";
                case SceneStatus.NotInBuild:
                    return "Not in build";
                case SceneStatus.Missing:
                    return "Missing";
                default:
                    return "No scene";
            }
        }
    }
}
