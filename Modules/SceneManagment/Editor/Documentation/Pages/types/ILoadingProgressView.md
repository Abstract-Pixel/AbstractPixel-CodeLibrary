# ILoadingProgressView

The part of a loading scene that shows loading progress (glossary: loading progress view), as the scene system sees it. The loading scene registers one with RegisterLoadingProgressView.

**Kind:** interface · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `ILoadingProgressView.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/ILoadingProgressView.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

The scene system tells it when Show screen starts (or at the cut), so it starts its appear animation then, and waits for its "full" animation before Hide screen, so the effect and the loading scene never animate over each other.

## Members

<!-- members AbstractPixel.SceneManagement.ILoadingProgressView -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
