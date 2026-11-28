# Mega ticket: onboarding, debugger overlay and debugger window builds (autonomous)

Type: orchestration. Covers map tickets 07, 08 and 09, after closing 06 (07 is blocked by it).
Written 2026-11-28. **This file is for the chief of staff only.** Nested sessions read
[SESSION-BRIEF.md](SESSION-BRIEF.md); verifiers read [VERIFIER-BRIEF.md](VERIFIER-BRIEF.md).

## Goal

One run, from start to a finished Game State Debugger window build, with every UI working in Project Rally Shift.
Each session ends verified and handed off, so the next one has no cleanup, no blocker and nothing to fix.

## Rule 1: never ask the owner for anything

From the launch message to the final report, no agent asks the owner a question, asks for approval, or waits for
the owner.

- No `AskUserQuestion`. No `ask` or buttons in `wrap_up`. No session offers or proposal cards.
- No tool that opens an approval card or dialog: no computer-use, no `request_directory`, no permission-mode,
  model or effort changes, no connector or plugin installs.
- No `budget start` in any session, and no prompt containing `N% budget`, `budget of N%` or `plan: <id>, step N`
  (see "Budgets": the budget mod would open a blocking dialog). Ignore the `Budget:` lines in tickets.
- A choice that is not written down: take the option closest to DECISIONS.md, UI-STYLE.md and the ticket, record it
  under "Decisions made without you", carry on.
- A tool call denied by permissions or auto mode: do not retry it unchanged and do not work around it. (If the denial
  text itself says to send the same call once more, do that once.) Use an allowed way to the same result, or record
  it and apply "Stop conditions".
- "Settle with the owner" or "show the owner" in an older ticket or handoff is replaced by that ticket's
  "Mega run" section. Stills for the owner go in the session folder and are listed in the final report.

## Roles

| Role | How it runs | Model | Effort |
|---|---|---|---|
| Chief of staff | The desktop Code session the owner starts in the Lab | Sonnet | Medium |
| Nested session (one per step) | A headless `claude -p` process the chief starts | Sonnet | Medium |
| Verifier | Subagent `gm-verifier`, started by a nested session | Sonnet | High |

- **Chief:** launches sessions, reads their reports, checks the gate, passes messages, keeps the ledger. It never
  writes code, reads source files, drives Unity (beyond `unity command editor_status`) or verifies features itself.
  One exception: if an Editor is blocked by a pop-up while no session is running (preflight, gate, between launches),
  the chief clears it by [SESSION-BRIEF.md](SESSION-BRIEF.md), "Pop-ups". While a session runs, its pop-ups are its own.
- **Nested session:** does one step, gets it verified, writes the handoff, reports.
- **Verifier:** judges only. It never fixes product code.
- **Communication:** chief to session = the launch or resume prompt. Session to chief = its final STATUS block.
  Session to session = handoffs only. Sessions never run in parallel (one agent drives the Editors).

**Why headless processes and not desktop "new session" cards:** the app's `start_session` refuses starts on turns
begun by another session's message and holds notes after many automated turns, so a long chain can stall. Do not use
it, and do not use the `Agent` tool or workflows for nested sessions. If the mechanism below fails, apply
"Stop conditions"; do not change the design.

## Paths

| Name | Path |
|---|---|
| LAB | `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\Abstract_Pixel_Lab` (Unity 7000.0.0a16) |
| LIB | `LAB\LocalPackages\AbstractPixelLibrary` (library repo, Lab clone; all code is written here) |
| CONTEXT | `LIB\Modules\GameManagement\AgentContext` |
| RUN | `CONTEXT\Wayfinder\mega-07-09` (these files) |
| LOGS | `LAB\Logs\mega-07-09` (prompts, result JSON; never committed) |
| LEDGER | `CONTEXT\Sessions\mega-07-09\LEDGER.md` |
| RALLY | `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\ProjectRallyShift` (Unity 6000.3.15f1) |
| RALLY LIB | `RALLY\LocalPackages\AbstractPixelLibrary` (git submodule; receives commits only) |
| Transcripts | `C:\Users\avig7\.claude\projects\C--Users-avig7-Desktop-AllUnity-ActiveProjects-Abstract-Pixel-Lab` (read only, for the effort audit) |

Allowed outside LAB and RALLY: the session scratchpad, reading `~/.claude/skills` and the transcripts folder, and the
save-folder backup in [RALLY-DOGFOOD.md](RALLY-DOGFOOD.md).

