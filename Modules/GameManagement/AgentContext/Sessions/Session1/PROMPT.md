# Session 1 prompt: game management foundation and parity baseline

You are an autonomous agent working in ProjectRallyShift (Unity 6000.3) on the AbstractPixelLibrary submodule's game
management module. plan: gm-commercial, step 1. Budget 8% of the 5-hour window.

## Read first

1. `LocalPackages/AbstractPixelLibrary/Modules/GameManagment/AgentContext/README.md`, then `PLAN-OVERVIEW.md`,
   `DECISIONS.md`, `ANALYSIS.md` and `Sessions/Session1/OVERVIEW.md`. Skim `DESIGN.md` (you implement none of it now).
2. Rally's `CLAUDE.md` and the library's `CLAUDE.md`. `AgentScripts/Tools/UNITY_CLI_QUICKREF.md` before driving the Editor.

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 1`, `kind: "implement"`, `budgetPct: 8`,
  `wrapReservePct: 1.5`, and phases `rename 1.5, inventory 2, parity 3, tests-docs 1, handoff 0.5` (shares). Call
  `phase` each time you move to the next task.
- Check `unity command editor_status` and that no other agent drives the Editor. Run
  `unity command set_autotick --enable true`.

## Task 1: branches, folder rename, state assets

- Branches: in the library and in Rally, create `game-management-rework` from `main`. If `main` does not yet contain
  the scene-system merge (compare with `scene-system-rework`), branch from `scene-system-rework` in the library and
  from the current Rally branch, and note it in the handoff. Do not disturb uncommitted work in the tree: check
  `git status` first and stop to ask if switching branches would touch it.

- In the library repo: `git mv Modules/GameManagment Modules/GameManagement` (the folder `.meta` moves with it; check),
  and rename `Runtime/AbstractPixel.GameManagment.asmdef` to `AbstractPixel.GameManagement.asmdef` (its `name` is
  already `AbstractPixel.GameManagement`). GUIDs must not change.
- Grep Rally (`Assets`, `AgentScripts`, `.agents`, `.claude`) and the library for the old module path
  `Modules/GameManagment` and fix path references in code, scripts and docs. Rally's own folder
  `Assets/[1]Project/ProjectServices/GameManagment` is a different folder: do not rename it; count its references for the
  handoff.
- Recompile with `bash AgentScripts/Tools/editor_cycle.sh noplay`. Check the console for errors and missing scripts in
  MainMenu, levels 2-4 and `GameManagment.prefab`. Check `../Abstract_Pixel_Lab` compiles (it uses the library through
  a `file:` package; see Rally CLAUDE.md "Unity 7 checks").
- Move Rally's 7 state assets (`Runtime/StateConfigurations/*.asset` with their metas) to Rally's
  `Assets/[1]Project/ProjectServices/GameManagment/States/`. They cross from the submodule into Rally, so `git mv`
  cannot carry them. Copy each `.asset` with its `.meta` (same GUID) into Rally, delete the library copies, then
  refresh and check that MainMenu, levels 2-4 and `GameManagment.prefab` still reference them (grep their GUIDs before and after).
- Must-have: renamed, assets moved, both Editors compile, nothing missing.

## Task 2: flow inventory (wayfinder tickets 01 and 03)

- Find every user of the module: `GameStateRegistry`, `StateConditionRegistry`, conditions, reactors (use the LSP
  `findReferences` first, grep `AgentScripts/`). Grep `.unity` and `.prefab` files for `m_MethodName: ActivateState`,
  `DeactivateState`, `SkipExecutionDelay` (UnityEvent buttons) and for the script GUIDs of every module component.
- Write `AgentContext/FLOWS.md`: one row per flow (main menu, level select, settings from menu and pause, pause and
  unpause, game over by bomb and by death, death skip, game win, retry, next level, back to menu, scene changes, loading
  screens, controller vs mouse cursor, input map changes, audio per state, HUD per state, and anything else you find).
  Columns: trigger, state before, state after, then time scale, cursor, menu shown, HUD, input maps, audio. Mark how it is
  triggered so a scenario can reproduce it.
- Add the Rally helper table: script, what it does with game management, and its future (library reactor, effect,
  condition, or stays in Rally on the state watch) per DESIGN.md's old-to-new table.
- Mark tickets 01 and 03 resolved (append `## Answer` pointing at FLOWS.md) and add a line to the map's Decisions so far.
- Must-have: every flow listed with expected values.

## Task 3: parity recorder and baseline

- Use the scenario system: `bash AgentScripts/Probes/run_scenario.sh <name>`, JSON files in `AgentScripts/Probes/Scenarios/`.
  Read existing scenarios and probes first and reuse them. Wait on `waitUntil` conditions, never fixed delays.
- Add one recorder probe in `AgentScripts/Probes/` that writes a JSON snapshot after each step: active states and the
  top state (through the registry's public API), `Time.timeScale`, `Cursor.visible` and `Cursor.lockState`, which
  menu roots are active, HUD active, enabled action maps on `GlobalInput.PlayerInputActions`. During scene changes it
  also samples every frame: `Time.timeScale`, the bomb timer value, the car position and the top state. Keep it read-only.
- Write one scenario per flow in FLOWS.md (group short ones). Save the baseline recordings under
  `AgentContext/Parity/baseline/`. Write how to re-run and compare in `AgentContext/Parity/README.md`.
- After probes: re-enable keyboard and mouse if a probe parked them, and revert unintended churn
  (`AgentScripts/Tools/revert_probe_churn.sh`, `GameManagment.prefab` bridge IDs, `SceneSystemSettings.asset`).
- Also record the 9 transition break scenarios listed in PLAN-OVERVIEW (baseline for Session 4's gate).
- Must-have: every flow in the user's list recorded (menu and settings both ways, level select to menu to settings,
  play, pause, pause settings, game over, game win, win to play, play to game over). Nice-to-have: all other flows plus a
  compare script.

## Task 4: test assembly, README, GLOSSARY

- `Modules/GameManagement/Tests/AbstractPixel.GameManagement.Tests.asmdef`: Editor platform only, references the
  runtime asmdef and NUnit, `autoReferenced: false`, `UNITY_INCLUDE_TESTS` constraint. One passing test (for example
  `StateSO.GetExecutionTimeScaleTarget`). Run it with the unity-pipeline loop. This is the only test assembly the module
  will have.
- `Modules/GameManagement/README.md`: what the module does today, its pieces, how Rally uses it, the link to AgentContext.
- `Modules/GameManagement/GLOSSARY.md` from DESIGN.md's word table, in the domain-modeling glossary format.

## Task 5: handoff

- Write `Sessions/Session1/HANDOFF.html` (one page, plain HTML and inline CSS): code changes, effect on developers,
  not done and why, findings (buggy flows, surprises), the Rally folder reference count, budget used versus 8% and
  whether a warning fired.
- Write `Sessions/Session2/OVERVIEW.md` and `PROMPT.md` in this same format for "Core state machine" (10%): plain-C#
  machine with the interface in DESIGN.md, catalog asset, request queue, parents and suspend, effect resolution,
  transition journal, all through EditMode tests. Base it on what you learned. Include DECISIONS round 3 rules (Pause as sub-state of Play, delay semantics, parent
  closing, "no state active" option with keep-last as default, user-chosen settings asset location).
- `budget end`. Commit only if the user's launch message allows it.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent brief.
- No game behaviour changes this session. Never touch live Steam leaderboards or stats.
- If the budget runs low: finish the must-haves, keep Rally working, and list the rest in the handoff.
