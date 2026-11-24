# RelocationLabels

<!-- layout new -->

Helpers that say what a relocation label means when it may be "No label" (null): its name, colour and steepest ground.

<!-- type-header AbstractPixel.SceneManagement.RelocationLabels -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Relocation/Runtime/RelocationLabels.cs" "Select the script" -->

Read more in [Relocation](../guide/relocation.md#make-a-relocation-label).

## Start here

Use these in editor tools or checks instead of reading a label field directly, because the field may be empty.

| You want to | Use | Why |
|---|---|---|
| Compare two label fields | `Same` | Both "No label", the same asset, or two copies of one asset count as the same. |
| Show a label's name | `NameOf` | Returns "No label" for an empty field. |
| Find its steepest ground | `SteepestGroundOf` | For "No label" it reads the project-wide value on the Scene System page. |
| Raycast for ground | `GroundLayers` | The Ground layers from the Scene System page. |

<!-- members AbstractPixel.SceneManagement.RelocationLabels -->

## See also

- [Relocation](../guide/relocation.md)
- [Find a problem](../reference/find-a-problem.md)