## What the owner does before the run (the only human steps)

1. Check the plan limits. The run needs several 5-hour windows. On 2026-11-28 the weekly limit was 80% used
   (resets 2026-12-02 15:59 UTC) and extra usage was off. A weekly limit stops the run.
2. Windows: sleep, lock and screen-off set to Never for the run. Editor stills need a visible, unlocked screen.
3. Open both Unity Editors (LAB and RALLY), with no unsaved scene work. Close Steam. Do not use the PC's mouse or
   the Unity windows while the run is going.
4. New Code session in LAB. Add RALLY as a folder. Model **Sonnet**, effort **Medium** (your default for Sonnet is
   High, so change it), permission mode **Auto**.
5. Paste the launch prompt below. If the prompt improver asks, choose **Send as is**.

```text
Execute the game management mega ticket now. You are the chief of staff. Read
LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/Wayfinder/mega-07-09/MEGA-TICKET.md
in full, then run it from preflight to the final report.

Work fully autonomously. I will be away from the computer. Never ask me a question, never wait for my approval,
and never end a turn waiting for me. If something is not written down, decide it the way the ticket says and
record it.

This includes the Unity Editors. Pop-ups and dialogs will appear and nobody else will click them. Whenever a
Unity command hangs or times out, or an Editor is blocked, check both Editors for a pop-up, read it, and answer it
yourself by the "Pop-ups" rules in SESSION-BRIEF.md, then confirm the Editor responds and carry on. I authorize
you to bring the Editor windows to the front and press their dialog buttons, including with the mouse through the
dialog scripts. Choose the safe button: never discard my work, accept terms, sign in, upgrade a project or
package, or allow firewall or other security access. Every nested session and verifier follows the same rules.

Stop only for the ticket's "Stop conditions", and then write the final report saying exactly how to resume.
Running this ticket is my authorization for what it lists under "Authorized by launching this ticket".
```

## Authorized by launching this ticket

- Commits: library (per task, by path, branch rules in PLAN-OVERVIEW), Lab (submodule pointer only), Rally (submodule
  pointer only). No Lab or Rally push.
- Library push of `game-management-rework` and `main` when a ticket closes and its tests pass. Never force-push.
- Moving the Rally library clone forward to tested `game-management-rework` commits.
- One-level Windows player builds of Rally, launching the **development** build, and the temporary Rally settings
  changes in RALLY-DOGFOOD.md (all restored).
- Nothing else outward-facing: no PRs, messages, uploads, Steam writes or purchases.

## Preflight (chief, once)

Stop with a final report if a **stop** row fails.

| # | Check | How | If it fails |
|---|---|---|---|
| 1 | Chief's model, effort, mode | `get_session` `self`: Sonnet, `medium`, `auto` | Record in the ledger, continue (it cannot change itself) |
| 2 | CLI | `claude --version` (verified on 2.1.329) | stop |
| 3 | Verifier definition loads | Run the "Probe" command below; the reply must quote the brief's first heading | stop |
| 4 | Verifier ran as Sonnet High, probe session as Sonnet Medium | "Effort audit" below | stop |
| 5 | Both Editors answer | `unity command editor_status` from LAB and from RALLY | Try `unity open <project>` (unity-cli skill) once; else stop |
| 6 | Plan limits | `get_usage` | Apply "Usage limits" |
| 7 | Owner's files | Save `git status --porcelain` and a hash of every modified or untracked file in LAB, LIB, RALLY, RALLY LIB to `LOGS\start-state.txt` | stop |

Also call `request_keep_awake` (`session_idle`) once. It does not replace owner step 2.

## Run order

