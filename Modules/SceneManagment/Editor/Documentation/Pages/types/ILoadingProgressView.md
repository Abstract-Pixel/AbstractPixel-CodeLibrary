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

<!-- members AbstractPixel.SceneManagement.ILoadingProgressView -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
