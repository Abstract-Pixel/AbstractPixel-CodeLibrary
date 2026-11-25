# Change scene <!-- level 1 -->

<!-- layout new -->

Go to another [scene group](term:scene-group) from a button or from code. One call runs the screen effect and the loading scene, then tells you how the change ended.

## Change scene from a button <!-- level 0 -->

Point a button's On Click at `MonoSceneActions.ChangeTo` and give it the scene group. No code.

**Try it in the sample**

<!-- action open-sample 01 "Open sample 01" -->

1. Press Play. <!-- done-when OnGameStarted -->
2. Click "Go to Room B". <!-- done-when screen:Hidden -->
3. Wait for Room B. <!-- done-when OnNewSceneGroupTransitionTo -->

**Or do it yourself**

1. Add a `MonoSceneActions` component to any object in the scene.
2. On the button's On Click, click +, drag that object in and pick MonoSceneActions > ChangeTo.
3. Drag the scene group you want to go to into the argument field.

> [!TIP]
> The screen fades, the new group opens, and the fade shows it.

<details><summary>If it goes wrong</summary>

- **The click does nothing:** look in the Console for a Refused [scene change result](term:scene-change-result), such as Busy.
- **No fade:** the manager scene holds no screen effect. Add one, or keep the instant cut.

</details>

## Change scene from code <!-- level 1 -->

Call `SceneActions.ChangeTo(group)` and await the `SceneChangeResult` it returns. It never throws.

**Try it in the sample**

<!-- action open-sample 02 "Open sample 02" -->

**Or do it yourself**

Await the result and react to it:

```cs source=lib:Modules/SceneManagement/Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
public async Task<SceneChangeResult> ChangeToOther()
{
    SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
    ShowResults("Change to " + otherGroupName + ": " + result);
    return result;
}
```

> [!TIP]
> "Last result: Change to Arena: Done" on the sample's label.

## Read the result <!-- level 1 -->

A [scene change result](term:scene-change-result) is Done, Refused with a reason, or Failed with the scene that failed. A refusal happens before anything touches the screen.

**Why a change is refused**

| Reason | When |
|---|---|
| `Busy` | Another [scene change](term:scene-change) is running. Only one runs at a time. |
| `StartUpRunning` | [Start-up](term:start-up) is still opening the first scene group |
| `AlreadyOpen` | The group asked for is the one already open |
| `NoGroupGiven` | The group was null |
| `NothingOpenToRetry` | Retry was asked for while no group is open |

<details><summary>If it goes wrong</summary>

- **Refused: AlreadyOpen:** use Retry to start the open group again.
- **Refused: StartUpRunning:** wait for `OnGameStarted`, or check `SceneActions.HasGameStarted`.
- **Failed:** see [When a scene change fails](when-a-scene-change-fails.md).

</details>

## Retry the open scene group <!-- level 1 -->

Call `SceneActions.Retry()` to close and reopen the group that is open, so the place starts again.

**Try it in the sample**

<!-- action open-sample 03 "Open sample 03" -->

**Or do it yourself**

```cs source=lib:Modules/SceneManagement/Samples/03 Fast Restart/Scripts/Sample03RetryButton.cs
public async Task<SceneChangeResult> RetryOnce()
{
    SceneChangeResult result = await SceneActions.Retry();
    ShowResults("Retry: " + result);
    return result;
}
```

> [!NOTE]
> [Reset shared scenes](term:reset-shared-scenes) decides whether the [shared scenes](term:shared-scene) close and reopen too.

<details><summary>Why it works</summary>

- **One call:** holds the whole change, screen effect included ([design decision 1](../../../../docs/adr/0001-scene-system-calls-transitions-through-one-interface.md)).
- **Busy:** set before the first await, so a second press in the same frame is refused, not run twice.

</details>
