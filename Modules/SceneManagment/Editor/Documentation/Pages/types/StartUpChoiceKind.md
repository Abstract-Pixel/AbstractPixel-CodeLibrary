# StartUpChoiceKind

<!-- layout new -->

An enum: what start-up does with the scenes Unity opened (decision 2 of session 4B). You read it from `StartUpChoice.Kind`, which `StartUpRule.Choose` returns.

<!-- type-header AbstractPixel.SceneManagement.StartUpChoiceKind -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/StartUpChoiceKind.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

Read it from `StartUpChoice.Kind`. Each value matches one case of `StartUpRule`.

| You want to | Use | Why |
|---|---|---|
| Know start-up did nothing | `DoNothing` | No settings, no open scene, or a setting start-up needs is empty. A message says why. |
| Know Play started in a loose scene | `TemporaryGroup` | Start-up built a scene group from the default scene set. |

## In code

Each example is closed; open the one you need.

**Example: Read what start-up chose**

```cs
StartUpChoice choice = StartUpRule.Choose(settings, openScenes);
if (choice.Kind == StartUpChoiceKind.TemporaryGroup)
{
    // The active scene is in no scene group.
}
```

<!-- members AbstractPixel.SceneManagement.StartUpChoiceKind -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
