# SceneChangeResult

What a scene change request tells its caller when it ends: Done, Refused (with the reason) or Failed (with the scene that failed to load).

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneChangeResult.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeResult.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## In code

From a sample:

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
ShowResults("Change to " + otherGroupName + ": " + result);
```

## Members

<!-- members AbstractPixel.SceneManagement.SceneChangeResult -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
