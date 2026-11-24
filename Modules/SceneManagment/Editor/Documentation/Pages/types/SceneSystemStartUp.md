# SceneSystemStartUp

<!-- layout new -->

Start-up: the first scene change of a session, run by the scene system itself when the game launches or Play is pressed.

<!-- type-header AbstractPixel.SceneManagement.SceneSystemStartUp -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/SceneSystemStartUp.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

You never call start-up. It reads the scene system settings, picks a scene group from the scenes Unity opened (`StartUpRule`), creates the coordinator and has it open the group's missing scenes.

| You want to | Use | Why |
|---|---|---|
| Know that start-up failed | `HasFailed` | True when start-up could not open its scene group, or had nothing it could do. |
| Change which group start-up picks | `StartUpRule` | The rule start-up follows. |

**Called for you, leave alone**

- **Before start-up:** scene changes asked for before start-up are refused.
- **Domain reload off:** state resets through `StaticsResetter`, so it runs on every Play.
- **Launch scene hold:** start-up holds the launch scene when it is among the scenes Unity is opening, until it calls `SceneActions.ReleaseLaunchScene`.

## In code

Each example is closed; open the one you need.

**Example: It runs by itself. Check whether it failed, or wait for it to finish**

```cs
if (SceneSystemStartUp.HasFailed)
{
    Debug.LogError("Start-up could not open the start scene group.");
}

SceneEventBus.OnGameStarted += OnGameStarted;
```

<!-- members AbstractPixel.SceneManagement.SceneSystemStartUp -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
