# Session 4B prompt: Naming, folders and core cleanup

You are an autonomous agent working in ProjectRallyShift (Unity 6000.3) on the AbstractPixelLibrary submodule's game
management module. plan: gm-commercial, step 4 (second part). Budget 5% of the 5-hour window.

## What "finished" means

`LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/DEFINITION-OF-DONE.md` is the general
guideline; read it first. This session is finished only when it holds **and** the section below holds. Anything cut goes
in the handoff's "Not done" table.

## Done and verified (this session)

- **Rename table approved:** the audit table (old name, what it does, new name, folder) is in the handoff, and the
  owner approved it before any rename. The three flagged names (`WhenLeavesGoTo`, `FollowUpRule`, `RisingEdge`) are in it.
- **Names pass:** every public type in `Runtime/` and `Editor/` passes NAMING.md's read-alone test. A grep for each old
  name finds only `[MovedFrom]` / `[FormerlySerializedAs]` attributes and the handoff.
- **Folders pass:** `find Runtime Editor Tests -name '*.cs' | xargs -n1 dirname | sort | uniq -c` shows no folder above 8.
- **No behaviour change:** full `AbstractPixel.GameManagement.Tests` passes with the same count as Session 4, 77 (renamed
  tests allowed); the sample's flow test matches its baseline in Rally and in the Lab (Unity 7, submodule moved to the
  pushed commit, full module tests too); parity scenarios
  `parity_pause`, `parity_gameover`, `parity_win`, `parity_menu` pass against `Parity/baseline/`.
- **Rally intact:** Rally's scenes and prefabs keep every game management component (no missing scripts; grep the
  GUIDs before and after); `editor_cycle.sh noplay` clean.
- **Docs in step:** GLOSSARY, README, DESIGN mapping and NAMING.md use the new names.
- Every new or touched script keeps one type per script.

## Starting point

Session 4 is done: the old system is deleted. Read "Starting point" in `OVERVIEW.md` for what is built, what was last
checked and what was not. Nothing is pushed. Push only if the owner says so in this session; without a push, update the
Lab with `git fetch <Rally's library folder> game-management-rework` and merge `FETCH_HEAD` into its `main`. This
replaces the push steps below.

## Read first

`AgentContext/PLAN-OVERVIEW.md` (rules), `DEFINITION-OF-DONE.md`, `Sessions/Session4/HANDOFF.html`, this session's
`OVERVIEW.md`, the library's and Rally's `CLAUDE.md`. The rest is read stage by stage below.

## Before each stage, read this first

Read the named documents **before** starting the stage, so you follow decisions already made instead of making your
own. If something is not covered, stop and ask the owner rather than deciding it.

| Stage | Read first |
|---|---|
| 1 audit | NAMING.md (all of it), GLOSSARY.md, the module's `README.md` |
| 2 folders | NAMING.md "Folders"; PLAN-OVERVIEW.md "Serialized data survives" |
| 3 renames | The approved rename table; PLAN-OVERVIEW.md "Serialized data survives"; DESIGN.md old-to-new mapping |
| 4 readability | The library's `CLAUDE.md` code style; PLAN-OVERVIEW.md "Code style"; DESIGN.md (so no seam changes) |
| 5 handoff | DEFINITION-OF-DONE.md "A session is finished"; `Wayfinder/issues/02-ui-design-grill.md` for Session 5's prompt |

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 4`, `kind: "refactor"`, `budgetPct: 5`,
  `wrapReservePct: 1`, phases `audit 1, folders 0.75, renames 1.25, readability 1, handoff 1`. If the scope check fires,
  do the must-haves first and spend what is left on the rest.
- Check `unity command editor_status`, `git status` in both repos; hash the owner's uncommitted files (the Lab has
  `Core/Runtime/SuperLogger/DebugDataBase.asset`).
- The Lab's Editor uses other command names, run from the Lab folder: `unity command editor status`,
  `unity command script recompile`, `unity command test run --mode editor --filter <assembly> --filter-type Assembly`;
  for Play mode add `--mode playmode --async-tests true`, then `unity command test status --detail summary`.
- Keep the Editor's focus alone while parity or Play-mode tests run; run one Editor's tests at a time.
- Do the audit first and ask the owner to approve the table in **one** question round; rename nothing before that.

## Check

- Tests: `unity command run_tests --mode editor --filter AbstractPixel.GameManagement.Tests --filter_type assembly`.
- Unity 7 as in PLAN-OVERVIEW.md "Both Editors": push, move the Lab's submodule, run full tests there. Parity: `bash AgentScripts/Probes/run_scenario.sh <name>`.

## Finish

`Sessions/Session4B/HANDOFF.html`, update `Wayfinder/map.md`, write `Sessions/Session5/OVERVIEW.md` and `PROMPT.md`
(the UI grill precursor; link DEFINITION-OF-DONE.md, with their own "Done and verified" section), `budget end`. Commit and push
by default (PLAN-OVERVIEW.md); the handoff lists the commits.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent brief.
- No behaviour change. A fix that changes behaviour goes in the handoff as a finding instead.
- One type per script: each class, struct, enum or interface in its own file, in folders by part; at most one extra
  small type when it belongs to the main one.
- Names follow `NAMING.md`: the read-alone test, two to four words, no near-twins; at most 8 scripts per folder.
- Rally must behave as on Steam today. Never touch live Steam leaderboards or stats.
- If the budget runs low: finish the must-haves, keep Rally working, and list the rest in the handoff.
- **No strings configure anything** (DECISIONS.md, 2026-11-26). A developer or agent never types a string to set
  something up: no string IDs, names or keys in inspector fields, settings, or the public API's setup calls. Use
  dragged asset references (states, `InputActionReference`), typed values or enums instead. Strings stay only for
  labels, logs and messages (for example a transition's requester name).
