# RelocationLabel

A relocation label: an asset that names a kind of relocatable, such as "Player" or "Enemy" (ADR 0006). Relocatables and spawn point positions match by label.

**Kind:** asset · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `RelocationLabel.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/RelocationLabel.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#make-a-relocation-label).

An empty label field is "No label", a valid label of its own, so a game that relocates only its player needs no asset.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Preview prefab | `previewPrefab` | Optional. Drawn at every spawn point position with this label when no relocatable with it is open in the Editor. |
| Colour | `colour` | The colour of this label's previews and tags in the Scene view. |
| Steepest ground | `steepestGround` | Steepest ground: the steepest slope, in degrees, this label accepts for a spawn point position. A steeper position is allowed but shown as a warning. |
| Align to surface | `alignToSurface` | Align to surface: positions added with Shift+click tilt to the ground's slope. Off keeps them upright. |

## Members

<!-- members AbstractPixel.SceneManagement.RelocationLabel -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
