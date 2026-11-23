# ISceneTransitionExecutor

The one thing the scene system asks to hide and show the screen around a scene change (ADR 0001). The transitions module provides it and registers it with RegisterTransitionExecutor; with none registered, scenes change with no effect.

**Kind:** interface · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `ISceneTransitionExecutor.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/ISceneTransitionExecutor.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#use-the-basic-fade).

The scene system calls Hide screen only after it accepted a change, so a refused change never covers the screen.

## Members

<!-- members AbstractPixel.SceneManagement.ISceneTransitionExecutor -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
