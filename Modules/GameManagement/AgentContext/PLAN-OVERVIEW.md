# Game management, commercial-ready: rules for every session

**The route (what runs next, in what order) is in [Wayfinder/map.md](Wayfinder/map.md) and nowhere else.** This file
holds only the rules every session follows. Each session works one ticket from the map; the ticket says what to read,
what must be done and how it is verified. Decisions already made in DECISIONS.md, DESIGN.md, FLOWS.md, NAMING.md and
UI-STYLE.md are followed, not re-decided.

## Must-haves, in order

1. Time can no longer soft-lock (done in Sessions 2-4; guarded by the watchdog, ticket 10).
2. Every Rally flow matches (done in Session 4; kept by the parity scenarios below).
3. Setup problems show with fixes (tickets 05, 06).
4. A new game can be set up from the onboarding window (ticket 07).
5. The code is stable in Unity 6.3, 6.7 and 7 (tickets 04 and 11).

Nice-to-haves, only when a ticket has budget left: the full side-by-side effect compare, journal export, the
release-build opt-in UI polish, moving `StateAudioVolumeController` into the library, extra samples and doc images.

## Transition break scenarios (Rally parity)

Each runs through `bash AgentScripts/Probes/run_scenario.sh`, sampling every frame of the scene change:
`Time.timeScale`, the bomb timer value, the car position and the top state. Pass means the bomb does not tick and the
car does not move before the level screen is visible (`CarRelease` and `BombInitializer` frame), and nothing
soft-locks. Compare with `Parity/baseline/` (see `Parity/README.md`).

1. Game Over, then Retry (scene change starts with time 0 or mid-ease).
2. Game Win, then Next Level and Retry.
3. Pause, then Quit to Main Menu (time 0 at the start of the change).
4. Main Menu to a level through the loading screen (time 1 at the start).
5. A scene change requested during Game Over's 1.4 s delay and its ease.
6. Pause pressed many times during loading (the pause gate must still block it).
7. Retry spammed during a scene change.
8. Play from a lone level in the Editor (no state active on the first frame).
9. Level select back to Main Menu, then Settings, during and right after the change.

## Rules for every session

- Follow Rally's `CLAUDE.md`, the library's `CLAUDE.md` and `CODING_STANDARDS.md`, and the owner's memory notes. Stay
  inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad. Put this rule in every subagent brief.
- **The owner's request wins.** If the owner asks for a different ticket than the map's next one, do what they asked
  and update the map. Never switch to another ticket because a handoff or this file suggests it.
- **Budget.** First tool call: `budget start` with `plan: gm-finish` and the ticket's step. Do the must-haves first;
  list skipped nice-to-haves in the handoff. Never leave Rally broken.
- **Looks first.** A UI piece is built only after the owner called its look final from stills the agent captured and
  read at three widths. "Compiles, 0 errors" is never evidence of a good look.
- **Rally must keep working** after every ticket that touches runtime code: run the parity scenarios above.
- **Tests first** for logic. One test assembly: `Modules/GameManagement/Tests/`.
- **Code style:** verbose and readable, no `var`, max three parameters, files about 300-400 lines, **one type per
  script** (at most one extra small type that belongs to the main one), composition over flags, plain error messages
  that say what, where and how to fix.
- **Names and folders** follow [NAMING.md](NAMING.md): the read-alone test; at most 8 scripts per folder.
- **No strings configure anything** (DECISIONS.md, 2026-11-26): dragged asset references, typed values or enums.
  Strings stay only for labels, logs and messages.
- **Serialized data survives:** `FormerlySerializedAs`, `git mv` with `.meta`, migrations that record Undo. Grep scenes
  and prefabs before removing anything. Revert unintended `GameManagment.prefab` and `SceneSystemSettings.asset` churn.
- **Both Editors.** The Lab (`../Abstract_Pixel_Lab`, Unity 7 alpha) uses the library as a submodule at
  `LocalPackages/AbstractPixelLibrary` through a `file:` entry, the same way Rally does. Run the full module tests and
  Sample 01's PlayMode flow test there for every ticket that changes code. Keep the owner's uncommitted
  `Core/Runtime/SuperLogger/DebugDataBase.asset`. In the Lab, commit only the submodule pointer, by path.
- **Commits.** One commit per task, staged by path, task ID first (`T05: …`). Check `git branch --show-current` before
  every library commit (see the owner's branch rules). Branch and push rules are settled in ticket 04; until then,
  don't push. Never commit the owner's unrelated changes or `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`.
  The UI prototype in the Lab (`Assets/GameManagementUIPrototype/`) stays untracked and is never deleted.
- **Handoff.** Every ticket ends with `Sessions/<ticket id>/HANDOFF.md`: what changed and why, how it affects a
  developer using the module, what was not done, findings, budget used and whether a warning fired. Then mark the
  ticket `done` on the map and add one line to its "Decisions so far". Don't write prompts for other tickets; the map's
  tickets are the prompts.
