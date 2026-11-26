# IRelocationHold

<!-- layout new -->

For a component that needs its own hold, such as a vehicle script that must stop its engine force.

<!-- type-header AbstractPixel.SceneManagement.IRelocationHold -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Relocation/Runtime/IRelocationHold.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#what-happens-in-a-scene-change).

## Start here

Any component in a relocatable's hierarchy that implements it gets the calls, with no registration.

| You want to | Use | Why |
|---|---|---|
| Pause your own behaviour | `OnHold` | Called when the relocatable is held. |
| Resume it | `OnRelease` | Called when the relocatable is released. |

## In code

Each example is closed; open the one you need.

**Example: Put a component like this on a relocatable to pause your own…**

Put a component like this on a relocatable to pause your own behaviour while it is held:

```cs
public class EngineHold : MonoBehaviour, IRelocationHold
{
    public void OnHold() { enabled = false; }
    public void OnRelease() { enabled = true; }
}
```

<!-- members AbstractPixel.SceneManagement.IRelocationHold -->