| Step | Session folder under `CONTEXT\Sessions\` | Spec | Initial cap (USD) | Expected 5-hour % |
|---|---|---|---|---|
| S0 | `06-state-inspector-build` | `tickets/06-...` "Mega run" | 12 | 8 |
| S1 | `07-onboarding-build` | `tickets/07-...` | 25 | 18 |
| S2 | `08-debugger-overlay-build` | `tickets/08-...` | 30 | 25 |
| S3 | `09-debugger-window-build` | `tickets/09-...` | 20 | 15 |
| S4 | `mega-07-09-acceptance` | [S4-RALLY-ACCEPTANCE.md](S4-RALLY-ACCEPTANCE.md) | 12 | 6 |

The caps and percentages are estimates, not limits on the work. Record real numbers in the ledger and correct later
caps from them. Run the steps in order; start one only after the previous step passes the gate.

## Mechanism

All commands run in the chief's Bash tool (Git Bash) from LAB.

**Launch.** `mkdir -p Logs/mega-07-09` once. Write the prompt to `LOGS\<step>.prompt.md`. Make a UUID, write it to the ledger first, then run this with
`run_in_background: true` (the app wakes the chief when it exits; do not poll):

```bash
RUN="$PWD/LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/Wayfinder/mega-07-09"
claude -p --model sonnet --effort medium --permission-mode auto \
  --disallowedTools AskUserQuestion \
  --add-dir "C:/Users/avig7/Desktop/AllUnity/ActiveProjects/ProjectRallyShift" \
  --agents "$RUN/gm-verifier.json" \
  --session-id "$SESSION_ID" \
  --max-budget-usd "$CAP" \
  --output-format json < "Logs/mega-07-09/$STEP.prompt.md" > "Logs/mega-07-09/$STEP.$N.json" 2> "Logs/mega-07-09/$STEP.$N.err"
```

**Resume** (extension, follow-up or gate failure): the same command with `--resume "$SESSION_ID"` in place of
`--session-id`, a new `$N`, and a short prompt on stdin. Pass every other flag again; they are not remembered by
design. Never use `--dangerously-skip-permissions` or `bypassPermissions`.

**Read the result.** From the JSON take only `subtype`, `is_error`, `result`, `total_cost_usd`, `permission_denials`.
(If the file holds a list, use its last item.) `total_cost_usd` is cumulative for the session across resumes.

| `subtype` | Meaning | Action |
|---|---|---|
| `success` | The session ended its turn | Read the STATUS block, then run the gate |
| `error_max_budget_usd` | Cap reached | "Budgets" |
| anything else, or no JSON | Crash, kill or usage limit | Read the `.err` file; handle limits per "Usage limits"; else resume once. A second identical failure: new session for the same step |

**Probe** (preflight 3):

```bash
echo 'Setup probe. Change nothing. Start the gm-verifier agent with the prompt "SETUP PROBE: read your brief, reply with its first heading and your tool names, then stop." Pass no model or effort. Reply with its answer.' | claude -p --model sonnet --effort medium --permission-mode auto --disallowedTools AskUserQuestion --agents "$RUN/gm-verifier.json" --session-id "$PROBE_ID" --max-budget-usd 1.5 --output-format json
```

**Effort audit** (after the probe and after every step). Count assistant messages only: a plain text search also
finds model names in the system prompt and the configured advisor model.

```bash
T="/c/Users/avig7/.claude/projects/C--Users-avig7-Desktop-AllUnity-ActiveProjects-Abstract-Pixel-Lab"
python -I - "$T/$SESSION_ID" <<'EOF'
import json, sys, glob, collections
base = sys.argv[1]
def show(label, path):
    seen = collections.Counter()
    for line in open(path, encoding='utf-8', errors='replace'):
        entry = json.loads(line)
        if entry.get('type') == 'assistant' and isinstance(entry.get('message'), dict):
            seen[(entry['message'].get('model'), entry.get('perTurnEffort'))] += 1
    print(label, dict(seen))
show('session', base + '.jsonl')
for meta in glob.glob(base + '/subagents/*.meta.json'):
    show(json.load(open(meta, encoding='utf-8')).get('agentType'), meta.replace('.meta.json', '.jsonl'))
