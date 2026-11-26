# Definition of done

What "finished" means for a task and for a session in the gm-commercial plan. Every session prompt points here. If a
session's OVERVIEW says more for a task, follow both.

## A task is finished when

1. **Its must-have line is met in full.** Nice-to-haves are optional. A must-have cut for budget makes the task
   *not finished*: say so in the handoff's "Not done" table, with the reason.
2. **Tests first, and they pass.** Every rule the task adds has an EditMode test in `Tests/`, and the module's whole
   test assembly passes (`unity command run_tests --mode editor --filter AbstractPixel.GameManagement.Tests --filter_type assembly`).
3. **It compiles clean.** `bash AgentScripts/Tools/editor_cycle.sh noplay` reports no errors, and the console shows no
   new errors or C# warnings.
4. **Rally still behaves the same,** unless the task is meant to change behaviour. If the task touched an old runtime
   file, the parity scenarios it could affect were run and compared with `Parity/baseline/` (see `Parity/README.md`).
   Differences are either frame-rate noise or listed as intended.
5. **Nothing of the owner's was disturbed.** `git status` and the hashes of their uncommitted files match before and after.
6. **Choices are written down.** Any decision not already in DECISIONS.md or DESIGN.md goes in the handoff's
   "Decisions made without you" table.

Checked by running the commands, not by reading the code. "Should work" is not finished.

## A session is finished when

1. **Every task is finished** in the sense above, or listed in the handoff's "Not done" table with the reason and
   whether it is really needed.
2. **Final checks pass on the end state:** full test assembly, clean compile, parity (when old runtime files changed),
   owner's files untouched.
3. **The handoff exists:** `Sessions/SessionN/HANDOFF.html` with result, checks run (with numbers), code changes,
   effect on developers, decisions made without the user, not done, and budget.
4. **The next session is ready:** `Wayfinder/map.md` updated, `Sessions/SessionN+1/OVERVIEW.md` and `PROMPT.md`
   written, and the prompt links to this file.
5. **The budget is closed:** `budget end` called, and the handoff says what was allotted, what was used and whether a
   warning fired.
6. **Commits only if allowed.** Commit only when the user's launch message allows it. Otherwise the handoff says
   nothing is committed.

A session that ran out of budget is finished only once points 3 to 5 are done. Save budget for them: they are the
wrap reserve.
