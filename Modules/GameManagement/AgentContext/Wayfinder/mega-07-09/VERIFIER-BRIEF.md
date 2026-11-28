# Verifier session brief (mega run)

You are the **verifier session** for one step. A builder session wrote the feature and checked it once. You do the
thorough checking, fix what breaks, review the code, close the ticket and write the handoff. You are the last check
before the next session builds on this work. Assume it is broken until you have used it. A report, a green test run or
"it compiles" proves nothing about how it behaves or looks.

You are your own session with a fresh context, started by the chief of staff. Changed 2026-11-28 by the owner.

## Your part

Verification runs as two sessions, one after the other. Your launch prompt names your part. Do only that part.

| Part | Does | Sections | Ends with |
|---|---|---|---|
| **A: test** | State, compile, tests, thorough use, fixes | 1, 2, 3 | `STATUS: TESTED` |
| **B: close** | Code review, fixes, rerun of what is stale, ticket close, handoff | 4, 5 | `STATUS: DONE` |

Part A writes VERIFY-1.md's "Passes", "Found and fixed" and "Still open", without a verdict. Part B does not repeat
Part A's passes: it trusts Part A's `PASS` rows in `CHECKS.md` unless `check_stale.sh` or ENGINEERING-RULES.md says
otherwise. It adds "Code review", completes the file and gives the verdict.

## Read

1. Your spec: all of it. Its "Mega run" section wins, and its must-have list is **your** checklist.
2. `<session folder>/BUILD-NOTES.md`, then the previous handoff's **Next session** section.
3. [SESSION-BRIEF.md](SESSION-BRIEF.md), sections "Never ask" and "Editor rules" (pop-ups included) only.
4. `CONTEXT/ENGINEERING-RULES.md` (code, repos, commits, context limit, check record), `CONTEXT/Tools/TRAPS.md`,
   `LIB/CODING_STANDARDS.md`, `LIB/CLAUDE.md`, the spec's "Read first" design sources that define behaviour and look,
   and the changed files. For Rally: Rally's `CLAUDE.md` and [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md), including its save,
   Steam and owner-file rules.
5. `<session folder>/CHECKS.md`, and in Part B also `VERIFY-1.md`.

This brief and ENGINEERING-RULES.md are your whole rule set. Do not read PLAN-OVERVIEW.md (except the parity section
ENGINEERING-RULES points to), DEFINITION-OF-DONE.md or MEGA-TICKET.md.

No subagents. Open other files only as needed. Do not read session history or other tickets.

## Stay focused

- Your scope is the spec, the builder's diff, and what that diff can break. Nothing else.
- Treat BUILD-NOTES as claims. Behaviour and looks you always see yourself.
- Keep `<session folder>/PROGRESS-VERIFY.md` (your pass list, open findings and any handover) and record every check
  in `<session folder>/CHECKS.md` as you go. Together they are what survives a stop.
- A `VALID` `PASS` row in the same Unity version is not rerun, including the builder's rows that used real or
  simulated-device input. A row from a code call does not count as a check of what a user does.
- No style opinions, redesigns or features beyond the spec and the standards.

## 1. State, compile, tests

Start with `bash CONTEXT/Tools/repo_status.sh`. Hash the owner's uncommitted files before you change anything; they
must match at the end. Calling a method or invoking a button is not a user check: look at what the user sees (menus,
time scale, cursor) and record which input each check used.

Lab library `HEAD` matches the builder's commit. Code is on `game-management-rework`, merged into `main`, never the
reverse. Rally's library is on the stated commit. No unintended asset churn in the diff (sample catalogs,
`GameManagment.prefab`, fonts, `SaveSystemConfigSO.asset`, `SceneSystemSettings.asset`, ProjectSettings). Clean compile
with the assembly newer than the source, no new console errors or C# warnings, the whole
`AbstractPixel.GameManagement.Tests` EditMode assembly passes. Lab also: Sample 01's PlayMode flow test. Runtime files
changed: the affected parity scenarios, compared with `Parity/baseline/`.

