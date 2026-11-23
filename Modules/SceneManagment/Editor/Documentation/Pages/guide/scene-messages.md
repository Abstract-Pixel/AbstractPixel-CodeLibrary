# Scene messages <!-- level 2 -->

Listen to `SceneEventBus` and `TransitionEventBus` to react to [scene changes](term:scene-change). Each message fires at one fixed moment.

## The messages <!-- level 2 -->

Subscribe in `OnEnable` and unsubscribe in `OnDisable`.

### OnNewSceneGroupTransitionTo

Fires once a scene change is done: every scene of the new group is open and it is the active group. Not sent for a group whose scene failed.

### OnGameStarted

Fires once per session, right after start-up's `OnNewSceneGroupTransitionTo`.

### OnLaunchSceneReleased

Fires once, when `SceneActions.ReleaseLaunchScene()` ends the [launch scene hold](term:launch-scene-hold). Never sent when nothing was held.

### OnMainSceneLoaded

Fires when the loader activates a main scene and makes it Unity's active scene.

### OnSceneGroupUnloaded

Fires with the group that closed, during a change, Retry or a switch to a preloaded group.

### OnSceneGroupPreloaded

Fires when a [preload](term:preload) finished.

### OnPreloadedSceneGroupActivated

Fires on a change to the preloaded group, after `OnNewSceneGroupTransitionTo`.

### OnSceneChangeFailed

Fires once per failed change, with the group asked for and the scene that failed. Not sent for a failed preload.

### OnScreenStateChanged

`TransitionEventBus.OnScreenStateChanged` sends the new [screen state](term:screen-state): Visible, Hiding, Hidden, Showing, Visible.

## Order in a scene change <!-- level 2 -->

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

## Order at start-up <!-- level 2 -->

Start-up never uses the screen effect or a loading scene. It holds every relocatable, opens the group's missing scenes, relocates, sends `OnNewSceneGroupTransitionTo` once and `OnGameStarted` once, then releases.

> [!NOTE]
> Only the scene system raises scene messages: the `Raise` methods are internal.

<details><summary>If it goes wrong</summary>

- A listener never fires → it subscribed after the message; read `SceneActions.ActiveSceneGroup` or `HasGameStarted` instead.
- A listener fires twice after a domain reload → unsubscribe in `OnDisable`.

</details>

## Details <!-- level 3 -->

The full table and order, step by step.

### All messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, a change, Retry, a switch to a preloaded group, the fallback scene group. Not sent for a group whose scene failed |
| `OnGameStarted` | the first group | Start-up only, once per session, right after its `OnNewSceneGroupTransitionTo` |
| `OnLaunchSceneReleased` | none | `SceneActions.ReleaseLaunchScene()`, once, when the launch scene hands over. Never sent when nothing was held |
| `OnMainSceneLoaded` | the main scene | The loader, after it activates a main scene and makes it the active scene |
| `OnSceneGroupUnloaded` | the group that **closed** | A change, Retry, a switch to a preloaded group. Not sent when there was no previous group |
| `OnSceneGroupPreloaded` | the preloaded group | Preload, when it finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | A change to the preloaded group, after `OnNewSceneGroupTransitionTo` |
| `OnSceneChangeFailed` | the group asked for, the scene that failed | A change, Retry or the fallback, once per failed change (4C, ADR 0011). Not sent for a failed preload |
| `TransitionEventBus.OnScreenStateChanged` | the new `ScreenState` | The transitions module: Visible → Hiding → Hidden → Showing → Visible |
| `OnStartHideScreenTransition`, `OnEndHideScreenTransition` | none | Hide screen starts / the screen counts as covered (was In started / In completed) |
| `OnStartShowScreenTransition`, `OnEndShowScreenTransition` | none | Show screen starts / ends (was Out started / Out completed) |

`SceneEventBus`'s `Raise*` methods and `ResetStatics` are internal: only the scene system raises scene messages.
`TransitionActions.ScreenState` is read-only and resets with domain reload off. Neither module reads or writes
`Time.timeScale`: game management owns game time and reacts to the screen state (ADR 0010).

### Order, per entry point in full

