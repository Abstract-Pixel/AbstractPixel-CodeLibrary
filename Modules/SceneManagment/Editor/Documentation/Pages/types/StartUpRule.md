# StartUpRule

Which scene group start-up opens, decided from the settings and the scenes Unity opened (session 4B, decision 2). One rule for builds and Play: a build is the case where only build index 0 is open.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `StartUpRule.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/StartUpRule.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

The launch scene is open: the start scene group, behind the launch scene. A sample launch scene is open: its sample scene group, behind it (scene system S, Editor only). An open scene, the active one first, is the main scene of a scene group in the settings list: that group. Two such scenes open: an error, and the first one wins. The active scene is a manager or shared scene of a sample scene group: that sample scene group. The active scene is a manager or shared scene: the start scene group, with one line saying why. The active scene is in no scene group: a temporary scene group built from the default scene set, with one warning. Plain data in and out: it never calls Unity's scene API.

## Members

<!-- members AbstractPixel.SceneManagement.StartUpRule -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
