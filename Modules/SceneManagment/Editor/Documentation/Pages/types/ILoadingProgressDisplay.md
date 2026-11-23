# ILoadingProgressDisplay

A look for the loading progress view written in code, such as an animation command sequence. The view finds one on its own object or uses the one assigned, and drives it: Show when Show screen starts, SetProgress with the smoothed value every frame, PlayFull once every scene is open (the view waits for its task), Hide when the loading scene closes.

**Kind:** interface · **Namespace:** `AbstractPixel.SceneManagement.SceneScreens` · **File:** `ILoadingProgressDisplay.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/ILoadingProgressDisplay.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## In code

From a sample:

```cs source=../../../../Samples/06 Loading Scene/Scripts/Sample06BarDisplay.cs
public class Sample06BarDisplay : MonoBehaviour, ILoadingProgressDisplay
```

## Members

<!-- members AbstractPixel.SceneManagement.SceneScreens.ILoadingProgressDisplay -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
