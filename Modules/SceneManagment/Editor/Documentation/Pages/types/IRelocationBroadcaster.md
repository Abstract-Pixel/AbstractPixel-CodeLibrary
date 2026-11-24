# IRelocationBroadcaster

<!-- layout new -->

The one thing the scene system asks to hold, relocate and release every relocatable during a scene change (ADR 0012).

<!-- type-header AbstractPixel.SceneManagement.IRelocationBroadcaster -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/IRelocationBroadcaster.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#what-happens-in-a-scene-change).

## Start here

The relocation sub-module provides it and registers it with `SceneActions.RegisterRelocationBroadcaster`; you do not implement it. With none registered, scenes change without relocation.

**Called for you, leave alone**

- **Who calls it:** the coordinator, in every scene change, start-up and the fallback scene group included, once per step.
- **Hold:** after the change is accepted, before Hide screen.
- **Relocate:** once every scene of the group is open; the change waits for it.
- **Release:** once the screen counts as visible.

<!-- members AbstractPixel.SceneManagement.IRelocationBroadcaster -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
