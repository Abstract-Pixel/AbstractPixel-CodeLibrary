# Nested session brief (mega run)

You do **one step** of [MEGA-TICKET.md](MEGA-TICKET.md). You do not need to read that file.
`CONTEXT` is `LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext`. Your working directory is the
Lab. Project Rally Shift is `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\ProjectRallyShift`.

## Never ask

Nobody can answer you. Never ask a question or end your turn waiting for an answer.
- Unwritten choice: take the option closest to DECISIONS.md, UI-STYLE.md and your spec; record it in the handoff.
- Permission denial: no unchanged retry (unless the denial text says to send it once more) and no workaround. Use an
  allowed way, or record it. If it blocks a must-have, report `BLOCKED`.
- No `AskUserQuestion`, computer-use, advisor tool, `budget start`, or "settle with the owner". Ignore `Budget:`
  lines in tickets. The chief meters you with a hard cost cap. If you stop at it, you will be resumed.

## Read

1. Your spec (ticket or S4 file), the previous handoff's **Next session** section first, then what the spec names.
2. `LIB/CLAUDE.md`, `LIB/CODING_STANDARDS.md`, `CONTEXT/PLAN-OVERVIEW.md`, `CONTEXT/DEFINITION-OF-DONE.md`.
3. When you touch Rally or run its checks: Rally's `CLAUDE.md` and [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md).

Where an older document disagrees with the spec's "Mega run" section or this brief, these win.
Open source files only as needed. Do not spawn explore or helper agents; the only subagent is the verifier.

## Work

1. **Preflight.** Both Editors answer `unity command editor_status`. `git status` and branch in all four repos match
   the previous handoff. If the previous step left something broken, fix it first and say so in your handoff.
2. **Progress file.** Create `<session folder>/PROGRESS.md`: one checkbox per must-have, plus Verify, Commits,
   Handoff. Tick and note the commit as you go. Update it before any long operation: it is what survives a stop.
3. **Build.** Tests first (see them fail). Look matches the approved stills and UI-STYLE.md. Reuse before writing.
4. **Check it yourself** the way a designer, developer or player would, in both Editors where the spec says. Run
   RALLY-DOGFOOD.md for your step.
5. **Commit** per task on `game-management-rework`, merge into `main` (the Lab runs `main`), and put Rally's library
   on the `game-management-rework` commit (RALLY-DOGFOOD.md, no push needed). Verify what you committed. Repeat
   this after each fix round.

## Editor rules

- One agent drives an Editor at a time. While your verifier runs, you do nothing in Unity: call it in the foreground
  and wait.
- Never `capture editor_window` on the Lab (it crashes Unity 7 alpha). No Windows or desktop screenshots. Use
  `CONTEXT/Sessions/Session5/capture-tools/` or `InternalEditorUtility.ReadScreenPixel` (wait 5 s after
  foregrounding), and read every still before using it as evidence.
- `recompile_status` can be stale: the DLL in `Library/ScriptAssemblies` must be newer than your source.
- Never close or kill an Editor. Answer every pop-up yourself (below).
- Long jobs (builds, scenario batches): start in the background and wait with foreground waits under 10 minutes each.
  **Never end your turn while a job is running**; your process exits with your turn.
- Temporary assets, probes and settings changes are removed or restored before verification.

### Pop-ups: nobody else will click them

A modal pop-up blocks every Unity command, and the owner is away. Answer it yourself; never wait or ask.

**Notice it.** Put a timeout on every `unity` command. On a timeout, a hang, no progress in a long job, or
`editor_status` showing `blocked_by_dialog`, look for a pop-up before anything else, in both Editors. Also look after
a recompile, a scene or asset change on disk, a branch switch, a build and a player launch.

**Handle it,** with the scripts in `../ProjectRallyShift/AgentScripts/Tools/`:
1. Read: `timeout 40 powershell -NoProfile -ExecutionPolicy Bypass -File ReadUnityDialog.ps1`. It and the accept
   script see **both** Editors, so confirm which project the pop-up belongs to first.
2. Press: `AcceptUnityDialog.ps1 -Buttons "<exact button name>"`. Always name the button (the default is `Reload`).
   If nothing changes, add `-RealClick`.
3. Not listed (common for the Lab's IMGUI dialogs): bring that Editor to the front (`focus_unity_project.ps1`,
   or `SetForegroundWindow`), which often clears it. If it stays, capture **only the pop-up's window**, read it, and
   click the chosen button by screen position. This is the one allowed Windows capture, never used as test evidence.
