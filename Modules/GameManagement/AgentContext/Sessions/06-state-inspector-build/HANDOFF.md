# S0 handoff (ticket 06 close-out)

## Result
Ticket 06 is committed, merged into `main`, pushed, and Rally runs it. Two S0 fixes: "Leave on scene change" now wraps
instead of being cut (seen whole at 275 px); the missing-parent problem names the state once ("...so it can never
open."; test seen failing first). Module tests 120/120 in the Lab. The owner skipped the verifier and trimmed the checks.

## Verification
No verifier verdict: skipped by the owner. Round 1 was started and stopped by the chief; it has no result and left no
findings. It left `Assets/ZZ_VerifyTemp` and a rewritten Sample 01 `Play.asset` in the Lab; both removed or restored.
Verified commit: none (unverified). Checked by me: see below.

## Checks run
| Check | Result | Input |
|---|---|---|
| Lab EditMode `AbstractPixel.GameManagement` | 120/120 | tests |
| Inspector at 275 px, Advanced open | "Leave on scene change" whole (`stills/adv-275.png`) | script-opened window |
| Table after renaming a state | `OldParentName` became `NewParentName` in table and Parents | `AssetDatabase.RenameAsset` |
| Table after an input asset reimport | Input cell changed `MapOne` to `All off` (reference broke from my JSON edit) | file edit |
| No-settings and no-catalog messages | read as text; `stills/prob-nocatalog-275.png` | probe window |
| Project Settings page, editable text fields | 0 (only a read-only path field) | script |
| Rally R1 settings page | five cards, Rally's 5 states, Pause under Play (`stills/rally-r1.png`) | real menu path, script-opened |
| Rally R2 inspector, main `PlayStateSO` and sub `SettingsStateSO` | switches, parents, table, input actions, Problems "Not in the state list" (SettingsStateSO is unlisted in Rally) | script-opened |
| Rally R3 drawer on a temporary Next State Requester | listed state: no icon; unlisted: red icon (`stills/rally-r3.png`) | script-opened |
| Rally R4 Scene System page | normal look, no pill yet (ticket 07) (`stills/rally-r4.png`) | script-opened |
| Rally 6.3 compile on `96ebe07` | clean, DLL fresh | recompile |
No real mouse or keyboard anywhere. Stills are in `stills/` (copies of the scratchpad files).

## What changed
- LIB `main` and `game-management-rework`: `4cdfe7f` (ticket 05 owner fixes: page rank numbers, new states inside Assets),
  `8a5dc67` (ticket 06: inspector, drawer, `StateProblemBox`, parent table, validator, tests), `96ebe07` (S0 fixes),
  merges `af1f5f3`, `05cd3a1`; docs `0d58f2e` on `main`. Pushed both branches (`89a97d7..96ebe07`, `121adbe..05cd3a1`).
- Developer effect: selecting a State asset shows the new inspector; State fields show the red icon when unlisted.
- Lab only, uncommitted: removed the inspector and drawer prototype windows, their helper classes and menu lines.

## Repo and Editor state
| Repo | Branch | HEAD | Uncommitted |
|---|---|---|---|
| LIB (Lab) | main | `05cd3a1` (gmr `96ebe07`) | `DebugDataBase.asset`, Sample 01 catalog (never committed) |
| LAB | main | pointer commit after this handoff | prototype removals; `Assets/GameManagement/` (owner's) |
| RALLY LIB | game-management-rework | `96ebe07` | untracked `.meta` files that were already there |
| RALLY | game-management-rework | pointer commit | fonts, `RallyGameStateCatalog.asset` (owner's, as before) |
Rally `git status --porcelain` equals the start record. Temporary assets deleted in both Editors; probe windows closed.

## Decisions made without you
- Split the old uncommitted work by file: five files for ticket 05's fixes, the rest in the ticket 06 commit. The
  validator and its tests hold both tickets' changes, so they sit in the 06 commit.
- Pushed the library branches although the library CLAUDE.md now says "Never push": the owner's relay and the brief say push.
- Wrapping toggle is a label beside an unlabelled box, because Unity fixes a field label's width.

## Not done / not verified
Carried to ticket 11 (see its "From ticket 06 (S0 close-out)"): Project Settings focus under real typing; narrow-table
"Locked Hidden" wrap re-capture in the Lab; drawer stills re-captured in both Editors; real input map rename in the
Input Actions editor; Unity 6.7; real-mouse drag-reorder, drag and drop and object picker (already ticket 11); no verifier.

## Traps found
- Probe screenshots read screen pixels: a Project Settings window left open in the Lab showed up in a Rally still, and a
  file window once covered a capture (discarded). Close old windows first; read every still.
- A Properties window opened right after closing another came up blank (twice). Opening without a prior close worked.
- Rally's recompile regenerates `ProjectRallyShift.slnx`; revert it.
- No pop-ups appeared this step.

## Next session
S1 (ticket 07, onboarding build) per `Wayfinder/map.md`. Start from LIB `main` `05cd3a1`; Rally library stays on
`96ebe07`. Do not redo the inspector, drawer or any check above. Read ticket 07 ("From ticket 05", "From ticket 06").
Lab has uncommitted prototype removals that are the owner's to keep.

# History

# Ticket 06 handoff: state inspector and state field drawer

2026-11-28. Built and UI-judged in the Lab (Unity 7000.0.0a16). **Not committed.** Verification, review and the
beginner walkthrough go to a second agent (owner's instruction); its results are at the end of this file.

## The owner's requirements

1. The inspector works for main states and sub-states on real `StateSO` assets.
2. Each switch (main: `Keep last | Own`; sub: `Same as parent | Own`) and the "Is sub-state" box show and hide the
   right things.
3. "What this state does over each parent" (the owner calls it the difference table) stays current: editing the sub-state,
   a parent, or a parent's parent updates it.
4. Long text never breaks or crowds the layout, above all input action map names of 56+ characters. Test with real long
   names, noticeably longer ones too, and restore any real asset you touch.
5. The UI must stay easy to read, not only fit.

## Owner decisions this session

| Question | Answer |
|---|---|
| Unticking "Is sub-state" | Clears Parents (Undo restores). No new field; a ticked box with no parents is not saved |
| Remembering a picked Own value | Saved hidden flag (`StateEffectSettings.PickedOwnValues`, internal) |
| Own with no parent value | Keeps the stored value |
| A parent that is a sub-state | One row; `Varies` when its stacks differ, each stack in the tooltip |

Look changes the owner asked for after seeing long names (they replace the matching UI-STYLE.md rows, updated):

- Input actions is a panel like Parents (header, count, bordered table). Each numbered entry shows **Action** (the object
  field, showing only the action's name) and **Map** (full name, wrapping). Each name appears once.
- **No letter codes** (A, B, C). The owner rejected them. Numbers are fine.
- Numbers and handles are centred: on the field in Parents, on the whole two-line entry in Input actions.
- The muted "same as parent" lines fold under "What it uses over each parent" (main states: "What this means"). Closed by
  default; opens when the developer switches that effect away from Own. Wording: `Over parents 3, 4: Locked Hidden`;
  `No parent sets this, so it stays as it was`.

Agent judgments the owner has not reviewed:

- Table Input cell over 12 characters shows `4 maps`; the maps are listed in full, numbered, under the table.
- Under 360 px table width, each parent's values stack, two per line. Under 340 px inspector width, "Action"/"Map"
  captions move above their values and switch labels shrink.
- Folded lines longer than 60 characters point at parent numbers instead of names.
- The missing-parent message says "its 5 other parents" past two names.
- A sub-state whose parent slots are all empty is now an Error (the machine can never open it).
- "Problems (n)" shows the count in its title.
- Multi-object editing of states is not supported by the custom inspector.

## Built (library, all uncommitted)

- `Editor/Inspector/`: `StateSOEditor`, `StateInspectorView`, `StateInfoSection`, `StateListPanel`, `StateParentList`,
  `StateReferenceList`, `StateParentTable`, `StateParentMapLegend`, `StateEffectRow`, `StateEffectFields`,
  `StateInputActionList`, `StateProblemsSection`, `StateAssetWatcher`, `StateInspector.uss`.
- `Editor/Inspector/Values/`: `StateStacks`, `StateParentValues`, `StateEffectValues`, `StateOwnValues`,
  `StateEffectCell`, `StateEffectKind`.
- `Editor/Drawers/StateFieldDrawer.cs` (UI Toolkit and IMGUI).
- `Editor/Problems/`: new `StateProblemBox` (shared with the settings page); validator gained `FindForState`, `NotInList`.
- Settings page rebuilds when a listed state changes (`StateAssetWatcher`).
- `Runtime/AssemblyInfo.cs`, `Runtime/Machine/Effects/StateEffectSettings.cs` (the hidden flag).
- Tests: `Tests/Editor/StateParentValuesTests.cs`, four more in `StateListValidatorTests.cs`.

Other uncommitted library changes belong to ticket 05 (its two owner fixes). Leave them in place.

## Checked in the Lab

Switches and edits were set by script on a real Properties window. No real mouse or keyboard.

| Check | Result |
|---|---|
| `AbstractPixel.GameManagement` EditMode | 116/116 |
| Sample states: first Own copies parent 1; teal only where values differ | pass |
| Table updates after editing the sub-state, a parent and a grandparent | pass |
| Main state hides parent panels; untick clears parents; Undo restores | pass (one Undo also reverted earlier scripted edits, because scripts share an Undo group) |
| Long names: state names ~55-80, map names 57-110 characters, 6 parents | stills read at 275 and 480 px; fixes made |
| 16 parents and 20 actions, live add, remove and resize | nothing hidden or over the footer |
| Row number alignment, 242-1067 px, live resize | 0.5 px or less |
| Action field text after the reference changes | pass |
| Folded lines closed on open (`Settings`, 760 px) | pass |

## Not verified

- **Unity 6.3: nothing at all** (not even a compile).
- Folded line opening after a switch back from Own: the script run returned nothing.
- Stills at 1200 px: the captures came out unpainted. Wide checks stop at 942 px (still) and 1067 px (measured).
- Clicks on `+`, `−`, fix buttons, "Open in Project Settings", "Learn more", "Is sub-state" tick; drag-reorder of
  parents and input entries; Unity's object picker; drag and drop onto fields; real-click Undo.
- **State field drawer: never looked at or captured.** The ticket wants the owner to see stills of it.
- The settings page after the new watcher (rebuild loops, lost focus while typing).
- The "no settings" and "no catalog" problem box; teal focus and hover colours.

## Traps

- Row spacing: Unity's `ListView` overrides a row's padding, so pad the row's body.
- Never `capture editor_window` on the Lab (DX12 crash). Stills used `InternalEditorUtility.ReadScreenPixel`; a window
  needs 3-5 s to paint after opening.
- `recompile_status` can be stale: check the DLL time in `Library/ScriptAssemblies`.
- Sample state assets still hold old serialized fields, so saving one rewrites it. Back up before editing and restore
  byte for byte. `Minimal Game Catalog.asset` already showed as modified before this session.
- Open from ticket 05: with no settings of its own, the Lab uses Sample 01's, so "Add to list" writes into the library's
  sample catalog.

## Second agent results

2026-11-28, second agent, budget 13%. Part of the list was done; the rest is listed under "Still not verified".
No library code was changed. No bugs were confirmed, so there are no new tests.

### Unity 6.3 (Rally, 6000.3.15f1)

- Before: clone at `89a97d7` (game-management-rework); only five untracked `.meta` files. Lab `HEAD` (`121adbe`)
  matches `89a97d7` in `Modules/GameManagement` code apart from `GLOSSARY.md`. So all 60 uncommitted code files
  (Editor, Runtime, Tests; no AgentContext, no Samples) were copied over. That includes ticket 05's two fixes,
  because the inspector shares files with them.
- Compile: clean. `AbstractPixel.GameManagement` EditMode: **116/116**.
- Inspector opened on `Settings` (sub-state) and `Play` (main): every section builds and both switch sets show.
  Still: `scratchpad/rally63-settings-480.png`.
- State field drawer (UI Toolkit) works. Rally's own catalog does not list the sample states, so both fields correctly show the red
  icon. Still: `scratchpad/drawer-rally63-420.png`.
- After: the Rally clone is restored (same `git status --porcelain` output as before, HEAD `89a97d7`) and recompiled.
  **Rally does not have the feature.** Rally's `ProjectRallyShift.slnx` was regenerated by the recompile. Its font
  `.asset` changes date from 10:49, before this session, so I left them alone.

### Unity 7.0 (Lab, 7000.0.0a16)

- EditMode 116/116.
- State field drawer: `NextStateRequester` on a temporary prefab, with `When Leaves` = Play (listed: no icon) and
  `Go To` = a new unlisted state (red icon with the warning tooltip). Stills at 420 and 275 px. The field shrinks and
  the icon stays inside the window. Temporary assets deleted.
- Beginner walkthrough on a new `Shop` state in a real Properties window, all as listed under "Input":
  - Main state on creation: "Not in the state list" problem, `Keep last | Own` switches.
  - Ticking "Is sub-state" turns the badge into Sub-state, shows "Add a parent…", the Parents panel ("List is empty"),
    and the switches become `Same as parent | Own`. An effect already set to Own stays Own.
  - `+` adds slot 1. The table shows `1 | None | - …` and Problems adds "only empty parent slots… Remove slot".
  - Setting parent 1 to Play makes the empty-slot problem go away, and the table shows Play's values. A second `+`
    plus Pause makes "Opens over: Play | Pause" and gives two table rows.
  - Cursor Own, then back to Same as parent: **the folded "What it uses over each parent" line opens**, reading
    `Locked Hidden over Play, Free over Pause` (this was a priority gap; it passes).
  - "Add to list" removes the Problems section. Undo brings it back and Redo removes it again.
  - "Open in Project Settings" opens Project Settings on the Game Management page. Two reads 3 s apart gave the same
    elements, so there is no rebuild loop while it sits idle.
  - Unticking "Is sub-state" clears the parents (Opens over: nothing). Undo restores Play and Pause.
  - The sample catalog was restored byte for byte from `backup/sample01` and reimported. The Sample 01 folder now
    matches the backup exactly.

### Input: real versus simulated

- Clicks on `+`, `Own`, `Same as parent`, "Is sub-state", "Add to list" and "Open in Project Settings" were sent as IMGUI
  `MouseDown`/`MouseUp` events to the Properties window (`EditorWindow.SendEvent` at the element's centre). They went
  through the panel's own hit testing, with the Editor in the foreground. No OS mouse was used.
- Setting a parent was scripted (`ObjectField.value`): it stands in for the object picker or drag and drop.
- Undo and Redo were scripted (`Undo.PerformUndo` / `PerformRedo`), not Ctrl+Z.

### Code review

- No `var` in the C# files. The largest file is 194 lines (`StateParentValues`). One type per file (the
  `StackResult` struct is private to `StateParentValues`, which the rule allows). No `GetInstanceID`, and no Unity 7
  only APIs found. Unity 6.3 compiling clean confirms the API surface for 6.3. 6.7 sits between 6.3 and 7.0 and was
  checked by reading only.
- `StateReferenceList.RemoveSlot` nulls the reference before deleting the element. That is redundant on Unity 6+ but
  harmless, so I left it.
- No simplification was worth the risk within this budget.

### Look questions for the owner (not changed)

- A sub-state with no parents still shows the empty table header (Parent | Time | Cursor | HUD | Input) under the
  "Add a parent" note. Hiding the table until there is a parent may read cleaner.
- Parent object fields show Unity's default `Play (State SO)` type suffix, both in the inspector and in the drawer.

### Second pass (6% budget): long names and reorder, Lab only

- **Setup:** `LongNames.Make` creates three states with names of about 55-80 characters, with and without spaces, a sub-state with 6
  parents, and 8 input entries over four maps (58, 71 and 110 characters, and `UI`).
- **Overflow, 275 / 480 / 760 / 1200 px:**
  - No element sticks out past the right edge at any width, and the 1200 px read reports 0 overflow.
  - Both lists draw every row (6/6 and 8/8), with nothing hidden below the list or over its footer.
  - **The 1200 px still painted** (wait 5 s after foregrounding).
- **Row numbers during live resize, 1200 → 760 → 480 → 275:** off centre by 0.5 px at most.
- **Stills:** `scratchpad/long2-275.png`, `long2-480.png`, `long2-760.png`, `long2-1200.png`,
  `long2-480-input.png`. They read well. Names without spaces wrap mid-word (`GarageMod|e`). Action names
  cut off with "…" in the field, as designed.
- **Reorder (scripted `MoveArrayElement`, the same data change the ListView makes):**
  - Moving the 110-character entry to the top renumbers the rows.
  - Every row height follows its own map (72 / 59 / 46 px for 3 / 2 / 1 wrapped lines).
  - Each field still shows only the action name.
  - Undo and then closing and reopening the window restored the order, heights and action-only text.
- **Drag with simulated mouse events does not work:** an IMGUI `MouseDown` + `MouseDrag` on the handle started an OS
  drag loop that froze the Editor until Escape was sent to it, and the list did not change. Real drag-reorder still
  needs a human or a real OS mouse. The Editor recovered with no harm.
- **Restore:**
  - `LongNames.Make` calls `SaveAssets`, which also rewrote the sample `Settings.asset` (old fields dropped).
  - Both it and the catalog were restored from `backup/sample01`. The Sample 01 folder matches the backup again.
  - `Assets/ZZ_StateInspectorTemp` was deleted.

Look questions from the long names (not changed):
- At 1200 px the table cuts the first parent's name with "…" while the Cursor and HUD columns have lots of spare room.
- The missing-parent problem repeats the long state name three times: twice in the message, once on the button.

### Third pass (4% budget): 6.3 long names with the current Lab code

- **Rally before:** HEAD `89a97d7`, with the same `git status` as the first pass. Rally's own Sample 01 folder was backed up to
  `scratchpad/backup/rally_sample01`. The 60 changed code files were copied over.
- **Compile and tests:** compiles clean, EditMode tests pass 116/116.
- **Long names in 6.3, 275 / 480 / 1200 px:**
  - 0 overflow, and every row is drawn (6/6 parents, 8/8 inputs).
  - Row numbers are off centre by 0.5 px at most during live resize 1200 → 760 → 480 → 275.
  - At 1200 px the full parent names show, and "Locked Hidden" and `0.3333` are whole.
- **Defect fixed:** at 275 px the stacked table cut "Locked Hidden" to "Locked H…".
  - **Fix:** `StateInspector.uss` now lets a value in the narrow table wrap onto a second line instead of being cut (two
    `.si-table-narrow` rules). It now reads "Locked / Hidden".
  - This is USS-only, so no EditMode test can cover it. I checked it with a still instead of a failing test.
  - The fix was made in the Lab first and copied to Rally. It was not re-captured in the Lab.
- **Switches in 6.3** (IMGUI mouse events on a temporary main state):
  - Ticking "Is sub-state" turns the switches into `Same as parent | Own` and shows the Parents panel and table. Unticking turns them
    back into `Keep last | Own` and hides both.
  - Undo after an untick with no parents changes nothing. That is expected: a ticked box with no parents is not saved.
- **Rally after:** the temporary assets are deleted and Rally's Sample 01 folder is restored from its backup. `git status --porcelain` matches the record and HEAD is `89a97d7`.
  It recompiled clean. **Rally does not have the feature.**
- **Stills:** `scratchpad/r63-long-275.png`, `r63-long-480.png`, `r63-long-1200.png`,
  `r63-long-275-table-fixed.png`.

### Still not verified

- The fix to the narrow table (`StateInspector.uss`) has not been re-captured in the Lab. The table after renaming a state or an input map, or after a grandparent edit. Project
  Settings focus while typing or dragging. Out of budget on the second pass.
- (Long names in the Lab: done in the second pass above.)
- Real drag-reorder of parents and input entries (renumbering, wrapped-row heights), the real object picker, and drag and
  drop onto fields. The input field's action-name-only text after picker, drag, Undo, reorder and reselect.
- Input actions panel and per-effect Own for HUD and Input in the walkthrough. Table updates after renaming a state or
  an input map. Grandparent edits (not re-run; the first agent's pass stands).
- Project Settings focus while typing or dragging with the watcher active (only the idle state was checked).
- The no-settings and no-catalog messages, teal colours, and real keyboard Undo.
- Anything in 6.3 beyond build, tests, one read and the drawer still.

## After the second agent (first agent, UI judgment)

- Fixed: at wide widths parent names were cut while Cursor and HUD sat mostly empty. Value columns now have fixed widths
  and Parent takes the rest; the table stacks under 400 px. Stills read at 480 and 1200 px in the Lab. Not rechecked in 6.3.
- Fixed the map list title: "Input maps turned on over parents ...".

## Third round (2026-11-28): column sizing and empty states. Final state

Where this differs from earlier sections (fixed column widths, 360/400 px stack points), this section wins. The rules
are in UI-STYLE.md.

- **Column bug:** value columns had fixed widths, so a bold teal "Locked Hidden" (needs 107 px, had 104) stayed cut at
  any width. `StateParentTable` now measures content; `StateTableColumns` (4 tests, seen failing first) decides widths
  and when to stack.
- **Empty states:** compact one-line lists; a warning box in the over-each-parent panel. The "NOTE: Add a parent..."
  line under the checkbox was kept (it says the tick is not saved).
- **Narrow switch:** "Same as parent" is no longer cut at 275 px.

| Check (script-driven; temporary states only) | 7.0 Lab | 6.3 Rally |
|---|---|---|
| Compile, module EditMode tests | 120/120 | 120/120 |
| No table value cut, 275-1200 px growing and shrinking | pass (all-teal and 6 long parents) | pass (6 long parents; 275, 440, 480, 760, 1200, 480, 275) |
| "Locked Hidden" wraps at 275 px | still read | measured, no still |
| Empty panels: 50 px; add one, remove it | pass (112 px with one) | pass (96 / 103 px with one) |
| No-parent warning | still read | still read |

- Not rerun in this round: live row-number centring (list row styles are unchanged) and the 6.3 teal two-parent case.
- + and - were pressed with a submit event, the sub-state box and Input switch set by script. No real mouse.
- **Still cut:** "Leave on scene change" below about 375 px. Unity sets that label's width in code, so a style rule
  could not change it.
- **Still unverified:** table updates after renaming a state or input map; Project Settings focus while typing or
  dragging; real-mouse drag-reorder, drag and drop and object picker. The owner has not answered on the drawer icon.
- Rally is restored to `89a97d7` (no inspector there); `ProjectRallyShift.slnx`, regenerated by test compiles, was
  reverted.
- Sample 01 `Play.asset` was written to disk during the first test run of this round (14:18:42), probably saving unsaved
  Editor test edits. It was restored from the pre-session backup, which the folder now matches.
- **Nothing is committed.** The Lab library is on `main`; the branch rule is commit on `game-management-rework`, then
  merge. The uncommitted work includes ticket 05's two fixes.

