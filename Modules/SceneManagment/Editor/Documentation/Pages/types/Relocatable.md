# Relocatable

A relocatable: an object in a manager scene or shared scene that the scene system moves to a spawn point position on every scene change, such as the player's car or a pooled enemy. It is held while the scene changes (its Rigidbodies kinematic, CharacterControllers off, NavMeshAgents stopped), relocated once the new scene group is open, and released once the screen counts as visible again.

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `Relocatable.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/Relocatable.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#make-an-object-a-relocatable).

One component and its relocation label are all it needs: no physics lock, no unlock script, no code.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Label | `label` | Relocatables go to spawn point positions with the same label. Empty means No label. |
| Instance | `instance` | Instance: picks one spawn point position with this label. 0 takes the next free one in Hierarchy order. |
| Wait for ground below | `waitForGroundBelow` | Wait for ground below: stay held, with the screen covered, until a collider on the Ground layers is found under the spawn point position (3 s at most). It never moves the object onto the ground. |
| On relocated | `onRelocated` | Wait for ground below: stay held, with the screen covered, until a collider on the Ground layers is found under the spawn point position (3 s at most). It never moves the object onto the ground. |
| On released | `onReleased` | Wait for ground below: stay held, with the screen covered, until a collider on the Ground layers is found under the spawn point position (3 s at most). It never moves the object onto the ground. |

## Members

<!-- members AbstractPixel.SceneManagement.Relocatable -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
