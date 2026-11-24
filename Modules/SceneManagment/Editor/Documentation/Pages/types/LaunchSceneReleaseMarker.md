# LaunchSceneReleaseMarker

<!-- layout new -->

A Timeline marker for the launch scene's hand-off: the frame where the start scene group takes over.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseMarker -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneReleaseMarker.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

## Start here

Drop one marker on the launch scene's Timeline, at the frame where the hand-off should happen. A `LaunchSceneReleaseReceiver` (or the launch scene's own director) releases the launch scene hold there.

| You want to | Use | Why |
|---|---|---|
| Mark the hand-off frame | `LaunchSceneReleaseMarker` | In the Timeline's marker menu it is listed as `Launch scene/Release`. |
| Act on the marker | `LaunchSceneReleaseReceiver` | Put it next to the PlayableDirector; it releases the hold when the marker fires. |

- **Missed frames:** retroactive, so a long frame that jumps past the marker still fires it.
- **Fires:** once per play of the Timeline (trigger once).
- **Needs:** the Timeline package (`com.unity.timeline`); without it the class is not compiled.

**Called for you, leave alone**

- **id / flags:** Timeline reads these to send the notification; you never set them.

## In code

Each example is closed; open the one you need.

**Example: No code: add it to the launch scene's Timeline. A script that waits…**

No code: add it to the launch scene's Timeline. A script that waits for the hand-off it causes looks like this:

```cs
private void OnEnable()
{
    SceneActions.RunWhenLaunchSceneReleased(StartIntro);
}
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseMarker -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
