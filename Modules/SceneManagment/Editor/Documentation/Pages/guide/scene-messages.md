# Scene messages <!-- level 2 -->

<!-- layout new -->

Listen to `SceneEventBus` and `TransitionEventBus` to react to [scene changes](term:scene-change). Each message fires at one fixed moment.

## The messages <!-- level 2 -->

Subscribe in `OnEnable` and unsubscribe in `OnDisable`. Most games need only the first two.

| Message | Argument | When it fires |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | A change is done: every scene of the new group is open and it is the active group. Sent by start-up, a change, Retry, a switch to a preloaded group and the fallback scene group. Not sent for a group whose scene failed. |
| `OnGameStarted` | the first group | Once per session, from start-up only, right after its `OnNewSceneGroupTransitionTo` |
| `OnLaunchSceneReleased` | none | Once, when `SceneActions.ReleaseLaunchScene()` ends the [launch scene hold](term:launch-scene-hold). Never sent when nothing was held. |
| `OnMainSceneLoaded` | the main scene | The loader activated a main scene and made it Unity's active scene |
| `OnSceneGroupUnloaded` | the group that **closed** | During a change, Retry or a switch to a preloaded group. Not sent when there was no previous group. |
| `OnSceneGroupPreloaded` | the preloaded group | A [preload](term:preload) finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | A change to the preloaded group, after `OnNewSceneGroupTransitionTo` |
| `OnSceneChangeFailed` | the group asked for, the scene that failed | Once per failed change, Retry or fallback ([design decision 11](../../../../docs/adr/0011-a-failed-scene-change-goes-to-the-fallback-scene-group.md)). Not sent for a failed preload. |
| **On `TransitionEventBus`** | | |
| `OnScreenStateChanged` | the new `ScreenState` | The [screen state](term:screen-state) changed: Visible, Hiding, Hidden, Showing, Visible |
| `OnStartHideScreenTransition`, `OnEndHideScreenTransition` | none | Hide screen starts / the screen counts as covered (were In started / In completed) |
| `OnStartShowScreenTransition`, `OnEndShowScreenTransition` | none | Show screen starts / ends (were Out started / Out completed) |

> [!NOTE]
> Only the scene system raises scene messages: `SceneEventBus`'s `Raise*` methods and `ResetStatics` are internal.

<details><summary>More: screen state and game time</summary>

- **Reading the state:** `TransitionActions.ScreenState` is read-only, and resets with domain reload off.
- **Game time:** neither module reads or writes `Time.timeScale`. Game management owns game time and reacts to the screen state ([design decision 10](../../../../docs/adr/0010-game-management-owns-time-and-transitions-report-screen-states.md)).

</details>

<details><summary>If it goes wrong</summary>

- **A listener never fires:** it subscribed after the message. Read `SceneActions.ActiveSceneGroup` or `HasGameStarted` instead.
- **A listener fires twice after a domain reload:** unsubscribe in `OnDisable`.

</details>

## Order in a scene change <!-- level 2 -->

`ChangeTo` and `Retry` share one body; a `SceneChangePlan` works out which scenes to close and open.

1. Refusals, before anything touches the screen.
2. [Hold](term:hold), then Hide screen.
3. The loading scene, when the group shows one.
4. Close the old scenes, free memory once, `OnSceneGroupUnloaded`.
5. Open manager scenes, shared scenes, then the main scene (`OnMainSceneLoaded`).
6. [Relocate](term:relocate), then `OnNewSceneGroupTransitionTo` once.
7. The loading scene leaves; Show screen; busy clears; [Release](term:release) when the screen counts as visible.

**Try it in the sample**

<!-- action open-sample 02 "Open sample 02" -->

<!-- timeline -->

## Each step of a change, in full <!-- level 3 -->

What the seven steps above do, grouped by what the player sees.

1. **Refusals**
   1. No group: `NoGroupGiven`.
   2. The group asked for is the open asset: `AlreadyOpen`. Groups are equal only to themselves.
   3. Nothing open to Retry: `NothingOpenToRetry`.
   4. Start-up running: `StartUpRunning`. Another change running: `Busy`, set before the first `await` and kept until Show screen ends.
