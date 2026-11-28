# Ticket 04 handoff: stability gate

`plan: gm-finish, step 4`. 2026-11-28. Closed.

## Runs

| Check | Result |
|---|---|
| Rally (6.3) `AbstractPixel.GameManagement.Tests` EditMode | 77/77 |
| Rally parity scenarios | 9/9 PASS |
| Lab (7.0.0a16) same EditMode tests | 77/77 |
| Lab Sample 01 PlayMode flow test | 1/1 after the fix, run twice |
| Unity 6.7.0b4 library compile | 0 errors, 25 warnings, none in GameManagement |

Parity notes:
- `parity_menu` and `parity_menu_spam` first showed BLOCKED (save isolation failed) right after a run I had stopped; both passed on rerun. An earlier all-BLOCKED run was my mistake: its log used the runner's own report path.
- Baseline compare: with the car held, `pause_quit` has two small bomb ticks, and the baseline has the same two. HUD changes happen in the same order; only the snapshot taken during Game Over/Win's delay differs (HUD now changes after the delay, DECISIONS.md). Other differences are state names and Settings merge, input maps by reference, and Back on Pause resuming (Session 4). I did not explain every differing field line by line.

6.7 method: throwaway project in the session scratchpad, a copy of library `main` plus the fixes, Unity packages from Rally's manifest minus ones that do not build on 6.7 (2D, ProBuilder, AI Navigation, terrain tools and others, Addressables), BroAudio copied from the Lab. So Addressables-dependent code was not checked on 6.7, and no tests or Play mode ran there. `game-management-rework`'s older save system was not compiled on 6.7.

## Fixed

- **Flow test failed in the Lab** ("Esc in Play opens Pause"): an unfocused Editor reset the test keyboard. Then 8 steps differed only in cursor lock: the cursor depends on the active device, which came from the machine's hardware. The test now ignores focus and starts with a virtual gamepad as the active device. No assertion or baseline changed.
- **6.7 errors:** two SaveSystem editor files used instance-ID APIs. Their guards now start at `UNITY_6000_7_OR_NEWER`.

No change to what a developer using the module sees at runtime.

## Owner answers

Code on `game-management-rework`, merged into `main`; docs on `main`. Push per ticket. Recorded in DECISIONS.md, PLAN-OVERVIEW and the library CLAUDE.md. Close with the 6.7 warnings carried to ticket 11.

## Not done

- 6.7 warnings and the missing-BroAudio behaviour: see ticket 11, "From ticket 04".
- Lab and Rally submodule pointers are not committed.

## Budget

3% planned, raised by 5% to 8% (owner). The research phase overran (1.2% of 0.5%); the wrap point fired once before the raise and a warning fired near 5.9%. About 6% used at the time of writing.