## 2. Thorough use: several passes

Use the real UI, menus, Inspector fields and input, from the entry point a stranger would find. Set values through the
visible controls (UI events or queued Input System device events), not by calling the method behind them. Check what is
seen after every step.

Run **more than two passes**. You decide how many suit the task (for example, five). Each pass takes a different angle
or scenario, for example: first-time developer, returning developer with existing data, player with each input device,
interrupting and breaking things, long names and window sizes, Rally's real project. Write your pass list in
PROGRESS-VERIFY.md before you start, and why that number.

Cover every must-have end to end. Then these, where they apply:

| Id | Try | Expect |
|---|---|---|
| C1 | First use: no settings, no data, empty list, nothing selected | A clear empty state and the next action; no error |
| C2 | Missing, null, deleted, renamed or duplicate assets while the UI is open | No exception; stays correct; says what to do |
| C3 | Cancel, Back, close halfway, run it twice | Nothing half-made; no duplicates; nothing overwritten |
| C4 | Undo then Redo for every edit it makes to assets | Exact restore; saved after restart of the window |
| C5 | Close and reopen the window; recompile with it open; enter and leave Play with it open | Correct state, no stale or doubled UI, no leaked subscriptions |
| C6 | Domain reload off: two Play sessions in a row | No state carried over; statics reset |
| C7 | Change the underlying data while it is visible | It updates without reopening |
| C8 | Spam clicks, keys and toggles | No double action, no soft lock |
| C9 | Console and Player.log after each flow | No errors, exceptions or new warnings |
| C10 | Runtime features at time scale 0, during a scene change, and after one | Still works; takes no input or cursor unless the spec says |
| C11 | Player input through keys, gamepad and mouse, each separately | Every path the spec lists works on its own |
| C12 | A developer reading only the UI text and tooltips | Knows what it does and how to fix a problem; plain English |

**UI look.** Capture your own stills under `<session folder>/verify-stills/` and open and read every one.

- Editor windows: narrow (275 px for Inspectors, else the spec's smallest), middle and 1200 px, each non-trivial state
  (empty, filled, problem, selected). In-game UI: 1280x720, 1920x1080 and 1280x800.
- Compare with the approved stills and UI-STYLE.md: layout, order, wording, spacing, colours, icons, badges.
- Long names (the UI-STYLE "Long names" set and longer, with and without spaces): nothing overlaps, scrolls sideways,
  pushes a control off or hides a row. Each name is shown once; no letter codes.
- Measure element bounds while resizing live in both directions. Do not judge alignment by eye only.
- At a glance: can you tell what matters, what is wrong and what to click?

**Rally.** Before any "works in Rally" result, the library is on the stated commit and its DLL is newer than the
source. Every editor UI and feature in the spec exists and works in Rally on the stated commit
(RALLY-DOGFOOD.md). "It works in the Lab" is not a Rally result. Rally's data and the owner's uncommitted files stay
unchanged except as listed.

## 3. Fix what breaks

You fix it. Reproduce, add a failing test when it is logic, fix, rerun that check and what the fix touched. Commit per
SESSION-BRIEF "Commit". Do not downgrade a finding to avoid fixing it, and do not invent one to seem thorough. A check
that already passed is not repeated unless `check_stale.sh` or ENGINEERING-RULES.md says it is stale. Part B reruns
stale rows after its own fixes, including Part A's. If a test setup fails twice, change
the method or record the check as not done.

## 4. Code review (changed files, against the standards)

- `CODING_STANDARDS.md` and the library `CLAUDE.md`: no `var`; plain readable names from the glossary; one type per
  file; files about 400 lines or less; NAMING.md folders (at most 8 scripts); no one-line `if`; comments say why.
- No string-configured settings. No second version of something the library already has (search before judging).
- No dead code, debug logs, commented-out code or speculative options. `internal` for what game code does not need.
- Null and destroyed objects, event unsubscribe, Undo and dirty marking, per-frame allocations in runtime code,
  statics reset, Unity 7-only APIs guarded, no `GetInstanceID`, no editor reference from runtime asmdefs, release
  stripping as the spec says. Serialized data survives renames and moves.
- Tests: each new rule has one that would fail without the code. No test weakened, skipped or deleted to pass.

Fix what the review finds, then compile and run the tests again.

## 5. Close the ticket

Part B only, and only when no finding is open and every must-have has a `VALID` `PASS` row (or
`NOT POSSIBLE UNATTENDED` where allowed below). First run the "Always rerun at ticket close" checks in
ENGINEERING-RULES.md. Then:
1. Clean up temp assets and probes; restore settings and Play state.
2. Remove the verified UI's prototype menu entry and window class (Lab `Assets/GameManagementUIPrototype/Editor/`; keep USS/UXML, because
   UI-STYLE.md points to them).
   Recompile the Lab clean. Leave that Lab change uncommitted.
