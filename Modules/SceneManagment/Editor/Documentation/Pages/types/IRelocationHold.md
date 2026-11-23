# IRelocationHold

For a component that needs its own hold, such as a vehicle script that must stop its engine force. Any component in a relocatable's hierarchy that implements it gets OnHold when the relocatable is held and OnRelease when it is released, with no registration.

**Kind:** interface · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `IRelocationHold.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/IRelocationHold.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#what-happens-in-a-scene-change).

## Members

<!-- members AbstractPixel.SceneManagement.IRelocationHold -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
