# LoadingProgressFeel

<!-- layout new -->

How a loading progress view's bar moves: smoothing speed, an optional curve and a hold at full.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LoadingProgressFeel -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/LoadingProgressFeel.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## Start here

You set it in the Feel foldout of a `LoadingProgressView`. It is plain data and maths, so you rarely call it.

- **Moves:** the shown value moves towards the raw loading progress at a set speed, never past it and never back.
- **Shapes:** an optional curve shapes what is shown.
- **Full:** means the shown value has reached 1.

| You want to | Use | Why |
|---|---|---|
| Read the feel from code | `LoadingProgressView.Feel` | Returns this object. |
| Know if the bar is full | `IsFull` | True once the smoothed value reaches 1. |

**Called for you, leave alone**

- **Restart and Step:** the view calls them when it shows and every frame.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Smoothing Speed | How fast the shown value catches up, in progress per second (unscaled). 1 fills an empty bar in one second. Default 1.5. | The bar lags behind a fast load, or should fill more calmly. |
| Curve | Optional: x is the smoothed progress, y what is shown. Empty shows it as it is. | The bar should speed up or slow down. |
| Hold At Full Seconds | How long the view holds at full before Hide screen starts, in unscaled seconds. Default 0.3. | The player should see the bar reach 100%. |

<!-- members AbstractPixel.SceneManagement.SceneScreens.LoadingProgressFeel -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
