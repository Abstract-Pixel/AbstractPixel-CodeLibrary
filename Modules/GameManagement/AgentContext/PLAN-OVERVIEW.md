# Game management, commercial-ready: rules for every session

**The route (what runs next, in what order) is in [Wayfinder/map.md](Wayfinder/map.md) and nowhere else.** This file
holds only the rules every session follows. Each session works one ticket from the map; the ticket says what to read,
what must be done and how it is verified. Decisions already made in DECISIONS.md, DESIGN.md, FLOWS.md, NAMING.md and
UI-STYLE.md are followed, not re-decided.

## Must-haves, in order

1. Time can no longer soft-lock (done in Sessions 2-4; guarded by the watchdog, ticket 10).
2. Every Rally flow matches (done in Session 4; kept by the parity scenarios below).
3. Setup problems show with fixes (tickets 05, 06).
4. A new game can be set up from the onboarding window (ticket 07).
5. The code is stable in Unity 6.3, 6.7 and 7 (tickets 04 and 11).

Nice-to-haves, only when a ticket has budget left: the full side-by-side effect compare, journal export, the
release-build opt-in UI polish, moving `StateAudioVolumeController` into the library, extra samples and doc images.

## Transition break scenarios (Rally parity)

Each runs through `bash AgentScripts/Probes/run_scenario.sh`, sampling every frame of the scene change:
`Time.timeScale`, the bomb timer value, the car position and the top state. Pass means the bomb does not tick and the
car does not move before the level screen is visible (`CarRelease` and `BombInitializer` frame), and nothing
soft-locks. Compare with `Parity/baseline/` (see `Parity/README.md`).

1. Game Over, then Retry (scene change starts with time 0 or mid-ease).
2. Game Win, then Next Level and Retry.
3. Pause, then Quit to Main Menu (time 0 at the start of the change).
4. Main Menu to a level through the loading screen (time 1 at the start).
5. A scene change requested during Game Over's 1.4 s delay and its ease.
6. Pause pressed many times during loading (the pause gate must still block it).
7. Retry spammed during a scene change.
8. Play from a lone level in the Editor (no state active on the first frame).
9. Level select back to Main Menu, then Settings, during and right after the change.

## Rules for every session

**Mega run sessions do not read this file.** Their rules are their role brief in
[Wayfinder/mega-07-09/](Wayfinder/mega-07-09/MEGA-TICKET.md) and ENGINEERING-RULES.md. This file is for single-ticket
sessions the owner starts.

- **Follow [ENGINEERING-RULES.md](ENGINEERING-RULES.md)** (code, repos, commits, context limit, check record) and
  read [Tools/TRAPS.md](Tools/TRAPS.md).
- **The owner's request wins.** If the owner asks for a different ticket than the map's next one, do what they asked
  and update the map. Never switch to another ticket because a handoff or this file suggests it.
- **Budget.** First tool call: `budget start` with `plan: gm-finish` and the ticket's step. Do the must-haves first;
  list skipped nice-to-haves in the handoff. Never leave Rally broken.
- **Looks first.** A UI piece is built only after the owner called its look final from stills the agent captured and
  read at three widths. "Compiles, 0 errors" is never evidence of a good look.
- **Handoff.** Every ticket ends with `Sessions/<ticket id>/HANDOFF.md`: what changed and why, how it affects a
  developer using the module, what was not done, findings, budget used and whether a warning fired. Then mark the
  ticket `done` on the map and add one line to its "Decisions so far". Don't write prompts for other tickets; the map's
  tickets are the prompts. (A mega run handoff's **Next session** section says what state the next session starts from
  and what to do first; it is not a replacement prompt.)
- **Carry forward into later tickets.** When a session learns something a later ticket needs (a new rule, a bug, a
  changed file or type name, a decision the owner made, a requirement that must not be lost), it adds a note at the
  end of that ticket's file under a `## From ticket NN` heading: what it is, why it matters, and where the detail is
  (handoff section, file path). Notes may add must-haves or checks. They never change the ticket's goal, its "Blocked
  by" or the map's order; propose those in the handoff for the owner to decide. The session working a ticket reads its
  "From ticket" notes before starting.
