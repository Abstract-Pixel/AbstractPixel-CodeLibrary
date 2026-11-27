# Grand plan: game management, commercial-ready

Budget plan: `plan: gm-commercial` (task-budget tool). Total 85% of the 5-hour window over 9 sessions. Session 4 runs in two parts, 4 and 4B, sharing **18%** (set 2026-11-26).
Every session reads this file's rules, its own OVERVIEW and PROMPT, and the previous handoff. Everything else is read
stage by stage: each PROMPT names the document (and section) to read **before** each stage, so decisions already made
in DECISIONS.md, DESIGN.md, FLOWS.md and NAMING.md are followed, not re-decided.

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
| 4 | Sample game, Rally migration, parity gate, delete old system | implement | 13% of Session 4's 18% | none | A small sample game copying Rally's flows (menus reusable as a setup template) built on the old system first, with a Play-mode flow test that records a baseline in Unity 6.3 and the Unity 7 Lab, then migrated and passing the same test in both; Rally on the new system, Pause as sub-state of Play, merged Settings state, every Rally script and probe on `GameStates`, parity and transition break scenarios pass, old code deleted |
| 4B | Naming, folders and core cleanup (second part of Session 4) | refactor | 5% of Session 4's 18% | none | Every name passes NAMING.md's read-alone test (owner approves the rename table), no folder above 8 scripts, readability pass with no behaviour change, docs in step |
| 5 | UI design grill | grill | 4% | **is the precursor** for 6, 7, 8 | Detailed UI spec and prototypes for the settings page, inspectors, onboarding, debugger |
| 6A | Project Settings page (UI sub-session) | implement | per sub-session | Session 5 | Owner finalizes the look from prototype 01/02, then the real Project Settings > Abstract Pixel > Game Management page with its problem list |
| 6B | State inspector and state field drawer (UI sub-session) | implement | per sub-session | 6A | Look from prototypes 03-05, then the real inspector and drawer |
| 6C | Play-mode watchdog | implement | per sub-session | 6A | The watchdog from the old Session 6 row |
| 7A | Onboarding window and menus (UI sub-session) | implement | per sub-session | 6B | Look from prototypes 06-08, then the window, menu choice page and both setup paths |
| 8A | Debugger overlay (UI sub-session) | implement | per sub-session | 7A | Look from prototype 09, then the in-game overlay; Editor and Development Builds; release opt-in |
| 8B | Game State Debugger window (UI sub-session) | implement | per sub-session | 8A | Look from prototype 10, then the window with changed-only diff lines |
| 9 | In-Editor docs, samples, final review | docs | 8% | none | Documentation-engine topics, graphics-free samples, code review, final handoff |

**Branches.** Library and Rally work on `game-management-rework`, made from `main`. If the merge into `main` has not
landed when Session 1 starts, branch from `scene-system-rework` (library) and the current Rally branch, and say so in
the handoff.

**2026-11-27 change:** Sessions 6, 7 and 8 are split into one sub-session per UI piece (6A-8B above). In each one the owner finalizes the look from captured, agent-checked stills before the real UI is built. Why and how: [Sessions/Session5/HANDOFF.md](Sessions/Session5/HANDOFF.md). The old 10%/10%/7% budgets are shared out per sub-session at its start.

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

Sessions 1-4, then 4B, must run in order. 4B comes after the old code is gone (less to rename) and before the UI
sessions build on today's names. 6, 7 and 8 need Session 5, then can run in any order. 9 runs last.

**Precursor rule.** A session marked with a precursor starts only after that grill session has written a detailed
`Sessions/SessionN/OVERVIEW.md` and `PROMPT.md` for it. UI work always gets a precursor. If any session finds that game
management needs a sub-module of its own (the way the scene system has Relocation), stop, record it in the handoff, and
add a precursor planning session for that sub-module before it is built. None is expected today.

Each later session's `OVERVIEW.md` and `PROMPT.md` are written by the session before it (or its precursor) at the end,
using what was actually built, in the same format as Session 1's.

