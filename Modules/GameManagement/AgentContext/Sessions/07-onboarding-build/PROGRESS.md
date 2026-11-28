# S1 progress (ticket 07, onboarding build)

Lab on `learn`; library `main` 45d6000 / `game-management-rework` 68c65b1; Rally library on 68c65b1 (compiled clean in 6.3).

- [x] Preflight: both Editors ready, library `main` 9f803cb, Lab preloaded had no GM settings
- [x] Logic + tests first: choices, plan text, folder check, catalog order (OnboardingPlanTests)
- [x] Builder: states, menus (copy + own), run again, rollback (OnboardingBuilderTests, OnboardingMenuButtonTests)
- [x] Package settings: project wins; package read-only; catalogs in packages never written (PackageSettingsGuardTests)
- [x] Settings Menu prefab; SystemPill + Scene System pill
- [x] Settings page: package warning, "Set up states...", setup switch
- [x] Window: 7 pages / 4 on a run later; menu entry; auto-open
- [x] Must-have 1 trials (stills in `stills/`): all copy, all own no prefab, mix with own prefabs + long names, only Play,
      cancel at Ready (nothing made), run again (add Game Win; add a menu to the existing Settings state). Lab restored.
- [x] Must-have 2: Settings page "6 states, no problems"; state inspector showed no Problems section (all copy, all own)
- [x] Must-have 3: Play mode on the all-copy result, by device (mouse, keyboard, gamepad), script-driven with queued device events
- [x] Must-have 4: auto-open: once per session, "Don't show again", re-enabled from the Setup card; rule test for batch/play/tests
- [x] Must-have 5: long folder and long prefab names at 330 / 620 / 1000 px
- [x] Must-have 6: Rally R1, R4 (pill), R5 (all 4 menus ticked and locked; closed without Set up; Rally files unchanged)
- [ ] Verify
- [ ] Finish steps (prototype removal, push, map, handoff)

## Commits (library)
- main 6774d52, a9b51bf gmr: window, builder, package guard, SystemPill (first)
- 650a121 / 36161f7: loose state names; d00300b / bee272d: Rally names; 8aab9e6 / 68c65b1: cleanup, read-me

## Pop-ups
None appeared (no modal dialogs in either Editor).

## Verify rounds
none yet
