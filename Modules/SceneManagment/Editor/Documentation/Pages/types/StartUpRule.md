# StartUpRule

<!-- layout new -->

Which scene group start-up opens, decided from the settings and the scenes Unity opened (session 4B, decision 2).

<!-- type-header AbstractPixel.SceneManagement.StartUpRule -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/StartUpRule.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#play-from-any-scene).

## Start here

One rule for builds and Play: a build is the case where only build index 0 is open. Plain data in and out: it never calls Unity's scene API, so tests can call it.

| You want to | Use | Why |
|---|---|---|
| See which group a set of open scenes gives | `Choose` | Returns a `StartUpChoice`. |
| Know if the launch scene hold applies | `ShouldHoldLaunchScene` | Only when a launch scene is among the scenes Unity is opening. |

## The rule

The first case that fits wins.

1. The launch scene is open: the start scene group, behind the launch scene.
2. A sample launch scene is open: its sample scene group, behind it (scene system S, Editor only).
3. An open scene, the active one first, is the main scene of a scene group in the settings list: that group. Two such scenes open: an error, and the first one wins.
4. The active scene is a manager or shared scene of a sample scene group: that sample scene group.
5. The active scene is a manager or shared scene: the start scene group, with one line saying why.
6. The active scene is in no scene group: a temporary scene group built from the default scene set, with one warning.

## In code

From a sample test:

```cs source=../../../../Samples/Shared/Tests/Sample04StartUpRuleTests.cs
StartUpChoice choice = StartUpRule.Choose(settings, new List<SceneReference> { shared });
```

<!-- members AbstractPixel.SceneManagement.StartUpRule -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
