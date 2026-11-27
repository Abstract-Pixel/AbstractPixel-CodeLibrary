# 04 Stability gate before any build

Type: task (AFK; the owner answers two questions). Blocked by: none. Must close before tickets 05-12.
Budget: `plan: gm-finish, step 4`.

## Goal

Prove today's code is green in every Editor before UI code lands on it, and settle where code is committed.

## Work

1. Rally (Unity 6.3): the full `AbstractPixel.GameManagement.Tests` EditMode run and every parity scenario in
   `../../Parity/README.md`, compared with `../../Parity/baseline/`. Open since Session 4B.
2. Lab (Unity 7): the full module EditMode tests and a clean rerun of Sample 01's PlayMode flow test. Open since
   Session 4B (the last run failed on cursor-lock state only).
3. Unity 6.7: compiles with no errors or warnings, or say why it could not be checked.
4. Ask the owner, then record both answers in `../../DECISIONS.md` and PLAN-OVERVIEW's rules:
   - **Branch.** The code rule is: commit on `game-management-rework`, merge into `main`, never the other way. But
     the Session 5-6B docs and this map live on `main` only, and the Lab checkout is on `main`. Choose where docs and
     code go from now on.
   - **Push.** PLAN-OVERVIEW used to say push per task; the library CLAUDE.md says never push. Choose one.
5. Fix any failure, tests first. A failure is never written off as "likely noise" without a clean rerun.

## Done and verified

- Every run above passes, with counts in the handoff, or each failure was fixed and rerun.
- The branch and push answers are written down.