**Every session prompt has its own "Done and verified" section.** DEFINITION-OF-DONE.md is the general guideline for
all sessions; it does not say what finished means for one session. So each `PROMPT.md` (and its OVERVIEW's "Done when")
must also state, for that session only: the concrete end state (which types, tools or behaviours exist), and how each
is verified (which tests by name or area, which commands, which parity scenarios or manual checks, with what result).
A session is finished only when both the general guideline and its own section hold. Each prompt also repeats the
one-type-per-script rule in its Rules.

## Rules for every session

- Follow Rally's `CLAUDE.md`, the library's `CLAUDE.md` and the user's memory notes. Stay inside ProjectRallyShift,
  `../Abstract_Pixel_Lab` and the session scratchpad. Put this rule in every subagent brief.
- **Budget.** First tool call: `budget start` with `plan: gm-commercial` and the session's step. Pass the session's
  tasks as phases with the shares in its OVERVIEW. Each task has a **must-have** line. If the budget runs low, finish the
  must-haves so the system works and is tested, and list the skipped nice-to-haves in the handoff. Never leave Rally broken. If the scope check asks for
  more budget, keep the given one and do the must-haves first.
- **Rally must keep working** after every session: run the parity scenarios before finishing. Before Session 4's gate,
  the old system stays the one Rally uses.
- **Tests first** for logic (library rule). One test assembly: `Modules/GameManagement/Tests/`.
- **Code style:** verbose and readable, no `var`, max three parameters, small files (about 300-400 lines), **one type per script** (each class, struct, enum or interface in its own file, in folders by
  part; at most one extra small type when it belongs to the main one; never several types dumped in one script, as the
  old `Runtime/Machine/Effects.cs` did), composition
  over flags, plain error messages that say what, where and how to fix.
- **Names and folders** follow [NAMING.md](NAMING.md): a name passes the read-alone test (what it is, and that it is
  game management, from the file name alone); at most 8 scripts per folder.
- **No strings configure anything** (DECISIONS.md, 2026-11-26). A developer or agent never types a string to set
  something up: no string IDs, names or keys in inspector fields, settings, or the public API's setup calls. Use
  dragged asset references (states, `InputActionReference`), typed values or enums instead. Strings stay only for
  labels, logs and messages (for example a transition's requester name).
- **Serialized data survives:** `FormerlySerializedAs`, `git mv` with `.meta`, migrations that record Undo.
  Grep scenes and prefabs for names and GUIDs before removing anything. Revert unintended `GameManagment.prefab`
  bridge-ID churn and `SceneSystemSettings.asset` rewrites.
- **Both Editors:** `../Abstract_Pixel_Lab` (Unity 7000.0.0a16) has the library as a git submodule at
  `LocalPackages/AbstractPixelLibrary` and uses it as a local package (`"com.abstractpixel.library":
  "file:../LocalPackages/AbstractPixelLibrary"` in `Packages/manifest.json`), the same way Rally does. Its checkout
  is on `main` today (old `GameManagment` only). After committing and pushing the library, move the Lab's submodule to
  the same commit: `git -C ../Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary fetch` then `checkout <commit>`.
  Unity picks the change up on its next refresh; nothing is imported or copied. Keep the owner's uncommitted change in
  that checkout (`Core/Runtime/SuperLogger/DebugDataBase.asset`); if git refuses, stop and ask. In the Lab, commit only
  the submodule pointer, by path; never the owner's other Lab changes. Then run the full module EditMode tests and the
  sample's PlayMode flow test in the Lab Editor. No throwaway copies, no compile-only check.
- **Commits by default** (decided 2026-11-26): commit as you go, do not leave work staged for the owner. Library
  commits per task on `game-management-rework` with the session ID first, pushed at the end of each task; Rally commits
  only the submodule pointer and Rally files. Never commit the owner's unrelated changes; stage paths by name.
- **Handoff.** Every session ends by writing `Sessions/SessionN/HANDOFF.html`, a one-page HTML document with: code
  changes (files, what and why), how it affects a developer using the module, what was not done or moves to another
  session, findings and improvement ideas, budget used versus allotted and whether a warning fired. Also update
  [Wayfinder/map.md](Wayfinder/map.md), resolve finished tickets, and write the next session's OVERVIEW and PROMPT.
