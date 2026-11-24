# MonoSceneActions

<!-- layout new -->

The scene calls for UnityEvents, such as a button's On Click. UnityEvents list only void methods, so the results are not returned.

<!-- type-header AbstractPixel.SceneManagement.MonoSceneActions -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/MonoSceneActions.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#change-scene-from-a-button).

## Start here

Add this component to any object, then point a UnityEvent at it. Each call starts the matching `SceneActions` call and does not wait for it.

| You want to | Use | Why |
|---|---|---|
| Go to another place from a button | `ChangeTo` | Pick MonoSceneActions > ChangeTo on the On Click and drag the scene group into the argument field. |
| Start the same place again | `Retry` | A restart button with no code. |
| Make the next change fast | `Preload` | Opens the group in the background from an event. |

- **One per object:** the component cannot be added twice to the same object.
- **Result:** not returned. To read a Refused or Failed result, call `SceneActions` from code instead.

**Find it in the sample**

<!-- locate "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/01 Button Changes Level/Scenes/Sample 01 Room A.unity" "Scene Actions" "MonoSceneActions" -->

## In code

Each example is closed; open the one you need.

**Example: Usually called from a Button's On Click with no code. From a script…**

Usually called from a Button's On Click with no code. From a script on the same object:

```cs
[SerializeField] private MonoSceneActions sceneActions;
[SerializeField] private SceneGroup nextGroup;

public void OnContinuePressed()
{
    sceneActions.ChangeTo(nextGroup);
}
```

<!-- members AbstractPixel.SceneManagement.MonoSceneActions -->
