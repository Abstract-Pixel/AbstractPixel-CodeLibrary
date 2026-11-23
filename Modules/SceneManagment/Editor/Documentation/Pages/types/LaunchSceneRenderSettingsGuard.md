# LaunchSceneRenderSettingsGuard

Keeps a launch scene dark while the start scene group opens behind it. Start-up makes the start scene group's main scene active, and the active scene's RenderSettings (ambient, fog, reflections) apply to every scene, so they would light the launch scene.

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement.SceneScreens` · **File:** `LaunchSceneRenderSettingsGuard.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneRenderSettingsGuard.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

When the active scene changes it remembers the new scene's values, swaps in black ones, and puts them back at the hand-off (RestoreMenuSettings), so the start scene group looks as it would without the launch scene.

## Members

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneRenderSettingsGuard -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
