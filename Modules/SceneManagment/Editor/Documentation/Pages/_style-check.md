# Style check <!-- level 1 -->

A page that uses every element, to check the look at 275 and 900 px. It is not in the contents.

## Prose and links <!-- level 0 -->

Body text with **bold**, *italic*, `inline code` and a [page link](guide/preload.md#preload-a-scene-group). A term: a [scene group](term:scene-group) opens its page; hover it for the definition. A long word: SceneTransitionExecutorSlotRegistration.

### A sub-heading

- A bullet
  - A nested bullet with `SceneActions.ChangeTo`
- A second bullet

1. Press Play. <!-- done-when OnGameStarted -->
2. Click the button. <!-- done-when screen:Hidden -->
3. Wait for the new room. <!-- done-when OnNewSceneGroupTransitionTo -->

| Label | Member | What it does |
|---|---|---|
| Main scene | `MainScene` | The scene that makes the place. |
| Reset shared scenes | `ForceReloadContextualScenes` | Closes and reopens shared scenes on each scene change. |

## Callouts <!-- level 2 -->

> [!NOTE]
> A note.

> [!WARNING]
> A warning next to the line it is about.

> [!TIP]
> The Game view shows Room B.

> A plain quote.

<details><summary>Why it works</summary>

Because the coordinator waits. See [design decision 3](../../../docs/adr/0003-loading-scene-opens-before-the-old-level-closes.md).

</details>

<details><summary>If it goes wrong</summary>

- Nothing happens → check the Console.

</details>

## Code <!-- level 3 -->

```cs
public class Example : MonoBehaviour
{
    // A comment
    [SerializeField] private SceneGroup next;
    private async void Go() { SceneChangeResult result = await SceneActions.ChangeTo(next); Debug.Log("Changed: " + result + " to a very long line that must scroll sideways"); }
}
```

## Image with marks

![The library logo](../../Resources/AbstractPixelLogo.png)
<!-- marks 1@0.25,0.5 "Left half"; 2@0.75,0.5 "Right half" -->

## Actions and live parts

<!-- action ping "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/01 Button Changes Level/Scene Groups/Sample 01 Room AGroup.asset" "Ping Room A group" -->
<!-- action open-settings "Start-up" "Open Start-up" -->

<!-- your-project start-up -->

<!-- timeline -->

<!-- diagram parts -->

<!-- code-map -->

<!-- members AbstractPixel.SceneTransitions.ScreenState -->
