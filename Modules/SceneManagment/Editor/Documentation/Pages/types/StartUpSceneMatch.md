# StartUpSceneMatch

<!-- layout new -->

Says whether an open scene is a scene named in the settings.

<!-- type-header AbstractPixel.SceneManagement.StartUpSceneMatch -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/StartUpSceneMatch.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

| You want to | Use | Why |
|---|---|---|
| Compare two scenes | `IsSameScene` | By GUID when both sides know one, otherwise by path when both know one, otherwise by name. |
| Find a scene in a list | `ListHolds` | True when the list holds the scene. |

**Called for you, leave alone**

- **In a build:** a player build knows no GUID for an open scene, only its path.
- **Case:** paths and names compare ignoring case; GUIDs compare exactly.

## In code

Each example is closed; open the one you need.

**Example: Compare scenes by GUID, then path, then name**

```cs
bool same = StartUpSceneMatch.IsSameScene(sceneGroup.MainScene, openScene);
bool listed = StartUpSceneMatch.ListHolds(sceneGroup.ContextualBootScenesList, openScene);
```

<!-- members AbstractPixel.SceneManagement.StartUpSceneMatch -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
