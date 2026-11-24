# SimpleLoadingProgressBar

<!-- layout new -->

The ready-made loading scene's plain progress bar: the fill's right edge follows the smoothed progress.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.SimpleLoadingProgressBar -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/Loading/SimpleLoadingProgressBar.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-the-progress).

## Start here

It is an `ILoadingProgressDisplay`, so a `LoadingProgressView` drives it. It uses the RectTransform's anchors, so it needs no sprite and no art.

| You want to | Use | Why |
|---|---|---|
| A bar with no art | This component | Assign Fill and it works. |
| A bar with its own look | Your own `ILoadingProgressDisplay` | This bar only moves an anchor. |

**Called for you, leave alone**

- **Show, SetProgress, PlayFull, Hide:** the view calls them. Show sets 0, PlayFull sets 1.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Fill | The fill, stretched inside the bar's background; its right anchor follows the progress. | You replace the fill object. |

## In code

Each example is closed; open the one you need.

**Example: The view drives it; the same calls from code look like this**

```cs
[SerializeField] private SimpleLoadingProgressBar bar;

private async Task Demo()
{
    bar.Show();
    bar.SetProgress(0.5f);
    await bar.PlayFull();
    bar.Hide();
}
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.SimpleLoadingProgressBar -->
