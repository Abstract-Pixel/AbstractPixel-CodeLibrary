# Scene messages

What `SceneEventBus` sends, in what order, and who listens in Project Rally Shift. Read from the code as of the
`scene-system-rework` branch, session 1. Items marked **B4**, **B5**, **B6**, **B10** and **B13** are known
inconsistencies. Session 2 of the scene system plan fixes them.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, go to a level, Retry |
| `OnMainSceneLoaded` | the main scene | `DefaultSceneLoader`, after it activates a main scene and makes it the active scene |
| `OnSceneGroupUnloaded` | the **new** group, not the one that closed | Go to a level, switch to preloaded |
| `OnSceneGroupPreloaded` | the preloaded group | Preload |
| `OnPreloadedSceneGroupActivated` | the preloaded group | Switch to preloaded |

## Order, per entry point

The fade wraps every entry point that goes through `TransitionActions`: `OnTransitionInStarted`, `OnTransitionInCompleted`,
then the scene work below, then `OnTransitionOutStarted` and `OnTransitionOutCompleted`. These are `TransitionEventBus`
messages from the fade system, not scene messages.

**Start-up** (`SceneBootstrapper` calls `SceneActions.InitializeStartSceneData`): the bootstrapper starts loading its
missing boot scenes without waiting for them, then the coordinator records the group and sends
`OnNewSceneGroupTransitionTo`. The boot scenes may still be loading at that moment.

**Go to a level** (`TransitionToSceneGroup`):
1. Close the old main scene, then the shared scenes. The "busy" flag is set only after the old main scene has closed, so a
   second request in that window starts a second change (**B5**).
2. Release unused memory once.
3. `OnSceneGroupUnloaded`.
4. Open manager scenes, shared scenes, then the main scene. `OnMainSceneLoaded` fires as the main scene becomes active.
5. `OnNewSceneGroupTransitionTo`. It fires before `ActiveSceneGroup` is updated, so a listener that reads
   `SceneActions.ActiveSceneGroup` sees the old group (**B13**). Load errors are logged and swallowed, so it also fires
   after a failed load (**B6**).

**Preload** (`PreloadSceneGroup`): closes any earlier preloaded scenes, opens the group's scenes held at 90% without
activating them (no `OnMainSceneLoaded`), then `OnSceneGroupPreloaded`. Nothing in Rally Shift preloads today, but
`RallyLevelTranistioner` switches to a preloaded group when one exists.

**Switch to preloaded** (`TransitionToPreloadedSceneGroup`):
1. No "busy" check (**B5**).
2. Activate the held scenes. The loader's shortcut for held scenes returns early, so the level is **not** made the active
   scene and `OnMainSceneLoaded` does not fire (**B4**).
3. Close the old main and shared scenes, release unused memory once, `OnSceneGroupUnloaded`.
4. `OnPreloadedSceneGroupActivated` instead of `OnNewSceneGroupTransitionTo`, so its listeners never hear of the new
   level (**B10**).

**Retry** (`ReloadActiveSceneGroup`): sets "busy" first. Closes the main scene, then the shared scenes if the group has
`ForceReloadContextualScenes`. Releases unused memory once. Reopens the shared scenes, then the main scene
(`OnMainSceneLoaded`). Then `OnNewSceneGroupTransitionTo`, even if a step threw (**B6**). No `OnSceneGroupUnloaded`.
Manager scenes stay open.

So "new level" arrives at a different point in each entry point, or not at all (**B10**).

## Who listens in Rally Shift

| Message | Listener | Purpose |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | `Modules/SaveSystem/Runtime/Core/SaveManager.cs` (library) | At the end of the frame, restores saved data into the new scenes. The first time, it also loads the save profile |
| | `GameplaySystems/Car/CarSubsystems/CarGhostRecorder/LevelGhostSaveLibrary.cs` | Writes ghosts to disk if a run changed them |
| | `ProjectServices/levelMusicPlayer/LevelMusicPlayer.cs` | Fades out the current level music |
| | `ProjectServices/PlatformServices/Achievements/Core/AchievementManager.cs` | Sends pending achievements to Steam |
| `OnMainSceneLoaded` | `Assets/SceneUtility/SceneToObjectChanger.cs` | Turns objects on or off for the scene that just became active. Misses a preloaded switch (**B4**) |
| `OnSceneGroupUnloaded`, `OnSceneGroupPreloaded`, `OnPreloadedSceneGroupActivated` | none | |

Rally paths are under `Assets/[1]Project/` unless shown otherwise. Many other systems read
`SceneActions.ActiveSceneGroup` directly (win screen, leaderboards, ghosts, level finish, `RallyLevelManager`) instead of
listening.

## Fade messages used for placement

| Message | Listener | Purpose |
|---|---|---|
| `TransitionEventBus.OnTransitionOutStarted` | `GameplaySystems/Car/CarCore/CarScripts/CarBootPhysicsToggle.cs` | Unlocks the car (it was frozen while the screen was covered), turns on its safety recorder, and sets `CarCanMove` |
| | `SceneTransitionSystem/.../FallbackAudioListener.cs`, `TransitionAudio.cs` (library) | Hands audio back to the scene's listener, plays the reveal sound |
| `TransitionEventBus.OnTransitionOutCompleted` | `CarBootPhysicsToggle.cs` | Same handler, as a safety net. It runs once |

With Play pressed inside a level there is no fade. `CarBootPhysicsToggle.Start` unlocks the car instead, in the Editor
only.
