# Relocatable

<!-- layout new -->

A component that moves an object in a manager scene or shared scene to a spawn point position on every scene change, such as the player's car or a pooled enemy.

<!-- type-header AbstractPixel.SceneManagement.Relocatable -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/Relocatable.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#make-an-object-a-relocatable).

## Start here

One component and its relocation label are all it needs: no physics lock, no unlock script, no code.

| You want to | Use | Why |
|---|---|---|
| Choose where it goes | `Label` | It goes to spawn point positions with the same label. Empty means No label. |
| Send it to one exact position | `Instance` | 0 takes the next free position in Hierarchy order. Code may set it before enabling a pooled object. |
| Run code once it has moved | `Relocated` | Raised once it is at its spawn point position, still held. |
| Turn controls on after the change | `Released` | Raised once per scene change, in the frame of release, after its physics is restored. |
| Check if it is paused | `IsHeld` | True during a scene change, or while it has no position. |

**What happens to it**

- **Hold:** while the scene changes its Rigidbodies turn kinematic, CharacterControllers turn off and NavMeshAgents stop.
- **Relocate:** it moves once the new scene group is open.
- **Release:** it is released once the screen counts as visible again.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Label | Relocatables go to spawn point positions with the same label. Empty means No label. | It is not the only kind of relocatable. |
| Instance | Picks one spawn point position with this label. 0 takes the next free one in Hierarchy order. | It must always start at the same position. |
| Wait for ground below | Stays held, with the screen covered, until a collider on the Ground layers is found under the spawn point position (3 s at most). It never moves the object onto the ground. | It must not fall before the ground has loaded. On by default when the object has a dynamic Rigidbody or a CharacterController. |
| On relocated | Inspector listeners of `Relocated`. | You want a reaction without code. |
| On released | Inspector listeners of `Released`. | You want a reaction without code. |

## In code

Each example is closed; open the one you need.

**Example: Listen to its events to react when the object has moved or is free…**

Listen to its events to react when the object has moved or is free again:

```cs
[SerializeField] private Relocatable relocatable;

private void OnEnable()
{
    relocatable.Relocated += OnRelocated;
    relocatable.Released += OnReleased;
}

private void OnDisable()
{
    relocatable.Relocated -= OnRelocated;
    relocatable.Released -= OnReleased;
}
```

<!-- members AbstractPixel.SceneManagement.Relocatable -->
