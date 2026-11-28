# Headless fallback (mega run)

Use this only when the app refuses `start_session`, and tell the owner first. Otherwise do not read this file.
Parent: [MEGA-TICKET.md](MEGA-TICKET.md). A verifier step is two sessions, Part A then Part B; each has its own
session id.

## Mechanism

The headless fallback. All commands run in the chief's Bash tool (Git Bash) from LAB.

**Launch.** `mkdir -p Logs/mega-07-09` once. Write the prompt to `LOGS\<step>.prompt.md`. Make a UUID, write it to the ledger first, then run this with
`run_in_background: true` (the app wakes the chief when it exits; do not poll):

```bash
# EFFORT: medium for a builder, high for a verifier
claude -p --model sonnet --effort "$EFFORT" --permission-mode auto \
  --disallowedTools AskUserQuestion Agent \
  --add-dir "C:/Users/avig7/Desktop/AllUnity/ActiveProjects/ProjectRallyShift" \
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

**Effort audit** (after every headless step). In the result JSON, `modelUsage` lists no model but Sonnet 5.5. In the
transcript, count assistant messages only (model and `perTurnEffort`): builder Medium, verifier High. No subagents.
For a visible session, check its model and effort with `get_session`. Sessions never call the advisor tool.
