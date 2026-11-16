# Scene messages

What `SceneEventBus` sends, in what order, and who listens in Project Rally Shift. Read from the code as of the
`scene-system-rework` branch, after session 2A. Go to a level and Retry share one start step and one finish step. Items
marked **B4**, **B5** and **B10** remain in the preload route. Session 2B fixes them.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, go to a level, Retry. Not sent when a scene fails to load |
| `OnMainSceneLoaded` | the main scene | `DefaultSceneLoader`, after it activates a main scene and makes it the active scene |
| `OnSceneGroupUnloaded` | the **new** group, not the one that closed | Go to a level, Retry, switch to preloaded |
| `OnSceneGroupPreloaded` | the preloaded group | Preload |
| `OnPreloadedSceneGroupActivated` | the preloaded group | Switch to preloaded |

## Order, per entry point

The fade wraps every entry point that goes through `TransitionActions`: `OnTransitionInStarted`, `OnTransitionInCompleted`,
then the scene work below, then `OnTransitionOutStarted` and `OnTransitionOutCompleted`. These are `TransitionEventBus`
messages from the fade system, not scene messages.

**Start-up** (`SceneBootstrapper` calls `SceneActions.InitializeStartSceneData`): the bootstrapper starts loading its
missing boot scenes without waiting for them, then the coordinator records the group and sends
`OnNewSceneGroupTransitionTo`. The boot scenes may still be loading at that moment.

**Go to a level** (`TransitionToSceneGroup`) and **Retry** (`ReloadActiveSceneGroup`) share the same steps. A
`SceneChangePlan` works out first which scenes to close and open. Retry is "go to the group that is already open".
1. Start step (`TryBeginSceneChange`): if a scene change is already running, log a warning and refuse. Otherwise set
   "busy" (`IsLoadingSceneGroup` and `IsUnloadingSceneGroup`) before the first `await`, so a second request in the same
   frame is refused.
2. Close the old main scene, then the shared scenes (all of them with `ForceReloadContextualScenes`, otherwise only the
   ones the new group does not use). Manager scenes stay open.
3. Release unused memory once.
4. `OnSceneGroupUnloaded`.
5. Open manager scenes, shared scenes, then the main scene. `OnMainSceneLoaded` fires as the main scene becomes active.
   A scene that throws is logged by name and stops the change.
6. Finish step (`EndSceneChange`): clear "busy", then record the open shared and manager scenes, make the group the active
   one, then `OnNewSceneGroupTransitionTo` once. A listener that reads `SceneActions.ActiveSceneGroup` sees the new group.
   After a failed load nothing is recorded and `OnNewSceneGroupTransitionTo` is not sent.

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

Go to a level and Retry raise "new level" at the same point, once, after everything has loaded. Switch to preloaded
still raises a different message (**B10**).

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
