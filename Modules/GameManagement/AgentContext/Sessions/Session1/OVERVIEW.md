# Session 1 overview: foundation and parity baseline

`plan: gm-commercial, step 1`. Budget: **8%** of the 5-hour window, wrap reserve 1.5%. Kind: implement.

## Goal

Make the module safe to change: fix the folder name, write down every flow Rally depends on, and record how Rally
behaves today so Session 4 can prove the new system matches it. Add the test assembly, README and GLOSSARY.
No behaviour change to the game in this session.

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 1 | Branches, folder rename, move Rally's state assets | 1.5% | `game-management-rework` branches made (from `main`, or the fallback); library folder and asmdef file renamed with `git mv` (metas kept); the 7 state assets moved to `Assets/[1]Project/ProjectServices/GameManagment/States/` with their metas; Rally and Lab compile; no missing scripts or references | Report how many references Rally's own `ProjectServices/GameManagment` folder has, for the user's decision |
| 2 | Flow inventory and Rally helper audit (tickets 01, 03) | 2% | `AgentContext/FLOWS.md`: every flow with trigger, states, and expected time, cursor, menus, HUD, input maps, audio; table of Rally scripts and UnityEvent calls depending on the module | Notes on flows that look buggy today |
| 3 | Parity recorder and baseline | 3% | A probe that snapshots state stack, `Time.timeScale`, cursor, active menus, HUD, enabled action maps after each step, and samples every frame of a scene change (time scale, bomb timer, car position, top state); scenarios for the user's flows and the 9 transition break scenarios in PLAN-OVERVIEW; baseline saved under `AgentContext/Parity/` | Scenarios for the remaining flows; diff script (deferred if short) |
| 4 | Test assembly, README, GLOSSARY | 1% | `Modules/GameManagement/Tests/AbstractPixel.GameManagement.Tests.asmdef` (Editor only) with one passing test; README with what the module does today and the plan link; GLOSSARY from DESIGN.md's word table | |
| 5 | Handoff and next session | 0.5% | `Sessions/Session1/HANDOFF.html`, map updated, `Sessions/Session2/OVERVIEW.md` and `PROMPT.md` | |

## Done when

All must-haves pass, Rally plays every flow unchanged, both Editors compile with no new warnings, and the handoff exists.
