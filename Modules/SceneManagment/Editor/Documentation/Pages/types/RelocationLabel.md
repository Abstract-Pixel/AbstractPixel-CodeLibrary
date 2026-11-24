# RelocationLabel

<!-- layout new -->

An asset that names a kind of relocatable, such as "Player" or "Enemy" (ADR 0006). Relocatables and spawn point positions match by label.

<!-- type-header AbstractPixel.SceneManagement.RelocationLabel -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/RelocationLabel.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#make-a-relocation-label).

## Start here

Make one per kind of relocatable with Assets > Create > Abstract Pixel > Scene System > Relocation Label.

| You want to | Use | Why |
|---|---|---|
| Relocate only the player | No asset | An empty label field is "No label", a valid label of its own. |
| Tell kinds apart in the Scene view | `Colour` | Colours the label's previews and tags. |
| Warn about steep positions | `SteepestGround` | A steeper position is allowed but shown as a warning. |

**Called for you, leave alone**

- **Id:** the label's asset GUID, stamped in the Editor. Labels match by it, so a copy in an Addressables bundle still counts as the same label.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Preview prefab | Optional. Drawn at every spawn point position with this label when no relocatable with it is open in the Editor. | You place positions in a scene without the relocatable open. |
| Colour | The colour of this label's previews and tags in the Scene view. | Two labels look alike. |
| Steepest ground | The steepest slope, in degrees, this label accepts for a spawn point position. A steeper position is allowed but shown as a warning. | The default 35 does not fit this kind of object. |
| Align to surface | Positions added with Shift+click tilt to the ground's slope. Off keeps them upright. | The object should sit flat on slopes. |

<!-- members AbstractPixel.SceneManagement.RelocationLabel -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
