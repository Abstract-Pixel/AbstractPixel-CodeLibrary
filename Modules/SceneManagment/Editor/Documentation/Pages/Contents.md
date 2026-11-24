# Contents

- [Home](Home.md)

## Guide

- [Get started](guide/get-started.md)
- [Change scene](guide/change-scene.md)
- [Scene groups and scene sets](guide/scene-groups-and-scene-sets.md)
- [Start-up and Play from any scene](guide/start-up.md)
- [Loading scene](guide/loading-scene.md)
- [Screen and transitions](guide/screen-and-transitions.md)
- [Preload](guide/preload.md)
- [Relocation](guide/relocation.md)
- [Build Settings and Addressables](guide/build-settings-and-addressables.md)
- [Editor tools](guide/editor-tools.md)
- [Scene messages](guide/scene-messages.md)
- [When a scene change fails](guide/when-a-scene-change-fails.md)

## Reference

- [Find a problem](reference/find-a-problem.md)
- [Code map](reference/code-map.md)
- [Design decisions](reference/design-decisions.md)

## Types

### Change scene

- [SceneActions](types/SceneActions.md)
- [MonoSceneActions](types/MonoSceneActions.md)
- [SceneChangeResult](types/SceneChangeResult.md)
- [SceneChangeOutcome](types/SceneChangeOutcome.md)
- [SceneChangeRefusal](types/SceneChangeRefusal.md)
- [SceneEventBus](types/SceneEventBus.md)
- [SceneCoordinator](types/SceneCoordinator.md)

### Scene groups and sets

- [SceneGroup](types/SceneGroup.md)
- [SceneGroupData](types/SceneGroupData.md)
- [TemporarySceneGroup](types/TemporarySceneGroup.md)
- [SceneSet](types/SceneSet.md)
- [SceneSetPart](types/SceneSetPart.md)
- [ShowLoadingScene](types/ShowLoadingScene.md)

### Settings and start-up

- [SceneSystemSettings](types/SceneSystemSettings.md)
- [SceneSystemSettingsSource](types/SceneSystemSettingsSource.md)
- [SceneSystemStartUp](types/SceneSystemStartUp.md)
- [StartUpRule](types/StartUpRule.md)
- [StartUpChoice](types/StartUpChoice.md)
- [StartUpChoiceKind](types/StartUpChoiceKind.md)
- [StartUpSceneMatch](types/StartUpSceneMatch.md)

### Scene loaders

- [ISceneLoader](types/ISceneLoader.md)
- [DefaultSceneLoader](types/DefaultSceneLoader.md)
- [AddressablesSceneLoader](types/AddressablesSceneLoader.md)
- [SceneLoaderSelector](types/SceneLoaderSelector.md)

### Loading scene and progress

- [LoadingProgressView](types/LoadingProgressView.md)
- [LoadingProgressFeel](types/LoadingProgressFeel.md)
- [ILoadingProgressDisplay](types/ILoadingProgressDisplay.md)
- [ILoadingProgressView](types/ILoadingProgressView.md)
- [SimpleLoadingProgressBar](types/SimpleLoadingProgressBar.md)

### Launch scene

- [LaunchSceneReleaseMarker](types/LaunchSceneReleaseMarker.md)
- [LaunchSceneReleaseReceiver](types/LaunchSceneReleaseReceiver.md)
- [LaunchSceneRenderSettingsGuard](types/LaunchSceneRenderSettingsGuard.md)
- [LaunchSceneStartUpLog](types/LaunchSceneStartUpLog.md)
- [LaunchSceneTimelineClock](types/LaunchSceneTimelineClock.md)

### Screen transitions

- [TransitionActions](types/TransitionActions.md)
- [TransitionProfile](types/TransitionProfile.md)
- [ImageFadeProfile](types/ImageFadeProfile.md)
- [TransitionEventBus](types/TransitionEventBus.md)
- [ScreenState](types/ScreenState.md)
- [ISceneTransitionExecutor](types/ISceneTransitionExecutor.md)

### Relocation

- [Relocatable](types/Relocatable.md)
- [RelocationLabel](types/RelocationLabel.md)
- [RelocationLabels](types/RelocationLabels.md)
- [SpawnPoint](types/SpawnPoint.md)
- [SpawnPointPosition](types/SpawnPointPosition.md)
- [IRelocationBroadcaster](types/IRelocationBroadcaster.md)
- [IRelocationHold](types/IRelocationHold.md)

