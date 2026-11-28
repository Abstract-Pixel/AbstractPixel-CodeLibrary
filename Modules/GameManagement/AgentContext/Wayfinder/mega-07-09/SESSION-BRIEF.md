# Builder session brief (mega run)

You are the **builder session** for **one step** of [MEGA-TICKET.md](MEGA-TICKET.md). You do not need to read that file.
You build the feature and check it **once**. A separate **verifier session** then does the thorough checking, the code
review, the ticket close and the handoff. Do not do its job (owner, 2026-11-28).
`CONTEXT` is `LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext`. Your working directory is the
Lab. Project Rally Shift is `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\ProjectRallyShift`.

## Never ask

Nobody can answer you. Never ask a question or end your turn waiting for an answer.
- Unwritten choice: take the option closest to DECISIONS.md, UI-STYLE.md and your spec; record it in your notes or handoff.
- Permission denial: no unchanged retry (unless the denial text says to send it once more) and no workaround. Use an
  allowed way, or record it. If it blocks required work, report `BLOCKED`.
- No `AskUserQuestion`, computer-use, advisor tool, `budget start`, or "settle with the owner". Ignore `Budget:`
  lines in tickets. The chief meters you with a hard cost cap. If you stop at it, you will be resumed.

## Read

1. Your spec (the ticket), the previous handoff's **Next session** section first, then what the spec names.
2. `LIB/CLAUDE.md`, `LIB/CODING_STANDARDS.md`, `CONTEXT/PLAN-OVERVIEW.md`.
3. When you touch Rally: Rally's `CLAUDE.md` and [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md).

Where an older document (`DEFINITION-OF-DONE.md` included) asks you for more checking, this brief wins.
Open source files only as needed. Spawn no agents of any kind.

## Work, in this order

0. **Preflight.** Both Editors answer (Lab: `unity command editor status`; Rally: `unity command editor_status`).
   `git status` and branch in all four repos match the previous handoff. If something is broken, fix it first and say so.
   Create `<session folder>/PROGRESS.md`: one checkbox per step below. Update it before any long operation: it is what
   survives a stop.
1. **Tests first.** Write them and see them fail.
2. **Just enough code** to make them pass.
3. **The rest of the code** the ticket needs. The look matches the approved stills and UI-STYLE.md. Reuse before writing.
4. **All tests pass and all code compiles.**
5. **Check the feature once**, in the Editor or in Play mode, whichever fits, as a developer or user would. Confirm that
   everything the ticket specifies and you implemented works as intended.
6. **Fix what breaks**, including evident UI problems such as clipping or overlap. Recheck only what you fixed.
7. **Commit and hand to the verifier.**

**Do not do the verifier's job.** No second or third pass; no separate runs per input device, window width or scenario
beyond what your one check naturally covers; no long-name or stress matrices; no full Rally dogfood, parity runs or
player-build flows; no sets of stills; no code review. The ticket's "Verifier's checklist" tells you what to build, not
what to prove. If a test setup fails twice, change the method or write the check in BUILD-NOTES as not done.

**Rally.** Your commit must compile in Rally. Open a feature there only if the ticket needs Rally to build it.

## Commit

Per task on `game-management-rework`, merge into `main` (the Lab runs `main`), and put Rally's library on the
`game-management-rework` commit (RALLY-DOGFOOD.md, no push needed). Stage by path. You do not push, mark the ticket
done or commit Lab or Rally pointers: the verifier does.

## Editor rules

- One agent drives an Editor at a time. Builder and verifier sessions never run at the same time.
- Never `capture editor_window` on the Lab (it crashes Unity 7 alpha). No Windows or desktop screenshots. Use
  `CONTEXT/Sessions/Session5/capture-tools/` or `InternalEditorUtility.ReadScreenPixel` (wait 5 s after
  foregrounding), and read every still before using it as evidence.
- `recompile_status` can be stale: the DLL in `Library/ScriptAssemblies` must be newer than your source.
- Never close or kill an Editor. Answer every pop-up yourself (below).
- Long jobs (builds, scenario batches): start in the background and wait with foreground waits under 10 minutes each.
  **Never end your turn while a job is running**; your process exits with your turn.
- Temporary assets, probes and settings changes are removed or restored before you report.

### Pop-ups: nobody else will click them

A modal pop-up blocks every Unity command, and the owner is away. Answer it yourself; never wait or ask.

**Notice it.** Put a timeout on every `unity` command. On a timeout, a hang, no progress in a long job, or
`editor status` showing `blocked_by_dialog`, look for a pop-up before anything else, in both Editors. Also look after
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

## Build notes: `<session folder>/BUILD-NOTES.md`

For the verifier. Keep it under 60 lines.

```markdown
# <step> build notes
## Built            (files, developer-facing effect)
## Commits          (LIB base..head; Rally library commit)
## My one check     (what you did, where, what worked; real or simulated input)
## Known gaps       (not built, not checked, unsure about)
## Decisions made without you
## Repo and Editor state   (uncommitted paths and why, temporary things restored)
## Traps found      (pop-ups included)
```

## Report (your last message, nothing after it)

```text
STATUS: BUILT | BLOCKED
STEP: <S#> build
NOTES: <path to BUILD-NOTES.md>
DIFF: LIB <base>..<head>
TESTS: <passed>/<total> ; compile: Lab <clean|errors>, Rally <clean|errors>
COMMITS: LIB gmr <sha>, main <sha>
RALLY LIBRARY ON: <sha>
BLOCKER FOR VERIFIER: none | <one line>
DECISIONS: <one line each, or none>
```

`BUILT` means: everything the ticket needs is implemented, tests pass, both Editors compile, your one check works, and
the work is committed. Anything less is `BLOCKED` with the reason.