4. A native file or folder window: read it, then close it with `WM_CLOSE` unless your own step opened it on purpose.
5. Confirm the Editor answers again, then check that the interrupted command really finished. Do not assume it did.

**Which button**

| Pop-up | Press |
|---|---|
| Unsaved scene changes made only by your probe | `Don't Save` |
| Unsaved changes you did not make or cannot trace | `Cancel`, then find out whose they are. Never discard them |
| Your own intended scene edit is unsaved | `Save` |
| Scene or asset changed on disk by your commit, checkout or edit | `Reload`, only if the Editor holds no unsaved work |
| Build "Opening file failed" | `Try Again` after closing the running player; if it repeats, `Cancel` |
| API Updater, or any offer to rewrite source or upgrade a project, package or Unity version | Decline |
| Compile errors on opening ("Safe Mode") | The choice that keeps the Editor usable to fix the errors; then fix them |
| Windows Firewall prompt (Unity or the dev player) | `Cancel`. Never allow access; local control works without it |
| Sign-in, licence, terms to accept, purchase, or a Windows administrator prompt | Do not accept or sign in. Report `BLOCKED` |
| Anything else | Read it all; press the choice that changes least (`Cancel`, `Close`, `No`) |

Never press a button that deletes or discards the owner's work, accepts terms, upgrades, or changes a security or
system setting. Log each pop-up in PROGRESS.md (project, text, button, result) and repeat them in the handoff under
"Traps found". If the same one returns, fix its cause (for example, save your scene edit at once).

## Verify

Start the verifier with `Agent`, `subagent_type: "gm-verifier"`. **Pass no `model` and no `effort`** (its definition
sets Sonnet High). Never use another agent type to verify. Send only this packet:

```text
Step: <S#>. Round: <n>. Spec: <path>. Session folder: <path>.
Diff: LIB <base>..<head> ; RALLY <base>..<head or "working tree: paths">.
Verify commit: <library sha>. Rally library is on: <sha>.
Changed on purpose: <repo files that may differ from the start state>.
Evidence I have: <stills, test counts, logs: paths>.
Skip (already passed, untouched since): <check ids, or none>.
Re-check (round 2+): <finding ids and the areas my fixes touched>.
```

Do not tell it what to conclude, and do not argue a finding away. For each finding: reproduce, add a failing test
when it is logic, fix, commit. Then start a **new** verifier with the next round's packet. You may dispute a finding
only with evidence, in the next packet; the new verifier decides.

Repeat until `VERDICT: PASS`. Any code change after a PASS needs another round. If the same finding survives three
rounds, change your approach and write why in PROGRESS.md.

## Finish

Only after PASS:
1. Remove the verified UI's prototype menu entry and window class (DEFINITION-OF-DONE, last section; keep USS/UXML).
   This Lab-only prototype change needs no new verifier round: recompile the Lab clean and say what you removed.
   Leave it uncommitted in the Lab.
2. Push the library branches (tests green), confirm Rally's library is still on the verified
   `game-management-rework` commit and **leave it there**, and commit the Lab and Rally submodule pointers only.
   If a push is denied, record it and continue.
3. Mark the ticket `done` in `Wayfinder/map.md`, add one line to "Decisions so far", add `## From ticket NN` notes to
   later tickets.
4. Write the handoff, commit docs on `main` by path. `Sessions/mega-07-09/LEDGER.md` is the chief's: leave it alone.
   S4 has no ticket: it skips steps 1 and 3.

## Handoff: `<session folder>/HANDOFF.md`

Every heading is required. Write "none" rather than leaving one out. Keep it under 120 lines.

```markdown
# <step> handoff
## Result
## Verification      (rounds, final VERDICT, verified commit, VERIFY file)
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

```text
STATUS: DONE | BLOCKED
STEP: <S#>
HANDOFF: <path>
VERIFY: <rounds> rounds, last = PASS | FAIL (<path>)
COMMITS: LIB gmr <sha>, main <sha> ; LAB <sha> ; RALLY <sha> ; pushed: yes | no (<why>)
RALLY LIBRARY ON: <sha>
BLOCKER FOR NEXT STEP: none | <one line>
DECISIONS: <one line each, or none>
```

`DONE` means: every must-have met, verifier PASS on the final code, handoff written, repos in the state it
describes. Anything less is `BLOCKED` with the reason.
