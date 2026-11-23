# SceneSystemStartUp

Start-up: the first scene change of a session, run by the scene system itself when the game launches or Play is pressed. It reads the scene system settings, picks a scene group from the scenes Unity opened (StartUpRule), creates the coordinator and has it open the group's missing scenes.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneSystemStartUp.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/SceneSystemStartUp.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

Scene changes asked for before that are refused. State resets through StaticsResetter, so it runs on every Play with domain reload off.

## Members

<!-- members AbstractPixel.SceneManagement.SceneSystemStartUp -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
