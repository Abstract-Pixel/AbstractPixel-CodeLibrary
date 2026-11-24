# SpawnPointPosition

<!-- layout new -->

One stored spawn point position (ADR 0013): a position and rotation local to its spawn point, and its instance.

<!-- type-header AbstractPixel.SceneManagement.SpawnPointPosition -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/SpawnPointPosition.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#add-spawn-points).

## Start here

Positions are data on the spawn point, never GameObjects. You rarely touch this struct; use the `SpawnPoint` methods.

| You want to | Use | Why |
|---|---|---|
| Read a position's rotation | `SafeLocalRotation` | An all-zero rotation (a new list entry) reads as no turn. |

<!-- members AbstractPixel.SceneManagement.SpawnPointPosition -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