**Start-up** (`SceneSystemStartUp`, `[RuntimeInitializeOnLoadMethod]`): scene changes are refused (`StartUpRunning`) from
before the first scene loads until start-up ends. It never uses the screen effect or a loading scene.
0. Before any scene's Awake (BeforeSceneLoad) it holds the **launch scene hold** when the settings name a launch scene and
   Unity is opening it (`StartUpRule.ShouldHoldLaunchScene`, `StartUpRule.CountsAsOpening`). The launch scene releases
   it with `SceneActions.ReleaseLaunchScene()`: waiting actions run, `OnLaunchSceneReleased` once, one log line.
1. After the first scenes load it reads the settings and picks a group from the scenes Unity opened (`StartUpRule`).
2. It creates the coordinator if no scene holds one.
3. **Hold** every relocatable, then ask for every missing scene of the group at once and in order (main scene, manager
   scenes, shared scenes).
4. Records the group and **Relocates** (a failed load relocates too, so objects with no position stay held), then
   `OnNewSceneGroupTransitionTo` once, `OnGameStarted` once, and **Release** when start-up ends, or when the launch
   scene hold ends if it still holds. A failed load raises neither;
   `SceneActions.HasStartUpFailed` turns true (no fallback for start-up).

**A change** (`ChangeTo`) and **Retry** (`Retry`) share one body. A `SceneChangePlan` works out which scenes to close and open.
1. Refusals first, each before anything touches the screen: no group (`NoGroupGiven`), the group asked for is the open
   asset (`AlreadyOpen`; groups are equal only to themselves), nothing open to Retry (`NothingOpenToRetry`), start-up
   running, another change running (`Busy`). Busy is set before the first `await` and stays set until Show screen ends.
2. **Hold** every relocatable. **Hide screen** through the registered `ISceneTransitionExecutor` (none registered: nothing). A Hide screen asked for
   while an effect still runs waits for it, so the screen is always covered.
3. A preload this change does not use is closed (held main scene first). Unity holds every later scene operation while a
   preloaded main scene waits. A change to the preloaded group itself runs as the preloaded switch in step 5.
4. **Loading scene**, when the group shows one (next section): it opens under the cover and Show screen uncovers it.
5. Close the old main scene and the shared scenes (all with Reset shared scenes), free memory once,
   `OnSceneGroupUnloaded`, open manager scenes, shared scenes, then the main scene (`OnMainSceneLoaded`). Loading
   progress fills. A switch to a preloaded group activates the held main scene first, then closes and opens the rest.
6. Done: record the open scenes, **Relocate**, `OnNewSceneGroupTransitionTo` once (then `OnPreloadedSceneGroupActivated` for a switch).
   Failed: record what is really open (no group counts as open), `OnSceneChangeFailed` once, then the **fallback scene
   group** (settings: "If a scene change fails, go to"; empty means the start scene group) runs steps 5 and 6 once,
   with the screen still covered and the loading scene still up. If the fallback fails too, or the failed change was
   the fallback, the screen is shown on whatever is open and an error is logged. A failed load is never tried again. A
   scene that fails to close is logged and the change carries on.
7. The loading scene leaves (minimum time, "full" animation, Hide screen, it closes).
   Relocate runs once per change: a failed change relocates on whatever is open.
8. **Show screen**, then busy clears. **Release** comes once, when the screen counts as visible (the transition profile's
   `ScreenCountsAsVisibleAt`, 0.75 by default), or when Show screen ends if it never reports; while the launch scene hold
   is on, it waits for the hold to end. The result says Done, or Failed with the scene.

**Preload** (`Preload`): refused for no group, the open group, start-up or a running change; Done at once when the group is
already preloaded. It never touches the screen and never shows a loading scene. An earlier preload closes first (memory
freed once), then the missing manager and shared scenes open and the main scene is held at 90%. `OnSceneGroupPreloaded`.
A failed preload closes what it opened, returns Failed and goes nowhere. The scene system never preloads by itself: a
change uses a preload only when the game preloaded that same group.

The coordinator makes no scene load or unload call of its own: every open and close goes through `ISceneLoader`, whose
held-scene contract is written on the interface and tested with the recording loader in `Tests/Editor`. A loader fails a
load it cannot do (a scene missing from the build, an Addressable scene without a GUID) by throwing; it never logs and
returns. `ISceneLoader.GetLoadProgress` reports a scene's own progress for the loading progress.
