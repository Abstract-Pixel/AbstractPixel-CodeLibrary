# TemporarySceneGroup

<!-- layout new -->

Makes a scene group at start-up for a scene that is in no scene group. It is not an asset.

<!-- type-header AbstractPixel.SceneManagement.TemporarySceneGroup -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Runtime/StartUp/TemporarySceneGroup.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

You do not call it. Start-up calls `Create` when you press Play in a scene that no scene group names.

**Called for you, leave alone**

- **Create:** called by start-up with the open scene and a scene set.

**What the group holds**

- **Main scene:** the open scene.
- **Manager scenes, shared scenes, Reset shared scenes:** from the scene set.
- **No scene set:** the group has only its main scene.
- **Name:** Temporary scene group (scene name).
- **Saved:** never; the group is not an asset and equals only itself.

## In code

Each example is closed; open the one you need.

**Example: Start-up makes one for a scene in no group; the same call from code**

```cs
SceneGroup temporary = TemporarySceneGroup.Create(openScene, settings.DefaultSceneSet);
SceneChangeResult result = await SceneActions.ChangeTo(temporary);
```

<!-- members AbstractPixel.SceneManagement.TemporarySceneGroup -->
