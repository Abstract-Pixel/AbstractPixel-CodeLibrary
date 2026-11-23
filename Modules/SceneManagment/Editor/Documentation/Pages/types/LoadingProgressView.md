# LoadingProgressView

The part of a loading scene that shows loading progress to the player (glossary: loading progress view). It reads SceneActions.LoadingProgress, smooths it with its feel settings and passes it on through four UnityEvents (a designer can drag a progress bar or a feedback player in with no code) and to an ILoadingProgressDisplay written in code.

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement.SceneScreens` · **File:** `LoadingProgressView.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/LoadingProgressView.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

The scene system tells it when Show screen starts and waits for its "full" beat and hold before Hide screen, so the screen effect and the loading scene never animate over each other.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Feel | `feel` | — |
| Display | `display` | Optional: a component that implements ILoadingProgressDisplay. Empty uses one on this object, if any. |
| On shown | `onShown` | Show screen started on the loading scene: start the appear animation. |
| On progress | `onProgress` | Every frame while shown: the smoothed loading progress, from 0 to 1. |
| On full | `onFull` | Every scene is open and the shown value reached 1: play the full beat. |
| On hidden | `onHidden` | The loading scene is closing. |

## Members

<!-- members AbstractPixel.SceneManagement.SceneScreens.LoadingProgressView -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
