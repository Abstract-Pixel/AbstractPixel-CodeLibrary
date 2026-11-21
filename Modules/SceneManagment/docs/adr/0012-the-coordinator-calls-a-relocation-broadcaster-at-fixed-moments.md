# The coordinator calls a relocation broadcaster at fixed moments

Relocation used to hang on messages: Rally moved the car when its spawn point woke up and unlocked it on a screen effect event. That broke three ways: no message marks the start of a scene change, the loading scene's own Show screen fires the same event long before the level exists, and start-up has no Show screen at all. So the scene system defines `IRelocationBroadcaster` (`Hold()`, `Task Relocate()`, `Release()`), the relocation sub-module registers it like the scene transition executor (ADR 0001), and the coordinator calls it in every scene change, start-up and fallback included: Hold after the change is accepted and before Hide screen; Relocate once every scene of the group is open and the main scene is active, awaited before "new level" is raised, before the loading scene leaves and before the last Show screen; Release when that last Show screen reports that the screen counts as visible (a transition profile setting, default 0.75), at the end of Show screen if it never reports, straight after Relocate when no executor is registered, and after the launch scene hold during start-up. Relocate finishes only when each relocatable is verified at its position one frame later and, with Wait for ground below, a collider is found under it (or a timeout passes), so the screen is never shown before relocation is safe. Every wait counts unscaled frames, never FixedUpdate, because game time can be 0 during a scene change.

## Considered options

- Listen to scene and screen messages (the plan before 2026-11-21). Rejected for the three reasons above.
- Release a fixed time after Show screen starts, from a scene system setting. Rejected: it drifts when the effect hitches or changes length. The executor reports the moment itself.

## Consequences

`ISceneTransitionExecutor`'s Show screen gains a way to report when the screen counts as visible. The scene system still never references the relocation sub-module.
