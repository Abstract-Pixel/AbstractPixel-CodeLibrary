# StartUpChoice

<!-- layout new -->

The result of `StartUpRule.Choose`: what start-up does, the scene group it opens (none for a temporary group, which the caller builds), and at most one line of each kind for the console.

<!-- type-header AbstractPixel.SceneManagement.StartUpChoice -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/StartUpChoice.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

You get one from `StartUpRule.Choose`; start-up reads it. Its setters are internal.

| You want to | Use | Why |
|---|---|---|
| Know what start-up does | `Kind` | A `StartUpChoiceKind`. |
| Know which group opens | `Group` | Null for a temporary group and for DoNothing. |
| Read the console lines | `ErrorMessage`, `WarningMessage`, `InfoMessage` | Empty when there is nothing to say. |

## In code

Each example is closed; open the one you need.

**Example: From a sample test**

```cs source=../../../../Samples/Shared/Tests/Sample10LaunchSceneTests.cs
StartUpChoice choice = StartUpRule.Choose(settings, new List<SceneReference> { splash });
```

<!-- members AbstractPixel.SceneManagement.StartUpChoice -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
