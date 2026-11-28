# Mega run 07-09 ledger (chief only)

## Preflight (2026-11-28)

| # | Check | Result |
|---|---|---|
| 1 | Chief model, effort, mode | Sonnet 5.5, **high** (expected medium; chief cannot change it), auto |
| 2 | CLI | 2.1.329, pass |
| 3 | Verifier loads | pass (probe 8306c838-2980-4dff-8126-d7799e576c21, $0.21, quoted the brief's first heading) |
| 4 | Effort audit | pass (probe session medium, gm-verifier high, only claude-sonnet-5-5) |
| 5 | Editors | pass. LAB answers `unity command editor status` (`editor_status` is not a command there); RALLY answers `editor_status` |
| 6 | Usage | 5-hour 2% (resets 2026-11-28 16:30 UTC); weekly 82% (stop at 97%, resets 2026-12-02 16:00 UTC); extra usage off |
| 7 | Owner's files | saved to Logs/mega-07-09/start-state.txt (LAB, LIB, RALLY, RALLY LIB) |

Keep-awake: requested (session_idle), granted.

## Runs

| Step | Session id | N | Cap USD | Cumulative USD | 5h % before | 5h % after | Weekly % before/after | Outcome | Extension reason |
|---|---|---|---|---|---|---|---|---|---|
| S0 | a192a129-9c2c-4ef0-8fc8-fb386a057f8b | 1-2 | 12 | 0.97 (N=2 only) | 2 | 16 | 82 / 83 | gate PASS, verifier waived | |
| S1 | 95ba92f3-5e40-42e0-98ca-83606a7598ac | 1-2 | 25, then 6 | 0.63 (N=2 only) | 16 | 75 | 83 / 91 | gate PASS, verifier waived | owner change |

## Owner changes during the run

- 2026-11-28: from S1 on, nested sessions must be visible desktop sessions the owner can open and message
  (`start_session`), overriding MEGA-TICKET "Mechanism". S0 stays headless and running. If the app refuses the
  start, tell the owner and fall back to headless so the run does not stall.
- Desktop sessions have no `--max-budget-usd`, `--agents` or `--add-dir`. Before S1: make `gm-verifier` loadable
  (project agent file, not committed), use `effort: medium`, `use_worktree: false`, and have S1 report whether it can reach Rally.
- 2026-11-28: owner: S0 must not waste tokens. Sent to S0: drop must-have 3 checks and Rally R2-R4 (carry to ticket 11),
  keep commits, R1, one scoped verifier round, handoff. The S0 gate accepts these as dropped.
- 2026-11-28 11:48 UTC: owner: no verifier for S0. Do Rally checks (R1-R4), handoff, then S1. Chief stopped S0 during
  verifier round 1 and resumed it (N=2). S0 gate items 2 (verifier part), 3 and the verifier part of 4 are waived.
  5h 12%, weekly 83% at resume. S1 onwards still verified.
- 2026-11-28 11:58 UTC: S0 gate PASS (verifier items waived). Resume N=2 reported $0.97; N=1 cost not reported (stopped).
  5h 2 -> 16%, weekly 82 -> 83%. LIB main 9f803cb, gmr 96ebe07, LAB a6e6820 (branch learn; handoff says main), RALLY 21df7088c.
- S1: start_session refused ("this turn runs an automated event"). Fallback to headless, as told to the owner.
  S1 N=1: session 95ba92f3-5e40-42e0-98ca-83606a7598ac, cap $25, 5h before 16%, weekly 83%.
- 2026-11-28 ~13:57 UTC: owner: skip S1's separate verifier (S1 had already checked thoroughly), and from S2 split each
  step into a light builder session and a separate thorough verifier session. Chief stopped S1 N=1 just as its verifier
  started (cost not reported) and resumed it (N=2, cap $6, reported $0.63) to finish only.
- S1 gate PASS (verifier items waived): STATUS DONE, handoff headings complete, only Sonnet 5.5 (medium), LIB main
  fa90a7d (gmr 68c65b1, pushed), LAB d931e82, RALLY eafca6508, RALLY LIB 68c65b1, both Editors ready, map 07 done,
  owner's files unchanged (only the listed Lab prototype menu). No independent verification or code review of ticket 07.
  5h 16 -> 75%, weekly 83 -> 91%.
- Run docs for the split committed on library main: bbbf10e. gm-verifier.json removed.
- S2 launch: 5h 75% is over the 60% wait gate. Owner asked to move on now, so the chief does not wait.
- S2 builder started as a visible session: local_3cc090ca-9118-4aef-a6b1-64f085606d85 (Sonnet 5.5 medium, auto, no cost cap), 5h 75%, weekly 91%.
- 14:47 UTC: S2 builder gate PASS (BUILT, 206/206 reported, LIB main 0747e0b, gmr and Rally library 4f1cd03, Editors ready, owner files unchanged). Rally library has one uncommitted change, DebuggerOverlayPanel.asset, not in the build notes: for the verifier. 5h 75 -> 90%, weekly 91 -> 93%. Verifier NOT started: 5h over the 60% gate. Timer set for the 16:30 UTC reset.
