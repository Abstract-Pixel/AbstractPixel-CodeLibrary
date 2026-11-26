# SpawnPointPosition

<!-- layout new -->

One stored spawn point position (ADR 0013): a position and rotation local to its spawn point, and its instance.

<!-- type-header AbstractPixel.SceneManagement.SpawnPointPosition -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Relocation/Runtime/SpawnPointPosition.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#add-spawn-points).

## Start here

Positions are data on the spawn point, never GameObjects. You rarely touch this struct; use the `SpawnPoint` methods.

| You want to | Use | Why |
|---|---|---|
| Read a position's rotation | `SafeLocalRotation` | An all-zero rotation (a new list entry) reads as no turn. |

## In code

Each example is closed; open the one you need.

**Example: One position: where, which way, and its instance**

```cs
SpawnPointPosition position = new SpawnPointPosition(new Vector3(0f, 0f, 3f), Quaternion.identity, 2);
Quaternion rotation = position.SafeLocalRotation;
```

<!-- members AbstractPixel.SceneManagement.SpawnPointPosition -->
