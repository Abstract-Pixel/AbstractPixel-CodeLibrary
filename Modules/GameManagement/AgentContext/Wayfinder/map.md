# Wayfinder map: game management, commercial-ready

## Destination

A game management module about 90% ready for the Unity Asset Store: deep state machine, validated setup, onboarding,
runtime debugger, in-Editor docs, Rally migrated with every flow matching today's behaviour.

## Notes

- Domain: Unity runtime and editor tooling, in AbstractPixelLibrary. Rally Shift is the dogfood project.
- Skills: codebase-design for code shape; grilling + domain-modeling for decision tickets; ui-uitk for editor tooling;
  ui-ugui for menus; unity-pipeline and unity-cli for the Editor.
- Standing preferences: easy setup over features, minimal code, no strings in setup, plain wording, verbose readable code,
  max three parameters, composition over flags.

## Decisions so far

- [Planning grill, rounds 1-3](../DECISIONS.md): architecture, edge-case rules, branches, asset locations, transition time; see DESIGN.md.
- [Session 1 flow inventory and helper audit](../FLOWS.md) (tickets 01, 03): after unpause no state is active today, because no Pause-to-Play trigger is serialized anywhere. Parity baseline in [../Parity/](../Parity/README.md).

- [Session 2 core state machine](../Sessions/Session2/HANDOFF.html): plain C# `GameStateMachine` in `Runtime/Machine/`
  with 33 new tests; new `StateSO` fields (`Parents`, `Effects`) beside the old ones; settings asset kept in Preloaded Assets.
- [Definition of done](../DEFINITION-OF-DONE.md): what finishing a task and a session means. Every prompt links it.

## Open tickets

- [01 Flow inventory](issues/01-flow-inventory.md) (task, AFK): resolved in Session 1.
- [02 Editor and onboarding UI design](issues/02-ui-design-grill.md) (grilling, HITL): precursor Session 5. Blocked by 01.
- [03 Rally helper script audit](issues/03-rally-helper-audit.md) (task, AFK): resolved in Session 1, part of the inventory.

## Not yet specified

- Ease curve for the time effect (Session 2 has seconds only; a Session 3 nice-to-have).
- Exact shape of the transition record and the debugger's timeline view (settled in ticket 02).
- How the migration orders states whose old priorities tie (needs the inventory; Rally's only tie, the two Settings
  states, disappears when they merge).
- Whether Rally's own folder `Assets/[1]Project/ProjectServices/GameManagment` is also renamed (Session 1 reports the
  reference count; the user decides).
- Docs topic list for the documentation engine (after Session 8).

## Out of scope

- Settings menu commercial readiness (needs its own plan).
- Asset Store packaging, demo art, translations, old Input Manager, audio backends other than BroAudio.
