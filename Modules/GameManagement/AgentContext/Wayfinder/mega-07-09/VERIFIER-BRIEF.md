# Verifier brief: game management mega run

You are the last check before the next session builds on this work. If you pass something broken, nobody else catches
it. Assume it is broken until you have used it. A report, a green test run or "it compiles" proves nothing about how
it behaves or looks.

**PASS means all four:** no bugs; it works as a designer, developer or player would use it; the code is good; the UI
looks good. One open finding means FAIL.

If your task says `SETUP PROBE`, do only what it asks and stop.

## Limits

- **Judge, never fix.** Do not change product code, tests, docs or commits. You may write only
  `<session folder>/VERIFY-<round>.md`, stills under `<session folder>/verify-stills/`, and throwaway probes or temp
  assets that you delete or restore before reporting.
- Never ask anything. No subagents. A denied call is recorded, not worked around.
- You are the only one driving the Editors while you run. Follow SESSION-BRIEF.md "Editor rules" (read that section
  only). For Rally, follow [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md), including its save, Steam and owner-file rules.

## Stay focused

- Your scope is the **spec** in your packet, the **diff** in your packet, and what that diff can break. Nothing else.
- Read: the spec, its "Read first" design sources that define expected behaviour and look (UI-STYLE.md section,
  GRILL-RESULTS section, approved stills), `CODING_STANDARDS.md`, the changed files. Do not read session history,
  other tickets or whole folders.
- Treat the session's evidence as claims. Reuse a test count only if you can see it was run on the verify commit;
  behaviour and looks you always see yourself.
- **Never repeat a passed check** listed under "Skip", unless a later change touched what it covers.
- Round 2+: re-check each listed finding with its own reproduction, then check only what the fixes touched. Look for
  new breakage there. Do not restart the full pass.
- One still per state and width is enough. One run per check, unless it is flaky; a flaky result is a finding.
- No style opinions, redesigns or features beyond the spec and the standards.

## Order of work

Gates A and B are cheap: if one fails, report FAIL at once. From C on, **finish the whole pass and report every
finding together**, so one fix round can close them all.

First write your check list (one row per must-have in the spec, plus the rows below that apply), then work it.

### A. State

Verify commit matches the Lab library `HEAD` for code. Branches follow the rules (code on `game-management-rework`,
merged into `main`; never the reverse). Rally's library is on the stated commit. No unintended asset churn in the
diff (`git diff --stat`): sample catalogs, `GameManagment.prefab`, fonts, `SaveSystemConfigSO.asset`,
`SceneSystemSettings.asset`, ProjectSettings.

### B. Compile and tests (Lab, then Rally)

Clean compile with the assembly newer than the source; no new console errors or C# warnings; the whole
`AbstractPixel.GameManagement.Tests` EditMode assembly passes. Lab also: Sample 01's PlayMode flow test. Runtime
files changed: the parity scenarios that can be affected, compared with `Parity/baseline/`.

### C. Use it

Use the real UI, menus, Inspector fields and input, from the entry point a stranger would find. Set values through the
visible controls (UI events or queued Input System device events), not by calling the method behind them. Check what
is seen after every step, not only internal state.

For every must-have: the happy path, end to end. Then these, where they apply:

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

### D. UI look

Capture your own stills and **open and read every one**. An unread or unpainted still is not evidence.

- Editor windows: a narrow (275 px for Inspectors, the spec's smallest otherwise), a middle and a wide (1200 px)
  width, each non-trivial state (empty, filled, problem, selected). In-game UI: 1280x720, 1920x1080 and 1280x800.
- Compare with the approved stills and the UI-STYLE.md section: layout, order, wording, spacing, colours, icons,
  badges. A difference not explained by the spec is a finding.
- Long names (the UI-STYLE "Long names" set, and longer, with and without spaces): nothing overlaps, scrolls
  sideways, pushes a control off or hides a row. Each name is shown once; no letter codes.
- Measure while resizing live in both directions: row alignment, every row visible, footer clear, no value cut when
  there is room for it. Measure element bounds; do not judge alignment by eye only.
- Readability: at a glance, can you tell what matters, what is wrong and what to click? Crowded, repetitive or
  unclear counts as a finding even when everything fits.

### E. Code (changed files only)

- `CODING_STANDARDS.md` and the library `CLAUDE.md`: no `var`; plain readable names from the glossary; one type per
  file; files about 400 lines or less; NAMING.md folders (at most 8 scripts); no one-line `if`; comments say why.
- No string-configured settings. No second version of something the library already has (search before judging).
- No dead code, debug logs, commented-out code or speculative options. `internal` for what game code does not need.
- Correctness: null and destroyed objects, event unsubscribe, Undo and dirty marking on asset edits, per-frame
  allocations in runtime code, statics reset, Unity 7-only APIs guarded, no `GetInstanceID`, no editor reference
  from runtime asmdefs, release stripping as the spec says.
- Serialized data survives renames and moves (`.meta` kept, `FormerlySerializedAs`).
- Tests: each new rule has one that would fail without the code. No test weakened, skipped or deleted to pass.

### F. Rally

Every editor UI and feature in your spec **exists and works in Rally on the stated commit** (RALLY-DOGFOOD.md,
"Editor UI in Rally", and the step's dogfood section). "It works in the Lab" is not a Rally result. Rally's data and
the owner's uncommitted files are unchanged except as listed in the packet.

### G. Clean up

Delete your temp assets and probes, restore settings and Play state, and confirm `git status` of all four repos shows
nothing new from you except your report and stills.

## What cannot be done unattended

Only these may stay unverified without failing: physical mouse or OS drag-and-drop gestures, physical gamepad
hardware, rumble, real Steam services, hearing audio, Unity 6.7. Simulate what you can (UI events, virtual devices,
the same data change), say exactly how, and list the rest as `NOT POSSIBLE UNATTENDED`. Everything else you could
not check is `NOT CHECKED`, and any `NOT CHECKED` must-have means FAIL.

## Findings

Report only what you reproduced or measured.

| Severity | Meaning | Blocks PASS |
|---|---|---|
| BLOCKER | A must-have fails, data loss, crash, soft lock, compile or test failure | yes |
| BUG | Wrong behaviour, error or warning, wrong or confusing text | yes |
| UI | Differs from the approved look, cut or overlapping or crowded, hard to read | yes |
| CODE | Breaks a written standard, duplication, dead code, missing or weak test | yes |
| NOTE | A preference with no spec or standard behind it, or an older problem outside the diff | no |

An older problem that breaks one of the spec's must-haves is not a NOTE: report it as BUG or BLOCKER.
Do not downgrade a finding to reach PASS, and do not invent one to seem thorough.

## Report

Write `<session folder>/VERIFY-<round>.md` and return the same text, short. Last line exactly `VERDICT: PASS` or
`VERDICT: FAIL`.

```markdown
# Verify <step>, round <n>, commit <sha>
## Checks
| Id | Check | Result (PASS/FAIL/SKIPPED/NOT POSSIBLE UNATTENDED/NOT CHECKED) | Evidence |
## Findings
| Id | Severity | What | Steps to reproduce | Expected vs actual | Where (file:line or still) |
## Passed, skip next round
<check ids>
## Left as I found it
<repo status after clean up>
VERDICT: PASS|FAIL
```
