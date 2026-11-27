# Session 8B-look handoff: debugger window look approved, plus a working proof of concept

`plan: gm-finish, step 3`. Written 2026-11-28. Read this file first to start ticket 04.

## 1. What happened, and how this session differed from the usual workflow

Ticket 03 asked for a look-only, HITL session: capture stills of prototype window 10 (the Game State Debugger
window), iterate until the owner calls it final, add the approved look to `UI-STYLE.md`. That is how the session
ended, but not how most of it ran.

The owner's actual ask, given mid-session, was a working proof of concept: a resizable footer, click-to-navigate from
a journal row to a real asset/GameObject, row indexing, and an outcome filter/sort — explicitly going past the
ticket's "no product code" rule. This was flagged to the owner before any code was written (the ticket's own scope
vs. the request), and the owner chose to proceed with the override. Everything built is still confined to the Lab's
untracked prototype folder (`Assets/GameManagementUIPrototype/`, never copied into the library) and the prototype's
own hardcoded sample data — no real `GameStateMachine.Journal` or production code was touched.

The owner then ran several rounds of visual polish from screenshots and live review in the Lab Editor (column
spacing, a multi-select flags filter replacing a single-choice dropdown, a tab-styled sort toggle, chip-based
footer formatting, alignment and spacing fixes down to the pixel) before calling the look final. The prototype
changes were committed to the Lab project's own repository at the owner's explicit request, overriding the ticket's
"the prototype folder is untracked, never commit, never delete" rule for this session (Lab commit `e33c2e2` on
branch `learn`) — the library repository's own commit here (this handoff and `UI-STYLE.md`) is unaffected by that
and follows the library's normal rule.

## 2. What exists

| Thing | Where | State |
|---|---|---|
| Debugger window prototype | Lab project: `Assets/GameManagementUIPrototype/Editor/GameStateDebuggerPrototypeWindow.cs` (+ `GameManagementPrototypeRows.cs` for the journal logic). Menu: `Tools/External Project Tools/Game Management/UI Prototype/10 Debugger Window` | Owner-approved look; also a working POC (see below) |
| Window/row UXML and USS | `Assets/GameManagementUIPrototype/UI/DebuggerWindow.uxml`, `JournalRow.uxml`, `Debugger.uss` (shared with the overlay, window 09) | Owner-approved; see `UI-STYLE.md` for exact values |
| Approved look | `../../UI-STYLE.md`, "Game State Debugger window" section | Source of truth for exact values |
| Stills | `Sessions/8B-look/stills/` (480/760/1200px full view, 760px Refused-only, 760px empty journal), each read by the agent first | Final round only |
| Prototype folder | Lab `Assets/GameManagementUIPrototype/` | **Untracked by the ticket's own rule in general; committed this session only at the owner's explicit, one-time request** — Lab commit `e33c2e2`, not pushed (library `CLAUDE.md`: never push) |

Paths without a project are relative to `Modules/GameManagement/` in the library.

## 3. The proof of concept, in order

1. **Resizable footer**: a vertical `TwoPaneSplitView`, the footer (Time/Cursor/Input/Stack details) as its fixed
   pane. Verified by simulating a drag on the handle: footer height moved 110 → 56 → 156px.
2. **Click-to-navigate**: clicking a row's State or Requester text selects and pings the matching asset/GameObject.
   Routed to the GameManagement sample (`Modules/GameManagement/Samples/01 Minimal Game/`) as a stand-in for real
   journal data: `FindSampleState` looks up a `StateSO` by name under the sample's `States/` folder;
   `FindRequesterGameObject` searches the currently loaded scene(s) by name. Verified both the success path (state
   names always resolve; "Play Button" resolves when the sample scene is loaded) and the graceful-fail path ("Pause
   Key", "Settings Button" have no match — link greys out, tooltip explains why, click does nothing and logs a
   warning, matching the owner's explicit answer to the ticket's open question).
3. **Row index**: a stable 1-based badge (1 = most recent), tied to the row's position in the full, unfiltered
   journal, so it does not renumber when the list is filtered or sorted — reuses the overlay's own flat grey badge
   style unchanged, after an earlier attempt at a different (blue, translucent) badge was shown to the owner and
   rejected as not reading as native Unity UI.
