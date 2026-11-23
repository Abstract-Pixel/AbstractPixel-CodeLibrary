# IRelocationBroadcaster

The one thing the scene system asks to hold, relocate and release every relocatable during a scene change (ADR 0012). The relocation sub-module provides it and registers it with RegisterRelocationBroadcaster; with none registered, scenes change without relocation.

**Kind:** interface · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `IRelocationBroadcaster.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/IRelocationBroadcaster.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#what-happens-in-a-scene-change).

The coordinator calls it in every scene change, start-up and the fallback scene group included, once per step.

## Members

<!-- members AbstractPixel.SceneManagement.IRelocationBroadcaster -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
