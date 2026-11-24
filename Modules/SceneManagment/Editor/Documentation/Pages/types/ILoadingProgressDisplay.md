# ILoadingProgressDisplay

<!-- layout new -->

A bar look written in code, such as an animation command sequence, that a `LoadingProgressView` drives.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.ILoadingProgressDisplay -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/ILoadingProgressDisplay.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## Start here

Implement it on the view's object, or assign it to the view's Display field. The view calls every member for you.

| You want to | Use | Why |
|---|---|---|
| Start the appear animation | `Show` | Called when Show screen starts. |
| Fill the bar | `SetProgress` | Called every frame with the smoothed value. |
| Play a finishing beat | `PlayFull` | Called once every scene is open; the view waits for its task. |
| Clean up | `Hide` | Called when the loading scene closes. |

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/06 Loading Scene/Scripts/Sample06BarDisplay.cs
public class Sample06BarDisplay : MonoBehaviour, ILoadingProgressDisplay
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.ILoadingProgressDisplay -->
