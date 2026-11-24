# IRelocationHold

<!-- layout new -->

For a component that needs its own hold, such as a vehicle script that must stop its engine force.

<!-- type-header AbstractPixel.SceneManagement.IRelocationHold -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/IRelocationHold.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#what-happens-in-a-scene-change).

## Start here

Any component in a relocatable's hierarchy that implements it gets the calls, with no registration.

| You want to | Use | Why |
|---|---|---|
| Pause your own behaviour | `OnHold` | Called when the relocatable is held. |
| Resume it | `OnRelease` | Called when the relocatable is released. |

<!-- members AbstractPixel.SceneManagement.IRelocationHold -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
