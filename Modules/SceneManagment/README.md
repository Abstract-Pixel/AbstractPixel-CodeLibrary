# Scene messages

What `SceneEventBus` sends, in what order, and who listens in Project Rally Shift. Read from the code as of the
`scene-system-rework` branch, after session 2B. Go to a level, Retry, Preload and Switch to preloaded share one start step
and one finish step. Nothing in Rally Shift calls preload yet.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, go to a level, Retry, switch to preloaded. Not sent when a scene fails to load |
| `OnMainSceneLoaded` | the main scene | `DefaultSceneLoader`, after it activates a main scene and makes it the active scene. A held (preloaded) main scene fires it when the switch activates it |
| `OnSceneGroupUnloaded` | the group that **closed** (the one that was active when the change started) | Go to a level, Retry, switch to preloaded. Not sent when there was no previous group. Not sent when a preload replaces an earlier preload |
| `OnSceneGroupPreloaded` | the preloaded group | Preload, when it finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | Switch to preloaded, after `OnNewSceneGroupTransitionTo` |

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
4. `OnSceneGroupUnloaded` with the group that closed.
5. Open manager scenes, shared scenes, then the main scene. `OnMainSceneLoaded` fires as the main scene becomes active.
   A scene that throws is logged by name and stops the change.
6. Finish step (`EndSceneChange`): clear "busy", then record the open shared and manager scenes, make the group the active
   one, then `OnNewSceneGroupTransitionTo` once. A listener that reads `SceneActions.ActiveSceneGroup` sees the new group.
   After a failed load nothing is recorded and `OnNewSceneGroupTransitionTo` is not sent.

**Preload** (`PreloadSceneGroup`):
1. Nothing happens when the group is the open level or is already preloaded.
2. Start step, as above. A preload is refused while any scene change runs.
3. An earlier preload is closed first: its held main scene first, then its other scenes, then unused memory is released
   once. Replacing a preload counts as a scene change for the memory release.
4. Open the manager and shared scenes that are not open yet, then the main scene, held at 90% without activating it (no
   `OnMainSceneLoaded`). Only the main scene is held, because Unity holds every later scene operation while a scene
   waits. A shared scene that is already open is never opened a second time, even with `ForceReloadContextualScenes`.
5. `OnSceneGroupPreloaded`. A preload that fails closes what it opened and raises nothing.

**Switch to preloaded** (`TransitionToPreloadedSceneGroup`):
1. Start step, as above. Refused while any scene change runs.
2. Activate the held main scene. The loader makes it the active scene and raises `OnMainSceneLoaded`. This comes first
   because Unity holds the close and open steps below while the main scene waits.
3. Close the old main scene and the shared scenes that must reload (all open ones with `ForceReloadContextualScenes`,
   otherwise the ones the new group does not use), release unused memory once, `OnSceneGroupUnloaded` with the group
   that closed.
4. Open the shared scenes that are still missing. The ones the preload opened stay as they are.
5. Finish step (`EndSceneChange`): as for go to a level, and the preload record is cleared before the event. Then
   `OnNewSceneGroupTransitionTo` once, then `OnPreloadedSceneGroupActivated`.

Known limits of preload, on `.scratch/scene-coordinator/map.md`: go to a level while a preload is held stalls the change
(Unity holds the unloads), so nothing may call preload until ticket 04 settles that. A force-reloaded shared scene opens
after the level is active in a switch, not before.

## Who listens in Rally Shift

| Message | Listener | Purpose |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | `Modules/SaveSystem/Runtime/Core/SaveManager.cs` (library) | At the end of the frame, restores saved data into the new scenes. The first time, it also loads the save profile |
| | `GameplaySystems/Car/CarSubsystems/CarGhostRecorder/LevelGhostSaveLibrary.cs` | Writes ghosts to disk if a run changed them |
| | `ProjectServices/levelMusicPlayer/LevelMusicPlayer.cs` | Fades out the current level music |
| | `ProjectServices/PlatformServices/Achievements/Core/AchievementManager.cs` | Sends pending achievements to Steam |
| `OnMainSceneLoaded` | `Assets/SceneUtility/SceneToObjectChanger.cs` | Turns objects on or off for the scene that just became active. Also hears a preloaded switch now |
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
