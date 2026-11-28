# Ticket 05 handoff: Project Settings page build

`plan: gm-finish, step 5`. 2026-11-28. **Done.** The owner dropped the remaining checks (carried to ticket 11), then
reported two problems; both are fixed, and the owner called the work done.

## What a developer gets

Project Settings > Abstract Pixel > Game Management: five cards (Setup, States, Between scenes, Input, Debugger),
Main states / Sub-states tabs, the state list with its problems and fix buttons, drag and drop, drag-to-reorder,
`+` / `-`, Create settings, Fix now, Move..., and "Check states before a build" (errors stop a build).

## Built

- `Core/Editor/UI` (assembly `AbstractPixel.Core.Editor.UI`, class `EditorUI`): the generic helpers moved out of
  `SceneSystemUI`. The topic-help overloads stayed in the scene module.
- `Editor/Problems/`: `StateListValidator`, `StateProblem`, `StateListEdits`, `StateBuildCheck`. All internal; tests
  reach them through `Editor/AssemblyInfo.cs`.
- `Editor/Settings/`: provider, `StateListView`, `StateRowView`, `StateRowDrag`, `StateListDropBox`, `NewStateMenu`, USS.
- `GameManagementSettingsAssets.Create`, `IsPreloaded`, `EnsureFolder`.
- New setting `GameManagementSettings.CheckStatesBeforeBuild` (on by default).
- The list is plain VisualElements, not a TreeView.
- The catalog is highest priority first. The page lists main states in reverse, so rank 1 is the lowest priority and a
  newly added state goes to the page's bottom (catalog index 0).

## Bugs fixed

| Bug | Fix |
|---|---|
| The drop overlay stayed on when a drag moved onto a row's object field | The box listens on TrickleDown, because the field stops the event |
| Under the list, the note spoke of "the sample's defaults" in a real game | It now only says how to change a state |
| Emptying a row (owner's report): the message named a catalog slot, such as "Slot 5" | The empty row keeps its page rank and the message says "Rank N in the state list is empty" (`StateListEdits.PageRank`) |
| New state (owner's report): with settings inside a package, the save window started outside `Assets`, then failed or fell back to Windows' last folder | `NewStateMenu.FolderForNewStates` uses `Assets/GameManagement/States` then; cancelling removes folders it made |

Owner look change: each sub-state box starts at the main row's rank badge, not its handle (UI-STYLE.md updated).

## Checked

All UI events below were simulated through the real Project Settings window or run by code. No real OS drag or mouse.

| Check | Result |
|---|---|
| Lab compile | clean |
| Lab `AbstractPixel.GameManagement.Tests` EditMode | 90/90 with the two owner fixes |
| Drops | overlay shows and hides (footer, sub-state box, non-State drag, leave, Esc, over a field); adds several, no duplicate, ignores non-States, one Undo; a drop on a row's field replaces only that row |
| Add order | dropped states land at the page's bottom |
| Reorder | main row onto main row works; sub-state onto main row and release outside change nothing; Undo (by code) reverts |
| Stills | 760 and 480 px in the Lab; 760 px in Rally matches by eye |
| Rally (6.3) at `89a97d7` | compiles; page loads. At `370c517`: GameManagement tests 86/86, settings and catalog byte-identical after opening |
| New state save window | opened by calling `NewStateMenu.Create`; its address bar read `...\Abstract_Pixel_Lab\Assets\GameManagement\States`; cancel left no folder |
| Scene tests | 1082 pass, 11 fail on the first run; `AbstractPixel.SceneManagement.Tests` alone 229/229. The 11 (10 SaveSystem sample references, 1 `Sample10LaunchSceneTests` timing) were judged unrelated by cause; no baseline run |

The first validator tests were written before the validator, and the two owner-fix tests before their final code, but
no failing run was recorded for either. Unity 6.3 was not rerun for the two owner fixes (owner: not needed; they use no
new Unity API). The empty-row wording is proven by test only.

Not verified: see ticket 11, "From ticket 05".

## Differences from the approved stills

- "?" and "Learn more" open the module README (no in-Editor docs yet; ticket 12).
- "Add existing state" is a submenu of unlisted states, and the `+` menu opens below the button.
- The Debugger card's two new fields are shown disabled.
- "Set up states..." is not shown; an empty list says "No states yet." (ticket 07).
- `SystemPill` and the scene setup window's pill were not made (ticket 07).

## Open: sample settings are editable (owner has not decided)

With no settings of its own, a project's page shows whichever settings `Find` gets first, which in the Lab are Sample
01's, inside the library. Adding a state then writes a project reference into the library's sample catalog, Move...
would move the sample's settings out of the library, and "Create settings" is not offered. Recommended: show package
settings read-only with a warning and a Create settings button.

## Findings for later sessions

- `recompile_status: completed` can be stale while a modal window blocks the Editor. This session reported "compiles"
  wrongly and the owner tested old code. Check the assembly in `Library/ScriptAssemblies` is newer than the source.
- Delayed page rebuilds only run while the Editor ticks: turn on `set_auto_tick` before scripted UI checks.
- Leave Rally's library on the commit you tested, or say the feature is not there. Moving it back made the page vanish.
- The Lab's `Assets/GameManagementUIPrototype/` is tracked by Git (earlier notes said untracked).

## State of the repos at handoff

- Pushed: `game-management-rework` `89a97d7`; `main` `121adbe`.
- **Not committed:** the two owner fixes with their tests, and these doc updates.
- Rally's library clone is at `89a97d7`, so it lacks the two owner fixes. Its submodule pointer is not committed.
- Lab: the removed settings-page prototype windows, the menu change and the submodule pointer are not committed.
- Sample 01's catalog shows as modified for line endings only; its contents match the commit.
- Lab: `Assets/GameManagement/States/New State.asset` (new, untracked) was left in place; New state saved it inside the
  project, and no library catalog refers to it.
- History: the first T05 commit was made on `main` by mistake and then copied to `game-management-rework`, so `main`
  holds the change twice. Nothing was force-pushed. One code push went out before the 11 scene failures were examined.

## Process and budget

Built in this session, then checked and fixed by an Opus (low effort) agent over several passes. Budget: 7% planned,
raised to 9% for the build (6.6% used, warnings fired, so hand testing moved to the agent). The owner then added
verification budget in rounds (6%, 5%, 5%, 8%); the agent reported about 13.3% used in all.
