# Engineering, context and check rules

Every game management session follows this file, in a mega run or not. It is the only home of these rules.
Read [Tools/TRAPS.md](Tools/TRAPS.md) with it.

## Code, repos and commits

- Follow Rally's `CLAUDE.md`, the library's `CLAUDE.md` and `CODING_STANDARDS.md`, and the owner's memory notes. Stay
  inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad. Put this rule in every subagent brief.
- **Rally must keep working** after every ticket that touches runtime code: run the parity scenarios in
  [PLAN-OVERVIEW.md](PLAN-OVERVIEW.md), section "Transition break scenarios" only.
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
  every library commit. Game management code goes on `game-management-rework` and is merged into `main`, never the other
  way; docs and the map go on `main` only. When a ticket closes and its tests pass, push the library branches it
  changed (DECISIONS.md, 2026-11-28). Never commit the owner's unrelated changes or `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`.
  The UI prototype in the Lab (`Assets/GameManagementUIPrototype/`) stays untracked and is never deleted.

## Context limit

Long sessions were the largest token cost: most cache reads came from calls above 200k context.

- After each ticked step, read your context size: `get_usage` (`mcp__ccd_session_mgmt__get_usage`, `session_id`
  `self`; load it with ToolSearch if needed), field `context.tokensUsed`.
- **At 275,000 or more** (owner, 2026-11-28): finish the item in your hands. Do not
  start another. If only the final commit and report
  are left, finish them.
- Then hand over. Update your progress file with a `## Handover` section: done, next item, open findings with how to
  reproduce them, uncommitted paths and why, and Editor and temporary-setting state. Commit finished work. Never
  leave a half-made edit.
- A mega run session reports `STATUS: CONTINUE`. Any other session tells the owner to start a fresh one.
- The fresh session reads its brief, the spec, the progress file and `CHECKS.md`. It does not redo ticked items.
- If you cannot read your context size, say so in your report, and hand over after finishing a major step.

## Check record: `<session folder>/CHECKS.md`

One record per ticket, shared by the builder, both verifier parts and later fix sessions. Add a row as soon as a
check runs or is waived. Update the row on a rerun; never add a second row for the same Id.

| Id | Check | Result | Commit | Unity | Input | Stale if | Evidence |
|---|---|---|---|---|---|---|---|
| example | Pause opens with Esc in Sample 01 | PASS | 4f1cd03 | 7.0 | simulated device | Modules/GameManagement/Runtime Modules/GameManagement/Samples | stills/pause.png |

- **Result:** `PASS`, `FAIL`, `UNVERIFIED` (not run, or waived by the owner: say which in Evidence),
  `NOT POSSIBLE UNATTENDED` or `NOT CHECKED`. A waived check is `UNVERIFIED`, never `PASS`.
- **Commit:** the library commit that holds the code you checked. Commit first when you can. A check run on
  uncommitted code is rerun, or its row updated, after the commit.
- **Unity:** the Editor or player version. A pass in one version says nothing about another.
- **Input:** `real`, `simulated device`, `code call` or `none`.
- **Stale if:** library paths, separated by spaces, with no spaces inside a path (use a parent folder). List every
  folder whose change could break the check, including code it depends on. When unsure, list more.
- **Skipping a check:** run `bash Modules/GameManagement/AgentContext/Tools/check_stale.sh <CHECKS.md>` from the
  library. Skip only `VALID` rows in the same Unity version. Rerun `STALE` rows you are responsible for.
- The tool sees only the library. A change in a game project (Rally's scenes, assets or settings) or to packages or
  project settings makes the affected rows stale: judge that yourself and rerun.
- **Always rerun at ticket close, even when VALID:** the module test assembly, a clean compile, and the affected
  parity scenarios when the step changed runtime files.