2. **Cover the screen**
   1. Hold every relocatable.
   2. Hide screen through the registered `ISceneTransitionExecutor`; with none registered, nothing happens. A Hide screen asked for while an effect still runs waits for it, so the screen is always covered.
   3. A preload this change does not use is closed, held main scene first. Unity holds every later scene operation while a preloaded main scene waits.
   4. The loading scene, when the group shows one: it opens under the cover and Show screen uncovers it.
3. **Swap the scenes**
   1. Close the old main scene and the shared scenes (all of them with Reset shared scenes), free memory once, `OnSceneGroupUnloaded`.
   2. Open manager scenes, shared scenes, then the main scene (`OnMainSceneLoaded`). Loading progress fills.
   3. A switch to a preloaded group activates the held main scene first, then closes and opens the rest.
   4. Done: record the open scenes, Relocate, `OnNewSceneGroupTransitionTo` once (then `OnPreloadedSceneGroupActivated` for a switch).
4. **Uncover the screen**
   1. The loading scene leaves: minimum time, "full" animation, Hide screen, it closes.
   2. Show screen, then busy clears.
   3. Release comes once, when the screen counts as visible (the profile's `ScreenCountsAsVisibleAt`, 0.75 by default), or when Show screen ends if it never reports. While the launch scene hold is on, it waits for the hold to end.
   4. The result says Done, or Failed with the scene.

<details><summary>More: when the change fails</summary>

- **Record:** what is really open (no group counts as open), then `OnSceneChangeFailed` once.
- **Fallback:** the [fallback scene group](term:fallback-scene-group) (Start-up's "If a scene change fails, go to"; empty means the start scene group) runs the open steps once, with the screen still covered and the loading scene still up.
- **Fallback fails too:** or the failed change was the fallback; the screen is shown on whatever is open and an error is logged.
- **No retries:** a failed load is never tried again. A scene that fails to close is logged and the change carries on.
- **Relocate runs once per change:** a failed change relocates on whatever is open.

</details>

## Order at start-up <!-- level 2 -->

Start-up never uses the screen effect or a loading scene. Scene changes are refused with `StartUpRunning` from before the first scene loads until start-up ends.

1. **Before any scene's Awake** (BeforeSceneLoad), `SceneSystemStartUp` holds the launch scene hold when the settings name a launch scene and Unity is opening it (`StartUpRule.ShouldHoldLaunchScene`, `StartUpRule.CountsAsOpening`).
2. **After the first scenes load,** it reads the settings and picks a group from the scenes Unity opened (`StartUpRule`), and creates the coordinator if no scene holds one.
3. **Hold** every relocatable, then ask for every missing scene of the group at once and in order: main scene, manager scenes, shared scenes.
4. **Record** the group and Relocate. A failed load relocates too, so objects with no position stay held.
5. **Announce:** `OnNewSceneGroupTransitionTo` once, `OnGameStarted` once, then Release when start-up ends, or when the launch scene hold ends if it still holds.

- **Releasing the launch scene:** `SceneActions.ReleaseLaunchScene()` runs the waiting actions, sends `OnLaunchSceneReleased` once and logs one line.
- **A failed load:** raises neither message; `SceneActions.HasStartUpFailed` turns true. Start-up has no fallback.

## Order of a preload <!-- level 2 -->

`Preload` never touches the screen and never shows a loading scene. The scene system never preloads by itself: a change uses a preload only when the game preloaded that same group.

1. Refused for no group, the open group, start-up or a running change. Done at once when the group is already preloaded.
2. An earlier preload closes first; memory is freed once.
3. The missing manager and shared scenes open, and the main scene is held at 90%.
4. `OnSceneGroupPreloaded`.

- **A failed preload:** closes what it opened, returns Failed and goes nowhere.

## The loader's part <!-- level 3 -->

The coordinator makes no scene load or unload call of its own: every open and close goes through `ISceneLoader`.

- **Held-scene contract:** written on the interface and tested with the recording loader in `Tests/Editor`.
- **Failing a load:** a loader fails a load it cannot do (a scene missing from the build, an Addressable scene without a GUID) by throwing. It never logs and returns.
- **Progress:** `ISceneLoader.GetLoadProgress` reports a scene's own progress for the loading progress.
