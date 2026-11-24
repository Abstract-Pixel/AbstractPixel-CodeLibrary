# LoadingProgressView

<!-- layout new -->

The component in a loading scene that shows the [loading progress](term:loading-progress) to the player, smoothed.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LoadingProgressView -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/LoadingProgressView.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## Start here

Add it to an object in your loading scene, then connect your bar through an event or an `ILoadingProgressDisplay`. It reads `SceneActions.LoadingProgress`, smooths it with its Feel settings and passes the result on.

| You want to | Use | Why |
|---|---|---|
| Fill a bar with no code | On Progress | Drag in your bar and pick its fill function; a designer can also drag in a feedback player. |
| Fill a bar from your own script | `ILoadingProgressDisplay` | Put it on this object, or assign it to Display. |
| Change how the bar moves | Feel | Speed, curve and hold at full, in one foldout. |
| Build the view at runtime | `SetDisplay` | For setups and tests that make the view in code. |

**Called for you, leave alone**

- **OnShowStarted:** the scene system calls it when Show screen starts.
- **PlayFullAnimation:** the scene system waits for its "full" beat and hold before Hide screen, so the screen effect and the loading scene never animate over each other.
- **Registering:** it registers itself with `SceneActions.RegisterLoadingProgressView` when enabled.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Display | Optional: a component that implements `ILoadingProgressDisplay`. Empty uses one on this object, if any. | Your bar's script sits on another object. |
| Feel | A foldout with the `LoadingProgressFeel` settings. Not the Feel package. | The bar should move faster, slower or hold longer. |
| **Events** | | |
| On Shown | Show screen started on the loading scene: start the appear animation. | The bar has an appear animation. |
| On Progress | Every frame while shown: the smoothed loading progress, from 0 to 1. | You fill your bar with no code. |
| On Full | Every scene is open and the shown value reached 1: play the full beat. | You want a finishing flash or sound. |
| On Hidden | The loading scene is closing. | You stop sounds or reset effects. |

## In code

Each example is closed; open the one you need.

**Example: Most setups need no code. A display written in code is handed to…**

Most setups need no code. A display written in code is handed to the view like this:

```cs
[SerializeField] private LoadingProgressView progressView;
[SerializeField] private SimpleLoadingProgressBar bar;

private void Awake()
{
    progressView.SetDisplay(bar);
}
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LoadingProgressView -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
