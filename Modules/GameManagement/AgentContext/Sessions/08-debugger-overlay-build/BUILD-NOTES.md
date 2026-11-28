# S2 build notes
## Built
- New asmdef `AbstractPixel.GameManagement.Debugger` (`Runtime/Debugger/`): overlay, view, snapshot, `StateColors`, toggle input; UI assets in `Assets/`.
- `Editor/Debugger/`: F8 shortcut, assembly filter (`IFilterBuildAssemblies`), UI assets preloaded only for builds that include the debugger.
- Settings: `DebuggerToggleAction`, `IncludeDebuggerInReleaseBuilds`; Debugger card fields live. README section.
- For ticket 09: `StateColors.IndexFor(state, catalog)` is internal, visible to the Editor asmdef.
## Commits
LIB `bbbf10e..d5cf8fb` (gmr `842ba44`, `4f1cd03`; merges on main). Rally library: gmr `4f1cd03`.
## My one check
- Lab, Sample 01 Play: toggle action (temporary `` ` `` binding, its map never enabled; queued Input System event) opened it at time 0; states driven by script (`GameStates.Request/Leave`); stack, effects, refusal, journal read; real OS F8 with Game view focused: 3 presses, 3 toggles.
- Found and fixed: F8 did nothing (Unity runs no shortcut with the Game view focused in Play; the key is now read through the Input System there); long top name clipped and short names cut to "..." (`StackNameFit`); release opt-in label clipped.
- Rally: compile clean on `4f1cd03`; Debugger card still read. Stills in `stills/`, scripts in `tools/` (paths point at my scratchpad).
## Known gaps
- No player build: the release filter and asset preload are proven only by rule tests. Check that a build missing the filtered assembly starts clean (Unity's runtime-init list names it).
- Not checked: F8 with another window focused (my try could not send the key to a floating Scene view) or rebound in Shortcuts; input pass-through with a real mouse or gamepad (test covers picking and focus only); scene change; two Play sessions without domain reload; no-settings project; 7th state; Rally Play mode (R7); toggle action saving and Undo (R6); Sample 01 flow test.
- Toggle action is read once when the overlay is made; changing it during Play needs a new Play session.
- Input updates set to Fixed Update would stop the toggle at time 0 (not handled).
- Effects show "—" for a value no state has set (Sample 01 input).
## Decisions made without you
- Colour = state's place in the catalog (empty and repeat slots skipped), mod 6; outside the catalog: FNV hash of the name. Reordering the catalog moves colours.
- "Last 8 journal lines" = last 8 journal records, then grouped; one moment = within 5 ms.
- Overlay starts hidden each run. Key hint: Editor shortcut key, else the action's binding, plus " hide".
- Release exclusion by assembly filter, not a define constraint, so the settings checkbox is the only opt-in.
- Icons are drawn (player default font may lack the symbols). Cursor reads "Locked, hidden" and so on.
- Long names: short stack names stay whole; long ones share the rest.
## Repo and Editor state
- LIB main, uncommitted from before: `DebugDataBase.asset`, Sample 01 catalog, mega-07-09 session files and untracked `.meta`s.
- Lab: owner's `Assets/GameManagement/` and pointer. `ProjectSettings.asset` gains a dead preload line (guid `a4f96bfd…`) after each module test run; I restored it. Check it again after tests.
- Rally: library on `game-management-rework` `4f1cd03`; pointer uncommitted; owner files hash-identical; `.slnx` restored.
- Temporary Lab assets, sample settings re-saves, input settings and the stray `Assets/UI Toolkit/` theme (made when I created PanelSettings) removed or restored.
## Traps found
- No pop-ups.
- `unity command test run` while in Play mode hangs and blocks all later commands. Freed with a real Ctrl+P in the Lab. Stop with `unity command editor.playmode stop` before testing.
- `EditorWindow.SendEvent` does not reach the shortcut manager.
