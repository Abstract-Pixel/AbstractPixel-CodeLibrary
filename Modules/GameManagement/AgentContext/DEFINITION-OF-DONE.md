# Definition of done

What "finished" means for a task and for a session on the game management map. Every ticket points here. If a
ticket says more for a task, follow both.

This file is a **guideline for every session**, not a per-session checklist. Each ticket in `Wayfinder/tickets/` has
its own **"Done and verified"** section that says what finished and verified mean for that ticket (end state and the
exact checks). A session is finished only when this guideline and its ticket's section both hold.

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
5. **It works the way a developer and a player would use it** (owner, 2026-11-26). Tests that call methods or invoke
   buttons are not enough: they missed that Esc never paused Sample 01 and that Settings left the Pause menu showing.
   - Enter Play mode in the real scene with the project's real settings and preferences, and drive it with real input:
     the keys and gamepad buttons a player presses (`InputSystem.QueueStateEvent` on a test device in PlayMode tests),
     and the Inspector fields, menus and preferences a developer sets.
   - Check what the player sees after every step (menus shown, time scale, cursor), not only the machine's state.
   - A sample's flow test drives each feature through every input it offers (key, gamepad button, UI button).
   - The handoff says which checks used real input and which did not.
6. **Nothing of the owner's was disturbed.** `git status` and the hashes of their uncommitted files match before and after.
7. **One type per script.** New or changed code keeps each class, struct, enum or interface in its own file, in
   folders by part (at most one extra small type when it belongs to the main one). No script holds a dump of several
   types, as the old `Runtime/Machine/Effects.cs` did.
8. **Names and folders pass [NAMING.md](NAMING.md).** New or renamed types pass the read-alone test; no folder holds
   more than 8 scripts.
9. **No strings configure anything.** No new field, setting or setup API takes a string ID, name or key
   (DECISIONS.md, 2026-11-26); references, typed values or enums instead.
10. **Choices are written down.** Any decision not already in DECISIONS.md or DESIGN.md goes in the handoff's
   "Decisions made without you" table.

Checked by running the commands, not by reading the code. "Should work" is not finished.

## A session is finished when

1. **Every task is finished** in the sense above, or listed in the handoff's "Not done" table with the reason and
   whether it is really needed.
2. **Final checks pass on the end state:** full test assembly, clean compile, parity (when old runtime files changed),
   owner's files untouched, and the full tests in the Unity 7 Lab with its library submodule moved to this session's
   commit (PLAN-OVERVIEW.md, "Both Editors").
3. **The handoff exists:** `Sessions/<ticket id>/HANDOFF.md` with result, checks run (with numbers), code changes,
   effect on developers, decisions made without the user, not done, and budget.
4. **The map is up to date:** the ticket is marked `done` in `Wayfinder/map.md` with one line in "Decisions so far".
   Anything new goes in as a ticket or as fog. No prompts for other sessions: the map's tickets are the prompts.
   Anything a later ticket needs is added to that ticket's file under `## From ticket NN` (PLAN-OVERVIEW, "Carry forward").
5. **The budget is closed:** `budget end` called, and the handoff says what was allotted, what was used and whether a
   warning fired.
6. **Committed.** Work is committed per task; the handoff lists the commits. Pushing follows ticket 04's decision.

A session that ran out of budget is finished only once points 3 to 5 are done. Save budget for them: they are the
wrap reserve.
