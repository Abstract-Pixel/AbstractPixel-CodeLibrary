# Changelog

**GameManagement: folder renamed, game state assets removed.**

- **Rename:** `Modules/GameManagment` is now `Modules/GameManagement`, and the runtime asmdef file matches its assembly name. GUIDs are unchanged.
- **Removed:** `Runtime/StateConfigurations/` held one game's state assets. A project that references them must copy them, with their `.meta` files, into its own `Assets` before it updates.
- **Added:** module README, GLOSSARY and an EditMode test assembly.

## [Unreleased] - scene system 5B (2026-11-26)
**Hardening.**
- **Breaking:** `SceneManagment` is now `SceneManagement` (folder and assemblies), and the `Singltons`, `Utilties` and Addressables loader file typos are fixed. Replace `AbstractPixel.SceneManagment` in your scenes, prefabs and assets, or button events stop firing.
- **Change:** the documentation engine moved to `EditorModules/Documentation` (editor only).
- **Change:** Addressables is no longer a package dependency; the scene and transition modules work without Addressables or BroAudio.
- **Feature:** `SceneActions.SetSceneLoader` for a custom loader.
- **Fix:** failing listeners no longer stop a scene change or the fade; stale scene field, inspector and settings page views; sample scenes read "never in build"; Retry in a scene in no group.
- **Fix:** saved scene groups keep each scene's GUID and path; reading `SceneActions` no longer creates a coordinator; relocation runs from an older change no longer move objects; scene status follows a build profile's own scene list; Undo for the before-build check.
- **Fix:** two compile errors on Unity 6.7.

## [Unreleased] - scene system D (2026-11-24)
**Scene documentation inside the Editor.**
- **Feature:** Tools > External Project Tools > Scene System > Open Scene Documentation: a dockable UI Toolkit window with Home, twelve guide pages, Find a problem, Code map, Design decisions, a page per public type and the ten sample pages. Search, Back and Forward, deep links to headings, live "What just happened" timeline and self-ticking steps in Play.
- **Feature:** help buttons open the documentation at the right section from the Scene System page (title and every section), Preferences > Scenes, the scene group, scene set, relocatable and spawn point inspectors, the spawn point overlay, the scene picker footer, the setup window and every group-check problem; eight components and assets without an inspector get a help row above their default inspector.
- **Feature:** each sample README gains "How it's set up" (actions that select the objects that make it work) and "Why it works"; the sample label shows "Guide: Scene documentation window" and an Editor-only Open guide button.
- **Docs:** the module README is now "Start here"; its content moved into the guide pages. `EDITOR-DOCS-UI-GUIDE.md` records what was checked in a running Editor.

## [Unreleased] - scene system 5A2 (2026-11-23)
**Scene picker and setup polish.**
- **Fix (samples):** Sample 05 and the InputRebinding sample no longer draw magenta in URP or HDRP: they use the library's sample shader `Abstract Pixel/Samples/Sample Lit` (no LightMode tag) and its materials. `PipelineDefaultMaterial` and its scene objects are removed.
- **Feature:** two scene opening preferences replace "Open the group with its main scene": "Open a main scene with its scene group" and "Open a scene in no group with the default scene set" (per user, per project, both on; the old value carries over). Preferences, Tools menu (two toggles), setup and the scene picker footer.
- **Feature (scene picker):** rows with a number, a label button (selects the group or scene set), a status button (opens Build Profiles at the scene list in use, the Addressables Groups window, the Scene System page at the group's Debugger row, or selects the group where the fix is) and a folder button; the Scene Groups tab shows the group check; sample scenes are grey; right-click menu (Open, Open additively, Select scene, Select scene group, Copy path); footer "Open with: Group, Default set".
- **Feature (setup):** steps Scene Sets ("Set up your scene sets now?") and Opening Scenes; Terminology lines on each step; the migration step moves to the Scene System page; once set up, the setup menu opens Opening Scenes only.
- **Feature (Scene System page):** rename a scene set from its card (double-click or F2; + opens the new set's name), with a red line for an empty, invalid or taken name.
- **Fix:** the Scene Groups Debugger's main scene field showed "None" when a group's scene link was empty in memory; it finds the scene by GUID, then path.
- **Fix:** setup's Ready line no longer counts sample scene groups as groups to sort into scene sets.

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