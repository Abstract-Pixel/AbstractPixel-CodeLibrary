# Game management: agent context

Planning documents for making the game management module about 90% commercial-ready (ready for the Unity Asset Store).
**Start at [Wayfinder/map.md](Wayfinder/map.md): it alone says what runs next.** Then open the ticket it points to.

| File | What it holds |
|---|---|
| [Wayfinder/map.md](Wayfinder/map.md) | The route: destination, ordered tickets with status, decisions so far, fog, out of scope. |
| [Wayfinder/tickets/](Wayfinder/tickets/) | One file per ticket: goal, what to read, must-haves, done and verified. The ticket is the session prompt. |
| [Wayfinder/mega-07-09/](Wayfinder/mega-07-09/MEGA-TICKET.md) | The unattended run for the 06 close-out and tickets 07-09: [chief's ticket](Wayfinder/mega-07-09/MEGA-TICKET.md) (owner steps and launch prompt), [session brief](Wayfinder/mega-07-09/SESSION-BRIEF.md), [verifier brief](Wayfinder/mega-07-09/VERIFIER-BRIEF.md), [Rally dogfood](Wayfinder/mega-07-09/RALLY-DOGFOOD.md), [final acceptance](Wayfinder/mega-07-09/S4-RALLY-ACCEPTANCE.md), `gm-verifier.json`. |
| [PLAN-OVERVIEW.md](PLAN-OVERVIEW.md) | Rules every session follows, must-haves, Rally parity scenarios. |
| [DEFINITION-OF-DONE.md](DEFINITION-OF-DONE.md) | What "finished" means for any task. |
| [DECISIONS.md](DECISIONS.md) | Every decision the owner made. The source of truth when documents disagree. |
| [DESIGN.md](DESIGN.md) | The target design: the deep state machine, its seams, words. |
| [ANALYSIS.md](ANALYSIS.md) | How the old module worked (history). |
| [NAMING.md](NAMING.md) | How scripts are named and foldered (the read-alone test). |
| [UI-STYLE.md](UI-STYLE.md) | The approved editor UI look. Every UI piece follows it. |
| [FLOWS.md](FLOWS.md), [Parity/](Parity/README.md) | Rally's flows and the parity baseline. |
| [Sessions/](Sessions/) | One folder per finished session or ticket: its handoff and stills. Session5 also holds the UI spec (`GRILL-RESULTS.md`), research and capture tools. |
