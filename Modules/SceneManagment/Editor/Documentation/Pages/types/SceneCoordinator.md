# SceneCoordinator

Runs every scene change: start-up, a change to a scene group, Retry and preload. One runs at a time.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneCoordinator.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Core/SceneCoordinator.cs" "Select the script" -->

Read more in [Scene messages](../guide/scene-messages.md#order-in-a-scene-change).

What to open and close comes from SceneChangePlan; what is open is kept in one record. A change that is accepted hides the screen through the registered scene transition executor and shows it again at the end; a refused change never touches the screen.

## Members

<!-- members AbstractPixel.SceneManagement.SceneCoordinator -->

## See also

- [Scene messages](../guide/scene-messages.md)
- [Find a problem](../reference/find-a-problem.md)
