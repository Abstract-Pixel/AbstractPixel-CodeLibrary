# Grand plan: game management, commercial-ready

Budget plan: `plan: gm-commercial` (task-budget tool). Total 77% of the 5-hour window over 9 sessions.
Read [DECISIONS.md](DECISIONS.md) and [DESIGN.md](DESIGN.md) before any session.

## Destination

The game management module is about 90% ready for the Unity Asset Store. A new game sets it up from the onboarding
window, mistakes show as problems with fixes, time can no longer soft-lock, and the runtime debugger shows every
transition. Rally Shift runs on it with every flow behaving as it does on Steam today, or better.

## Sessions

| # | Session | Kind | Budget | Precursor | Delivers |
|---|---|---|---|---|---|
| 1 | Foundation and parity baseline | implement | 8% | none | Branches, folder rename, Rally state assets moved to Rally, flow inventory, parity recorder and baseline (with per-frame transition samples), test assembly, README, GLOSSARY |
| 2 | Core state machine | implement | 10% | none | Plain-C# machine, catalog asset, queue, parents, effect resolution, journal, EditMode tests |
| 3 | Adapters, conditions, reactors, input source | implement | 10% | none | Unity effect sink, request adapters, state watch, library reactors, input source seam, asset migration tool |
| 4 | Rally migration, parity gate, delete old system | implement | 10% | none | Rally on the new system, Pause as sub-state of Play, merged Settings state, every Rally script and probe on `GameStates`, parity and transition break scenarios pass, old code deleted |
| 5 | UI design grill | grill | 4% | **is the precursor** for 6, 7, 8 | Detailed UI spec and prototypes for the settings page, inspectors, onboarding, debugger |
| 6 | Settings page, validator UI, watchdog | implement | 10% | Session 5 | Project Settings > Abstract Pixel > Game Management, inspector problems with fixes, play-mode watchdog |
| 7 | Onboarding window and menus | implement | 10% | Session 5 | Menu choice page, duplicate-or-attach paths (both tested), base menus with Settings button and back |
| 8 | Runtime debugger | implement | 7% | Session 5 | UI Toolkit overlay on the journal; Editor and Development Builds; release opt-in |
| 9 | In-Editor docs, samples, final review | docs | 8% | none | Documentation-engine topics, graphics-free samples, code review, final handoff |

**Branches.** Library and Rally work on `game-management-rework`, made from `main`. If the merge into `main` has not
landed when Session 1 starts, branch from `scene-system-rework` (library) and the current Rally branch, and say so in
the handoff.

## Priorities and deferred work

Must-haves, in this order: time can no longer soft-lock (Sessions 2-4), every Rally flow matches (Session 4), setup
problems are shown with fixes (Session 6), a new game can be set up from the onboarding window (Session 7).

Deferred to the execution agents, as nice-to-haves only when their session has budget left:
- Debugger polish: the before/after effect diff view, journal export, the release-build opt-in UI (Session 8).
- Moving `StateAudioVolumeController` into the library (Session 3); it can stay in Rally on the state watch.
- A compare script for parity recordings (Session 1); a manual diff of the JSON is enough.
- Renaming Rally's own `ProjectServices/GameManagment` folder (user decides after Session 1).
- Doc images and extra samples beyond one per main feature (Session 9).

## Transition break scenarios (Session 4 gate, also recorded as a baseline in Session 1)

Each runs through `bash AgentScripts/Probes/run_scenario.sh`, sampling every frame of the scene change:
`Time.timeScale`, the bomb timer value, the car position and the top state. Pass means the bomb does not tick and the
car does not move before the level screen is visible (`CarRelease` and `BombInitializer` frame), and nothing
soft-locks. Run each with both "no state active" options and record both results.

1. Game Over, then Retry (scene change starts with time 0 or mid-ease).
2. Game Win, then Next Level and Retry.
3. Pause, then Quit to Main Menu (time 0 at the start of the change).
4. Main Menu to a level through the loading screen (time 1 at the start).
5. A scene change requested during Game Over's 1.4 s delay and its ease.
6. Pause pressed many times during loading (the pause gate must still block it).
7. Retry spammed during a scene change.
8. Play from a lone level in the Editor (no state active on the first frame).
9. Level select back to Main Menu, then Settings, during and right after the change.

Sessions 1-4 must run in order. 6, 7 and 8 need Session 5, then can run in any order. 9 runs last.

**Precursor rule.** A session marked with a precursor starts only after that grill session has written a detailed
`Sessions/SessionN/OVERVIEW.md` and `PROMPT.md` for it. UI work always gets a precursor. If any session finds that game
management needs a sub-module of its own (the way the scene system has Relocation), stop, record it in the handoff, and
add a precursor planning session for that sub-module before it is built. None is expected today.

Each later session's `OVERVIEW.md` and `PROMPT.md` are written by the session before it (or its precursor) at the end,
using what was actually built, in the same format as Session 1's.

## Rules for every session

- Follow Rally's `CLAUDE.md`, the library's `CLAUDE.md` and the user's memory notes. Stay inside ProjectRallyShift,
  `../Abstract_Pixel_Lab` and the session scratchpad. Put this rule in every subagent brief.
- **Budget.** First tool call: `budget start` with `plan: gm-commercial` and the session's step. Pass the session's
  tasks as phases with the shares in its OVERVIEW. Each task has a **must-have** line. If the budget runs low, finish the
  must-haves so the system works and is tested, and list the skipped nice-to-haves in the handoff. Never leave Rally broken.
- **Rally must keep working** after every session: run the parity scenarios before finishing. Before Session 4's gate,
  the old system stays the one Rally uses.
- **Tests first** for logic (library rule). One test assembly: `Modules/GameManagement/Tests/`.
- **Code style:** verbose and readable, no `var`, max three parameters, small files (about 300-400 lines), composition
  over flags, plain error messages that say what, where and how to fix.
- **Serialized data survives:** `FormerlySerializedAs`, `git mv` with `.meta`, migrations that record Undo.
  Grep scenes and prefabs for names and GUIDs before removing anything. Revert unintended `GameManagment.prefab`
  bridge-ID churn and `SceneSystemSettings.asset` rewrites.
- **Both Editors:** the library also compiles in `../Abstract_Pixel_Lab` (Unity 7) with no new warnings.
- **Commits:** only when the user allows it in the launch message. Then: library commits per task with the session ID
  first, Rally commits only the submodule pointer and Rally files.
- **Handoff.** Every session ends by writing `Sessions/SessionN/HANDOFF.html`, a one-page HTML document with: code
  changes (files, what and why), how it affects a developer using the module, what was not done or moves to another
  session, findings and improvement ideas, budget used versus allotted and whether a warning fired. Also update
  [Wayfinder/map.md](Wayfinder/map.md), resolve finished tickets, and write the next session's OVERVIEW and PROMPT.