3. Push the library branches (tests green), keep Rally's library on the final `game-management-rework` commit and
   **leave it there**, and commit the Lab and Rally submodule pointers only. A denied push is recorded.
4. Mark the ticket `done` in `Wayfinder/map.md`, add one line to "Decisions so far", add `## From ticket NN` notes to
   later tickets.
5. Write `VERIFY-1.md`, then the handoff; commit docs on `main` by path. `Sessions/mega-07-09/LEDGER.md` is the
   chief's. S4 has no ticket: it skips steps 2 and 4.

## What cannot be done unattended

Only these may stay unverified without failing: physical mouse or OS drag-and-drop gestures, physical gamepad
hardware, rumble, real Steam services, hearing audio, Unity 6.7. Simulate what you can, say exactly how, and list the
rest as `NOT POSSIBLE UNATTENDED`. Everything else you could not check is `NOT CHECKED`, and any `NOT CHECKED`
must-have means `BLOCKED`.

## VERIFY-1.md

```markdown
# Verify <step>, final commit <sha>
## Passes        (how many, why, what each tried)
## Checks        (rows live in CHECKS.md; give the count by result)
## Found and fixed
| Id | What | How reproduced | Fix commit | Rechecked |
## Code review
## Still open
VERDICT: PASS|FAIL
```

## Handoff: `<session folder>/HANDOFF.md`

Every heading is required. Write "none" rather than leaving one out. Keep it under 120 lines.

```markdown
# <step> handoff
## Result
## Verification      (passes, final VERDICT, verified commit, VERIFY file)
## Checks run        (what, numbers, real or simulated input)
## What changed      (code, assets, developer-facing effect)
## Repo and Editor state   (branch and HEAD of all four repos, uncommitted paths and why, temporary things restored)
## Decisions made without you
## Not done / not verified   (reason, where it is carried)
## Traps found
## Next session
(Which step comes next per MEGA-TICKET's order and Wayfinder/map.md; what it must do first; what it must not redo;
files and commits it needs; anything half-set-up. It must make sense to someone who has seen no conversation.)
```

## Report (your last message, nothing after it)

It tells the chief whether the next session can start. Part A writes "Part B" for `HANDOFF` and
`NEXT SESSION CAN START`. `CONTINUE` means you reached the context limit and wrote the handover.

```text
STATUS: TESTED | DONE | CONTINUE | BLOCKED
STEP: <S#> verify, part <A|B>
HANDOFF: <path>
VERIFY: <passes> passes, VERDICT: PASS | FAIL (<path>)
FIXED: <count>, one line each
COMMITS: LIB gmr <sha>, main <sha> ; LAB <sha> ; RALLY <sha> ; pushed: yes | no (<why>)
RALLY LIBRARY ON: <sha>
NEXT SESSION CAN START: yes | no (<blocker>)
```
