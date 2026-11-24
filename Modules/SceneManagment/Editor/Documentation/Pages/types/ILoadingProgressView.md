# ILoadingProgressView

<!-- layout new -->

The [loading progress view](term:loading-progress-view) as the scene system sees it: told when to appear, waited on before Hide screen.

<!-- type-header AbstractPixel.SceneManagement.ILoadingProgressView -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/ILoadingProgressView.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## Start here

`LoadingProgressView` already implements it; write your own only for a view of a different kind. The loading scene registers one with `SceneActions.RegisterLoadingProgressView`.

| You want to | Use | Why |
|---|---|---|
| Start the appear animation | `OnShowStarted` | Called when Show screen starts, or at the instant cut. |
| Play a "full" animation | `PlayFullAnimation` | Hide screen waits for its task, so the effect and the loading scene never animate over each other. |

## In code

Each example is closed; open the one you need.

**Example: LoadingProgressView implements it and registers itself. A view of…**

`LoadingProgressView` implements it and registers itself. A view of your own does the same:

```cs
public class MyProgressView : MonoBehaviour, ILoadingProgressView
{
    private void OnEnable() { SceneActions.RegisterLoadingProgressView(this); }
    private void OnDisable() { SceneActions.UnregisterLoadingProgressView(this); }

    public void OnShowStarted() { /* start the appear animation */ }
    public Task PlayFullAnimation() { return Task.CompletedTask; }
}
```

<!-- members AbstractPixel.SceneManagement.ILoadingProgressView -->