## Samples

- [All samples](../../../Samples/README.md)
- [10 Launch Scene](../../../Samples/10%20Launch%20Scene/README.md)
- [01 Button Changes Level](../../../Samples/01%20Button%20Changes%20Level/README.md)
- [02 Change Level From Code](../../../Samples/02%20Change%20Level%20From%20Code/README.md)
- [03 Fast Restart](../../../Samples/03%20Fast%20Restart/README.md)
- [04 Play From Any Scene](../../../Samples/04%20Play%20From%20Any%20Scene/README.md)
- [05 Persistent Player And Spawn Points](../../../Samples/05%20Persistent%20Player%20And%20Spawn%20Points/README.md)
- [06 Loading Scene](../../../Samples/06%20Loading%20Scene/README.md)
- [07 Preload The Next Level](../../../Samples/07%20Preload%20The%20Next%20Level/README.md)
- [08 Addressables](../../../Samples/08%20Addressables/README.md)
- [09 Editor Tools](../../../Samples/09%20Editor%20Tools/README.md)

## Terms

- scene-group: guide/scene-groups-and-scene-sets.md#scene-groups
- main-scene: guide/scene-groups-and-scene-sets.md#scene-groups
- scene-set: guide/scene-groups-and-scene-sets.md#scene-sets
- override: guide/scene-groups-and-scene-sets.md#overrides
- scene-system-settings: guide/get-started.md#run-the-setup
- scene-system-page: guide/editor-tools.md#scene-groups-debugger
- manager-scene: guide/scene-groups-and-scene-sets.md#scene-sets
- shared-scene: guide/scene-groups-and-scene-sets.md#scene-sets
- reset-shared-scenes: guide/scene-groups-and-scene-sets.md#reset-shared-scenes
- close-previous-manager-scenes: guide/scene-groups-and-scene-sets.md#close-previous-manager-scenes
- loading-scene: guide/loading-scene.md#show-a-loading-scene
- show-the-loading-scene: guide/loading-scene.md#show-a-loading-scene
- loading-progress-view: guide/loading-scene.md#show-the-progress
- scene-status: guide/scene-groups-and-scene-sets.md#scene-status
- scene-picker: guide/editor-tools.md#scene-picker
- scene-in-no-group: guide/start-up.md#play-from-any-scene
- start-up: guide/start-up.md#play-from-any-scene
- start-scene-group: guide/start-up.md#pick-the-start-scene-group
- launch-scene: guide/start-up.md#launch-scene
- launch-scene-hold: guide/start-up.md#launch-scene
- scene-change: guide/change-scene.md#change-scene-from-code
- retry: guide/change-scene.md#retry-the-open-scene-group
- preload: guide/preload.md#preload-a-scene-group
- scene-change-result: guide/change-scene.md#read-the-result
- loading-progress: guide/loading-scene.md#show-the-progress
- fallback-scene-group: guide/when-a-scene-change-fails.md#pick-the-fallback-scene-group
- scene-transition-executor: guide/screen-and-transitions.md#use-the-basic-fade
- hide-screen: guide/screen-and-transitions.md#screen-states
- show-screen: guide/screen-and-transitions.md#screen-states
- screen-counts-as-visible-at: guide/screen-and-transitions.md#screen-counts-as-visible
- screen-state: guide/screen-and-transitions.md#screen-states
- relocatable: guide/relocation.md#make-an-object-a-relocatable
- relocation-label: guide/relocation.md#make-a-relocation-label
- instance: guide/relocation.md#make-a-relocation-label
- spawn-point: guide/relocation.md#add-spawn-points
- spawn-point-position: guide/relocation.md#add-spawn-points
- relocation-broadcaster: guide/relocation.md#what-happens-in-a-scene-change
- hold: guide/relocation.md#what-happens-in-a-scene-change
- relocate: guide/relocation.md#what-happens-in-a-scene-change
- release: guide/relocation.md#what-happens-in-a-scene-change
- wait-for-ground-below: guide/relocation.md#make-an-object-a-relocatable
- ground-layers: guide/relocation.md#what-happens-in-a-scene-change
- steepest-ground: guide/relocation.md#what-happens-in-a-scene-change
- sample-scene-group: guide/editor-tools.md#sample-scene-groups
- sample-launch-scene: guide/editor-tools.md#sample-scene-groups
- scene-documentation: Home.md
- help-button: Home.md
