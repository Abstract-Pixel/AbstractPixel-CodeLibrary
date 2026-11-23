# SpawnPoint

A spawn point: a component in a main scene that holds the positions where relocatables with its relocation label go in that scene group (ADR 0013). Its own transform is its first position; more positions are data, each with its instance.

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SpawnPoint.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/SpawnPoint.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#add-spawn-points).

Use one spawn point per label per area, never one GameObject per object. Shift+click in the Scene view adds positions.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Label | `label` | Relocatables with this label go to this spawn point's positions. Empty means No label. |
| First instance | `firstInstance` | The instance of the first position, which is this object's own transform. |
| More positions | `morePositions` | The instance of the first position, which is this object's own transform. |

## Members

<!-- members AbstractPixel.SceneManagement.SpawnPoint -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
