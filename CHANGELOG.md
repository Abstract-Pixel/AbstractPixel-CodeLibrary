# Changelog

## [Unreleased] - Save System v2 (branch save-system)
**Save System v2.** It can't read v1 save files. Shipped games stay on the `save-system-v1` tag. Guide: `Documentation~/save-system.html`. Changes since v1: `Documentation~/save-system-v1-to-v2.html`.
- **Feature:** No manager in the scene. Make the config with Tools > External Project Tools > Save System > Create Config.
- **Feature:** Scripts get saved data before `Start`. Plain C# classes and ScriptableObjects can save.
- **Feature:** Save files are classes. Five built-in ones ship with the library (`SaveFiles.GameProfile`, `Scene`, `Settings`, `Input`, `Achievements`), and your own rows are written into `GameSaveFiles.cs`. Pick one with `ISavable<TData, TFile>`.
- **Feature:** `SaveActions.Register(this)` needs no save ID. Later instances of a class get "Inventory 2", "Inventory 3".
- **Feature:** `SaveActions.Save<TFile>()`, `Reload<TFile>()` and `Forget(object)`. Forget on a scene object writes a gone marker, so it stays gone. Three save locations: Global, GameProfile and Scene.
- **Feature:** A new game profile gives objects that stayed alive their new-game values.
- **Feature:** `GameProfile` objects with slot numbers, such as `SaveActions.GameProfiles.Get(2).Open()`. Custom fields on `profile.Custom` stay until the next save.
- **Feature:** Load and save events on `SaveEventBus`. Load events reach late subscribers too. `OnSaveCompleted` fires only once a file is on disk.
- **Feature:** `SaveStorage` reads, writes or deletes a whole save file in one line, for the open game profile by default, with a `SceneField` for Scene files.
- **Feature:** Save file rows that share a file or a class name warn in the config. Two objects with the same save ID log one error naming both.
- **Feature:** Safe writes, and an optional backup per save file in a `Backups` folder.
- **Feature:** `[Save]` on a field saves it with no other code, on a MonoBehaviour, a plain class (plus `SaveActions.Register(this)`) or a ScriptableObject. `[Save(Formerly = "hp")]` and `[SaveId("Inventory")]` keep data through renames. `ISaveIn<TFile>` picks the save file. A class switched from `ISavable<int>` to one `[Save]` field keeps its saved value.
- **Feature:** Mistakes the editor can see, such as `[Save]` on a GameObject field, log one error naming the fix, block play mode and fail the build.
- **Feature:** `IOnRestored`, `SetSaveId` for spawned copies, typed load listeners (`SaveEventBus.AddLoadListener<TFile>`), `SaveEventBus.OnSaveFailed`, and `SaveActions.GameProfiles.LastOpened` and `Count` for a Continue button.
- **Feature:** For designers: Save Transform, Save Active State and Save Destroyed components; autosave on scene unload or every N minutes; UnityEvents and Open Slot, New Game and Reload Chosen File on Mono Save Actions; the bridge inspector lists saved fields and has a Copy save ID button.
- **Feature:** Clean JSON for Vector2, Vector2Int, Vector3Int, Color and Rect.
- **Feature:** The bridge inspector's saved field list shows each field's type as a small colour dot. The save ID row hides when its card collapses.
- **Feature:** Mistakes on a saved script, such as a save file with no row in the config, show on its card in the bridge inspector as well as in the Console. The row check now covers every class with `ISaveIn`, not only `ISavable` ones.
- **Feature:** Tools > External Project Tools > Save System > Saved Objects lists every registered saved object in play mode, grouped by save file, on the same cards as the bridge inspector, with a Ping button. Plain classes and ScriptableObjects show up there.
- **Feature:** A "Saved" badge on Hierarchy rows with saved scripts, red on a mistake, and a grey dot on a parent whose children save. It has not been seen on screen: Unity 7000.0.0a16's new Hierarchy window never called the row hook.
- **Fix:** A save file row added through the config's list no longer copies the row before it. It used to keep that row's class and storage name, which blocked `GameSaveFiles.cs` from ever picking it up.
- **Samples:** Eight samples in ladder order: Save A Field, Buttons With No Code, Save Slots And Continue, Pick A Save File, Spawned Copies, Custom Data With ISavable, Whole Files With SaveStorage, ScriptableObject.
- **Samples:** The samples now live in the package at `Modules/SaveSystem/Samples`, next to Runtime, Editor and Tests, with nothing to import. They are one Editor-only assembly, `AbstractPixel.SaveSystem.Samples` (plus `AbstractPixel.SaveSystem.Samples.Tests`), instead of fifteen. Namespaces moved from `SaveSystemSamples.*` to `AbstractPixel.SaveSystem.Samples.*`. A build that includes a sample scene, or a scene using a sample script, stops and names it. Samples load their scenes in the Editor, so they no longer need to be in the build's scene list. Sample 04 uses the built-in `SaveFiles.Settings` instead of shipping its own save file, so no sample row reaches a project's config. Each sample has a one-page `README.html`.
- **Docs:** The guide is grouped as Start, Do, Look up, Understand and Fix, with an install section, a 5-minute first saved value, an API table, a troubleshooting table, diagrams, ladder levels and links to every sample. Tools > External Project Tools > Save System > Open Guide opens it. Status moved to `save-system-roadmap.html` (was `save-system-v2-plan.html`); `save-system-v2-handoff.html` is now `save-system-v1-to-v2.html`. `SaveSystemDocsTests` checks links, anchors and sample code lines.
- **Docs:** A **?** button in the header of the config, Savable Bridge, Mono Save Actions, Save Transform, Save Active State and Save Destroyed inspectors, and in the Saved Objects window's tab bar, opens the guide at that object's section.
- **Fix:** A save system test run no longer leaves errors in the Console. Tests that make a mistake on purpose expect the message with `SaveLogAssert.Expect`, which checks it without printing it. Every save system message now goes through one internal `SaveLog`.
- **Docs:** The guide has screenshots of the config inspector, a Savable Bridge card in play mode and the Saved Objects window.
- **Samples:** The sample tests run on a save system kept in memory, so they never write into the save files you play the samples with.
- **Fix:** When a module or sample deletes its save file class, the sync removes its row and forgets it. It used to also record the row as "removed by you" in the config's hidden lists.
- **Fix:** Listing and deleting game profiles works. Saving no longer erases data of objects that aren't loaded. A Savable Bridge keeps its save ID when a new scene is first saved. A saved scene object switched off and on keeps saving.
- **Cleanup:** Only the public types game code uses show in autocomplete. Save data, rows and storage internals are internal.
- **Settings:** Saves into the built-in `SaveFiles.Settings`. The debug toolbar uses `SaveStorage`.
- **Core:** `SearchableDropdown` works on Unity 6 and Unity 7.
- **Core:** The library no longer needs the Custom Inspector plugin. `GlyphToggler`'s glyph map is a built-in `Dictionary`, so the package needs Unity 6.6 or later.

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