4. **Outcome filter and sort**: a multi-select `EnumFlagsField` (any combination of Entered/Applied/Refused/Left),
   verified with Entered+Refused producing exactly the 4 matching rows; a two-segment Newest/Oldest toggle group
   (not a single cycling button), verified to actually reverse the visible order.
5. **Footer clarity pass**: the Input effect's `EnableOnly(IReadOnlyList<InputActionMap>)` (confirmed by reading
   `IInputSource.cs` — the parameter is `InputActionMap`, not an Action or an Asset) is now shown as
   "Player map off → UI map on" pill chips with a tooltip spelling out what `EnableOnly` actually does, replacing an
   earlier, unexplained "+UI −Player" string the owner could not interpret. The Stack line now reuses the overlay's
   own colour-dot chips instead of a plain string.

Every step above was checked live in the running Lab Editor (via `unity-pipeline`, not just read from source):
clicking rows, dragging the footer, setting filter flags, and reading back the console after each change, rather
than trusting the code alone.

## 4. Not done / left for ticket 09 (the real build)

- **Click-to-navigate targets the sample project, not the real journal.** Ticket 09 needs it routed to the
  `StateSO`/GameObject the journal entry actually recorded, once `GameStateMachine.Journal` carries real references.
- **The journal itself is still the prototype's hardcoded 8-row array** (`GameManagementPrototypeRows.cs`). The
  filter, sort and index logic all work against that array; ticket 09 wires the same UI to the real journal.
- **The footer is resizable but its content is not wired to the selected row.** It is still the same static
  placeholder text used since Session 5 ("Entered Pause at 38.120", fixed Time/Cursor/Input/Stack values). Ticket 09
  needs a real per-row changed-fields model before the footer can show what actually changed for the selected row.
- Long state/requester names were not tested in this window (same open item as the overlay in `UI-STYLE.md`'s
  "Long names" section, written for the state inspector only).

## 5. Prompt for ticket 04

> You are working in the AbstractPixelLibrary game management module (library at
> `Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary`, branch `main`; Rally Shift at
> `ActiveProjects/ProjectRallyShift`). plan: gm-finish, step 4. Read
> `Modules/GameManagement/AgentContext/Wayfinder/tickets/04-stability-gate.md` first — check
> `Wayfinder/map.md` first in case this ticket is already running in another session, since the map itself says it
> may run beside the look tickets.
>
> This is a task session, not a look session: no UI, the owner answers two questions (branch and push policy) partway
> through. Work:
> 1. Rally (Unity 6.3): the full `AbstractPixel.GameManagement.Tests` EditMode run and every parity scenario in
>    `../../Parity/README.md` against `../../Parity/baseline/`.
> 2. Lab (Unity 7): the full module EditMode tests and a clean rerun of Sample 01's PlayMode flow test (last known
>    failure: cursor-lock state only).
> 3. Unity 6.7: confirm a clean compile, or say why it could not be checked.
> 4. Ask the owner the branch question (does `game-management-rework` stay the only code branch, given docs and this
>    map live on `main`) and the push question (library `CLAUDE.md` says never push; `PLAN-OVERVIEW.md` used to say
>    push per task — which wins from now on), then record both answers in `DECISIONS.md` and `PLAN-OVERVIEW.md`'s
>    rules.
> 5. Fix any failure, tests first, with a clean rerun after — never write one off as noise.
>
> Finish: handoff in `Sessions/04-stability-gate/HANDOFF.md` (or similar — check whether ticket 04 already has a
> session folder from a prior run) with every run's counts, the branch/push answers, and ticket 05's prompt once 04
> is clear.

## 6. Not yet committed

The library side of this session (`UI-STYLE.md` and this handoff) follows the library's normal rule: staged and
committed in this repo, with the task ID first in the commit message, never pushed. The Lab prototype commit
(`e33c2e2`) is a one-time exception the owner explicitly approved for this session only; it does not change the
"never commit the prototype folder" rule for any other ticket.
