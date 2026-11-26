# SpawnPoint

<!-- layout new -->

A component in a main scene that holds the positions where relocatables with its relocation label go in that scene group (ADR 0013).

<!-- type-header AbstractPixel.SceneManagement.SpawnPoint -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Relocation/Runtime/SpawnPoint.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#add-spawn-points).

## Start here

Its own transform is its first position; more positions are data, each with its instance. Use one spawn point per label per area, never one GameObject per object.

| You want to | Use | Why |
|---|---|---|
| Add a position | Shift+click in the Scene view | Adds a position at the ground you click. |
| Add a position from code | `AddPosition` | Returns its position number. |
| Read where a position is | `GetWorldPose` | Stored positions follow the spawn point's parents, scale included. |
| Remove a position | `RemovePosition` | The last remaining position cannot be removed; delete the spawn point instead. |

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Label | Relocatables with this label go to this spawn point's positions. Empty means No label. | It serves a labelled kind of relocatable. |
| First instance | The instance of the first position, which is this object's own transform. | A relocatable asks for a set instance. |
| More positions | The stored positions, each a `SpawnPointPosition` with its instance. | Normally edit them in the Scene view instead. |

## In code

Each example is closed; open the one you need.

**Example: Read or add positions from code (the Scene view tools do the same)**

```cs
[SerializeField] private SpawnPoint spawnPoint;

private void AddOneMore()
{
    Pose first = spawnPoint.GetWorldPose(0);
    Pose next = new Pose(first.position + Vector3.right * 2f, first.rotation);
    spawnPoint.AddPosition(next, spawnPoint.PositionCount + 1);
}
```

<!-- members AbstractPixel.SceneManagement.SpawnPoint -->