EOF
```

Pass: the session shows only `('claude-sonnet-5-5', 'medium')`; every `gm-verifier` only
`('claude-sonnet-5-5', 'high')`; no other agent type verified; the result JSON's `modelUsage` lists no model but
Sonnet 5.5. Sessions never call the advisor tool (it is set to Opus).

Proven on 2026-11-28 with live probes: a headless session ran at Medium and `gm-verifier`, loaded from
`gm-verifier.json`, ran at High with this brief read and no `Edit` or `Agent` tool; auto mode ran Bash and `unity`
with no denial; `--resume` kept the conversation; a session stopped at its cap finished after a resume with a higher
cap. **Not proven:** a background command lasting hours (the Bash tool sets no limit on background commands), and
auto mode's answer to builds, pushes and launching the player (a denial follows rule 1).

## Session prompt template

Keep it this short. The session reads the files itself.

```text
You are the nested session for step <S#> of the game management mega ticket.
Read, in order: <RUN>\SESSION-BRIEF.md, then <spec file> (for a ticket, all of it; its "Mega run" section wins).
Previous handoff: <path, or "none">.
Session folder: <path>. Report with the STATUS block from the brief.
<only if needed: facts from the chief, three lines at most>
```

## Gate between steps (chief)

All must hold. Check by file and git output, not by trusting the report.

1. STATUS is `DONE`.
2. `<session folder>\HANDOFF.md` has every heading in the brief's template, including **Next session**.
3. The newest `VERIFY-<n>.md` in the session folder ends with `VERDICT: PASS`, and no later commit changed code
   (`git log` after the commit the verdict names shows docs, pointer or map commits only).
4. Effort audit passes.
5. LIB is on `main` with no uncommitted change of this step; RALLY LIB is on `game-management-rework` at the commit
   the handoff names, and `SaveableAttribute.cs` exists in it.
6. Files in `LOGS\start-state.txt` that the handoff does not list as changed on purpose still have the same hash.
7. Both Editors answer `editor_status`, not compiling, not blocked by a dialog.
8. The map row is `done` (S0-S3).

A failed gate: resume that session with only the failed items. Do not start the next step.

## Budgets

The run takes whatever it needs to finish the must-haves. Numbers are checkpoints, not a ceiling.

**Two meters, both kept by the chief in the ledger:**
- **Hard cap per session:** `--max-budget-usd`. Native, never opens a dialog, extended by resuming with a higher number.
- **5-hour window %:** `get_usage` before and after every launch or resume. Count across resets.

Do **not** use the budget tool's `start`: at its wrap point it opens an "Add budget or wrap up?" dialog that waits for
the owner, and over budget it blocks subagents. This replaces the per-ticket budget rule for this run only.

**When a cap is reached:** read the session's `PROGRESS.md` (nothing else).

| State | Action |
|---|---|
| A must-have, verification, a verifier finding, a commit or the handoff is unfinished | Extend: resume with cap = previous cap + 40% of the initial cap |
| Only nice-to-haves are left | Resume with + $4: "Drop nice-to-haves. Finish the handoff and report." |
| Something would block the next step | Extend until it is fixed, then gate again |
| No progress (no new tick, closed finding or commit) over two extensions in a row | End that session. Start a fresh one for the same step from its PROGRESS.md and newest VERIFY file |
| The fresh session also makes no progress over two extensions | Stop conditions |

**A blocker found for a later step** is fixed before that step starts: resume the session that owns it, or start a
fix session for it (initial cap $10, same verification rule, extend the same way).

Ledger row, one per launch or resume: step, session id, cap, cumulative USD, 5-hour % before and after, outcome,
reason for an extension.

## Usage limits

| `get_usage` before a launch or resume | Action |
|---|---|
| 5-hour used under 60% | Go |
| 5-hour 60% or more, or a session just failed on the limit | Wait for the reset: `sleep <seconds to reset + 120>` with `run_in_background: true`, then check again |
| Weekly 97% or more, or the failure says weekly limit | Stop conditions |

Never enable extra usage or switch to another account or API key.

Known risk: if the 5-hour limit ends the chief's own turn, nothing wakes it. The 60% start gate makes that
unlikely. If it happens, the owner sends "continue"; the chief resumes from the ledger.

## Stop conditions

Stop cleanly instead of asking, and only for these:
- weekly limit reached;
- a must-have needs a login, a purchase, hardware, or an action permissions deny with no allowed alternative;
- the no-progress rule ran out;
- the launch mechanism or an Editor cannot be made to work;
- finishing would require destroying or overwriting the owner's work.

To stop: leave both repos compiling and committed or clearly listed, Rally on a tested commit, restore every temporary
setting and the save folder, then write the final report with the exact blocker and the command that resumes the run
(`--resume <id>`).

## Resuming the chief

If the chief's context is lost or it is restarted, read this file, LEDGER and the newest handoff. Never redo a gated
step.

## Final report (chief's last message, also `CONTEXT\Sessions\mega-07-09\FINAL-REPORT.md`)

1. Result per step: done or stopped, verify rounds, commits.
2. Rally: library commit it is on, and which editor UIs are there now.
3. Owner review: still paths (drawer, onboarding, overlay, window), the dogfood build path.
4. Decisions made without the owner.
5. Not verified, and where it is carried (ticket 11).
6. Uncommitted or unpushed items and any permission denial.
7. Budget: ledger totals and every extension.

Then commit LEDGER and FINAL-REPORT on library `main` by path (no push). The run ends after S4. Do not start
ticket 10.
