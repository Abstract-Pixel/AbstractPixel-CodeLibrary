# Wayfinder map: game management, commercial-ready

**This file alone decides what runs next and in what order** (owner, 2026-11-27). Handoffs report what happened and may
suggest changes; they never pick the next session. If the owner asks for something else, do that and update this map.

## Destination

A game management module about 90% ready for the Unity Asset Store: deep state machine, validated setup, onboarding,
runtime debugger, in-Editor docs, Rally migrated with every flow matching today's behaviour, code stable in Unity 6.3,
6.7 and 7.

## Notes

- This map carries execution: tickets 04-12 build things, not only decide them.
- Domain: Unity runtime and editor tooling in AbstractPixelLibrary. Rally Shift is the dogfood project.
- Skills: prototype for looks; ui-uitk for editor UI; ui-ugui for menus; grilling + domain-modeling for owner
  questions; codebase-design for code shape; unity-pipeline and unity-cli for the Editor.
- Rules for every session: [../PLAN-OVERVIEW.md](../PLAN-OVERVIEW.md) and [../DEFINITION-OF-DONE.md](../DEFINITION-OF-DONE.md).
- Budget plan: `gm-finish`; ticket N is `plan: gm-finish, step N`.

## Route (work top to bottom; take the first open, unblocked ticket)

| # | Ticket | Type | Blocked by | Status |
|---|---|---|---|---|
| 01 | [Onboarding window look (7A-look)](tickets/01-onboarding-look.md) | prototype, HITL | none | open |
| 02 | [Debugger overlay look (8A-look)](tickets/02-debugger-overlay-look.md) | prototype, HITL | 01 | open |
| 03 | [Game State Debugger window look (8B-look)](tickets/03-debugger-window-look.md) | prototype, HITL | 02 | open |
| 04 | [Stability gate before any build](tickets/04-stability-gate.md) | task | none (before 05) | open |
| 05 | [Project Settings page build (6A-build)](tickets/05-settings-page-build.md) | implement | 01-04 | open |
| 06 | [State inspector and drawer build (6B-build)](tickets/06-state-inspector-build.md) | implement | 05 | open |
| 07 | [Onboarding window build (7A-build)](tickets/07-onboarding-build.md) | implement | 06 | open |
| 08 | [Debugger overlay build (8A-build)](tickets/08-debugger-overlay-build.md) | implement | 07 | open |
| 09 | [Game State Debugger window build (8B-build)](tickets/09-debugger-window-build.md) | implement | 08 | open |
| 10 | [Play-mode watchdog (was 6C)](tickets/10-watchdog.md) | implement | 09 | open |
| 11 | [Hardening walk](tickets/11-hardening-walk.md) | implement | 10 | open |
| 12 | [In-Editor docs, samples, final review](tickets/12-docs-and-final-review.md) | docs | 11 | open |

Ticket 04 runs no UI and needs Unity, so it may run beside the look tickets in its own session.
When a ticket closes: set its status here to `done`, add one line to "Decisions so far", and save its handoff in
`../Sessions/<ticket id>/HANDOFF.md`.

## Decisions so far

- [Planning grill, rounds 1-3](../DECISIONS.md): architecture, edge cases, branches, asset locations; see DESIGN.md.
- [Session 1 flow inventory and helper audit](../FLOWS.md): parity baseline in [../Parity/](../Parity/README.md).
- [Session 2 core state machine](../Sessions/Session2/HANDOFF.html): plain C# `GameStateMachine`, 33 tests.
- [Session 3 adapters and sink](../Sessions/Session3/HANDOFF.html): Unity sink and driver, input source, requesters, reactors; 74 tests.
- [Session 4 Rally migration](../Sessions/Session4/HANDOFF.html): Rally on the new system, parity gate 18/18, old system deleted.
- [Session 4B names and folders](../Sessions/Session4B/HANDOFF.html): 15 renames per [../NAMING.md](../NAMING.md). Its Rally checks and Lab flow-test rerun were never run; now ticket 04.
- [Session 5 UI grill](../Sessions/Session5/HANDOFF.md): spec in GRILL-RESULTS.md; one UI piece per session, look first.
- [Session 6A settings page look](../Sessions/Session6A/HANDOFF.md): approved; style rules in [../UI-STYLE.md](../UI-STYLE.md).
- [Session 6B state inspector look](../Sessions/Session6B/HANDOFF.md): approved; long names and real Input comparison required in the build.
- Replan (2026-11-27, owner): every look first (01-03), then a stability gate, then every build in order, then the
  watchdog, a hardening walk and docs. This map is the only source of the route. Old `gm-commercial` budget plan
  closed (127% spent of 77%); `gm-finish` covers the rest.

## Not yet specified

- Ease curve for the time effect (seconds only so far).
- Whether Rally's own folder `Assets/[1]Project/ProjectServices/GameManagment` is renamed (owner decides).
- Docs topic list for the documentation engine (settled in ticket 12).

## Out of scope

- Settings menu commercial readiness (needs its own plan).
- Asset Store packaging, demo art, translations, old Input Manager, audio backends other than BroAudio.
- Full side-by-side effect compare in the debugger (nice-to-have only if ticket 09 has budget left).
