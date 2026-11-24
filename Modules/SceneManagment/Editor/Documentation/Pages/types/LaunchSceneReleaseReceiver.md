# LaunchSceneReleaseReceiver

<!-- layout new -->

Put it next to the launch scene's PlayableDirector: it releases the launch scene hold when the Timeline reaches a `LaunchSceneReleaseMarker`.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseReceiver -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneReleaseReceiver.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

## Start here

Add the component to the object with the launch scene's PlayableDirector, then hook `On Released` to the code that turns the launch scene's own camera and listener off and closes the scene.

| You want to | Use | Why |
|---|---|---|
| Hand off at a Timeline frame | A `LaunchSceneReleaseMarker` on the Timeline | The receiver acts when the marker fires. |
| Close the launch scene after the hand-off | `On Released` | Raised once, right after the hold is released. |

- **Waits for:** start-up to finish (or fail), then one more frame, so the start scene group's first frame is ready.
- **Releases with:** `SceneActions.ReleaseLaunchScene`.
- **On destroy:** it also releases, so leaving Play never leaves the start scene group held.
- **Per object:** one receiver at most (Disallow Multiple Component).
- **Needs:** the Timeline package (`com.unity.timeline`).

**Called for you, leave alone**

- **OnNotify:** Timeline calls it when a marker fires.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| On Released | Raised once, right after the launch scene hold is released. | You add or change what the launch scene does at the hand-off (turn its camera and listener off, close itself). |

## In code

Each example is closed; open the one you need.

**Example: The method that sample 10 hooks to On Released**

The method that sample 10 hooks to `On Released`:

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
public void OnReleased()
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseReceiver -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
