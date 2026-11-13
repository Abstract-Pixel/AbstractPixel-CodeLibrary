# Changelog

## [Unreleased] - Save System v2 (branch save-system)
**Save System v2.** Not compatible with v1 save files or code; shipped games stay on the `save-system-v1` tag. Full guide: `Documentation~/save-system.html`. What changed from v1: `Documentation~/save-system-v2-handoff.html`.
- **Feature:** No SaveManager in the scene. The save system starts before the first scene from `Assets/Resources/SaveSystemConfig.asset`, made by Tools > External Project Tools > Save System > Create Config.
- **Feature:** Saved data is restored after every `Awake` and before any `Start`, for objects that register at any time. Plain C# classes and ScriptableObjects can save with `SaveActions.Register`.
- **Feature:** Save files are classes from the config's save file list (`SaveFiles.cs`, written by the editor). `ISavable<TData, TFile>` picks one; library modules ship their own with `[SaveFileDefaults]`.
- **Feature:** `SaveActions.Save<TFile>()`, `Reload<TFile>()`, `Forget(object)`. Three save locations: Global, GameProfile and Scene.
- **Feature:** `GameProfile` handles with slot numbers: `SaveActions.GameProfiles.CreateNew()`, `Get(slot)`, `All`, `Current`, `profile.Open()`, `Delete()`, `Rename()`.
- **Feature:** Sticky load events on `SaveEventBus`: `OnLoadCompleted(Type)`, `OnLoadAllCompleted`, `OnGameProfileOpened`; plus `OnSaveCompleted(Type)` and `OnSaveAllCompleted`.
- **Feature:** `SaveStorage` reads and writes a whole save file as text in one line, in play mode and edit mode.
- **Feature:** Atomic writes. Optional backups per save file, in a `Backups` folder that mirrors the main one.
- **Fix:** Listing and deleting game profiles works; "last opened" is remembered; Delete All Save Files deletes the right folders; saving no longer erases data of objects that aren't loaded.
- **Settings:** Saves into its own `SettingsSaveFile`; the debug toolbar uses `SaveStorage`.
- **Core:** `SearchableDropdown` compiles on Unity 6 and Unity 7.

## [1.1.0] - 2026-10-31
**GameManagment: delayed menus, eased time scale, skip, state fixes.** Every new option defaults to the old behaviour.
- **Feature:** `StateSO.OverrideExecutionTimeScale` + `ExecutionTimeScale` (0 freeze, 0.2 slow motion, 1 no freeze), `TimeScaleEaseDuration` and `TimeScaleEaseCurve`. Easing runs on realtime in `GameStateComponent`; defaults (off / 0) keep the instant `IsTimeZeroOnExecution` behaviour.
- **Feature:** `GameStateRegistry.OnStateConfigurationsApplied` and `AreConfigurationsApplied(state)`: raised once a state has waited its `StateExecutionDelay` and applied its configurations.
- **Feature:** `GameStateMenu.revealTiming` (`MenuRevealTiming.OnStateRegistered` default, or `OnConfigurationsApplied` to show the menu only after the delay).
- **Feature:** `GameStateComponent.SkipExecutionDelay()` ends the delay early (applies once). `IsActive` / `StateConfig` getters.
- **Feature:** `GameStateComponent.reactivateOnActiveSceneChange` (default off): re-activates the state one frame after an active-scene change if it is inactive.
- **Feature:** `AutoSelectUIOnEnable.waitUntilInteractable` (default off): select only once the selectable is interactable, e.g. after an entrance animation.
- **Fix:** an evicted state's pending delayed apply / time-scale ease is stopped, so it can no longer apply after it left.
- **Fix:** `GameStateEventTrigger.OnDisable` now also unsubscribes `OnStateUnregistered` (destroyed triggers stayed subscribed).
- **Fix (GhostSystem):** `GhostPlayer` plays back in `Update` on game time (like `GhostRecorder`) instead of `FixedUpdate`, and `SimpleProximityVisualFader` fades in `LateUpdate`. Stepping on physics ticks made the ghost judder next to an interpolated player car.

## [1.0.3] - 2026-04-13
**Scene Management DataTypes and Tooling**
- **Feature:** Introduced `SceneField`, a serializable utility to reference Unity scenes safely by name while maintaining a link to the `SceneAsset` in the editor. That can work in runtime foer builds
- **Feature:** Added `SceneReference`, a hybrid container that supports both traditional Build Settings-based scenes (`SceneField`) and Addressable Assets (`AssetReference`).
- **UI/UX:** Implemented a custom `SceneFieldPropertyDrawer` featuring a visual validation indicator (green/red status box) to instantly show if a referenced scene is included in the project's Build Settings.
- **Architecture:** Updated `AbstractPixel.Core` assembly definitions to include `com.unity.addressables` as a reference, enabling support for the Addressable Asset System in scene management.
- **Tooling:** Automated name synchronization in the `SceneField` inspector, ensuring the string-based scene name updates whenever a new `SceneAsset` is assigned.

## [1.0.2] - 2026-03-04
**Stability, Dependency Management, and Documentation**
- **SaveSystem:** Added `com.unity.nuget.newtonsoft-json` (v3.2.2) as an explicit dependency in `package.json` to support JSON serialization in the SaveSystem module.
- **Architecture:** Disabled "Use GUIDs" in specific Assembly Definitions to prevent potential reference breakage during compilation in restricted environments.
- **Maintenance:** Shifted all historical changelog data from `README.md` to this dedicated `CHANGELOG.md` file.
- **Automation:** Added additional validation checks to `SavableBridgeAutomation.cs` to prevent unnecessary errors.
- **Error Handling:** Implemented explicit, non-crashing error messages in `SavableBridgeAutomation.cs` to help identify configuration issues quickly.
- **Fix:** Created a new default `Debug Data Base` ScriptableObject asset to prevent `DebugManagerRefresher` initialization errors.

## [1.0.1] - 2026-03-03
**Cleanup and Standardization**
- **Refactor:** Added Namespaces (`AbstractPixel.Core`) to all legacy utility scripts to prevent naming collisions.
- **Maintenance:** Deleted `LazySingleton`. It was redundant as `PersistentSingleton` provides a more robust solution.
- **Architecture:** Regenerated all `.meta` files (GUIDs) for transferred scripts to ensure unique identity.
- **Fix:** Corrected naming conventions and fixed typos within `SaveSystem` package folders (e.g., `DataManagement`).

## [1.0.0] - 2026-03-02
**Initial Release**
- **Core:** Initial transfer of base utilities: Object Pooling, Timer Systems, and Custom Attributes.
- **Feature:** Implemented `SaveSystem` module: Serialization wrappers, File System management, and Public API.
- **Structure:** Established Assembly Definition boundaries for Runtime/Editor code isolation.