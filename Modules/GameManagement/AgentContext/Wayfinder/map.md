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
- [Session 3 adapters and sink](../Sessions/Session3/HANDOFF.html): Unity sink and driver, input source seam,
  request adapters, state watch and reactors, migration tool; `Runtime/Machine/` split to one type per script. 74 tests.
- Workflow rules (2026-11-26): every session prompt has its own "Done and verified" section; one type per script.
- [Definition of done](../DEFINITION-OF-DONE.md): what finishing a task and a session means. Every prompt links it.
- Plan change (2026-11-26): Session 4 gets a sample game copying Rally's flows for parity in both Editors; Session 4B
  (names, folders, core cleanup) is its second part; together 18%. Naming rules in [../NAMING.md](../NAMING.md). Grill round 1:
  Unity 7 checks run full tests in the Lab via its library submodule; prompts name what to read before each stage;
  commits by default; scene scripts that make requests are called requesters. Follow-up: **no strings configure
  anything**; input maps are chosen by `InputActionReference` (DECISIONS.md, 2026-11-26).

## Open tickets

- [01 Flow inventory](issues/01-flow-inventory.md) (task, AFK): resolved in Session 1.
- [02 Editor and onboarding UI design](issues/02-ui-design-grill.md) (grilling, HITL): precursor Session 5. Blocked by 01.
- [03 Rally helper script audit](issues/03-rally-helper-audit.md) (task, AFK): resolved in Session 1, part of the inventory.

## Not yet specified

- Ease curve for the time effect (seconds only so far; skipped in Session 3 as a nice-to-have).
- Exact shape of the transition record and the debugger's timeline view (settled in ticket 02).
- Migration ties: resolved in Session 3 (ordered by name and reported as problems).
- Whether Rally's own folder `Assets/[1]Project/ProjectServices/GameManagment` is also renamed (Session 1 reports the
  reference count; the user decides).
- Docs topic list for the documentation engine (after Session 8).

## Out of scope

- Settings menu commercial readiness (needs its own plan).
- Asset Store packaging, demo art, translations, old Input Manager, audio backends other than BroAudio.

- 2026-11-26 Session 4 (partial): S3 committed; Sample 01 on the new system, flow test passes in Rally; Lab blocked (branch behind main). Next: Session 4 continues from task 2. See Sessions/Session4/HANDOFF.html.
