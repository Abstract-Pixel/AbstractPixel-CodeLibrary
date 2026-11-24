# TransitionProfile

<!-- layout new -->

The settings of a screen effect: which controller plays it, and when the screen counts as covered and as visible.

<!-- type-header AbstractPixel.SceneTransitions.TransitionProfile -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/DataLayer/TransitionProfile.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#screen-counts-as-visible).

## Start here

This class is abstract: you make an asset of a profile that derives from it, such as `ImageFadeProfile`, and give it to the `InitializeTransitions` component in a manager scene.

| You want to | Use | Why |
|---|---|---|
| Let the change go on before the cover finishes | `ScreenCountsAsCoveredAt` | Scenes change underneath while the effect finishes on top. |
| Release relocatables earlier or later | `ScreenCountsAsVisibleAt` | The share of Show screen at which relocatables are released. |
| Play a different effect | `TransitionControllerPrefab` | The prefab whose controller plays Hide screen and Show screen. |

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Transition Controller Prefab | The prefab the transition manager creates; it must hold an `ITransitionController`, or an error is logged and no effect plays. | You make your own effect. |
| Screen Counts As Covered At | The share of Hide screen after which the scene change goes on, 0.5 to 1. Default 1. | The wait for a full cover feels long. |
| Screen Counts As Visible At | The share of Show screen at which relocatables are released, 0 to 1. Default 0.75. The effect keeps finishing on top. | The player should get control sooner or later. |

## In code

Each example is closed; open the one you need.

**Example: Pick the effect at run time**

```cs
[SerializeField] private TransitionProfile slowFade;

private void UseSlowFade()
{
    TransitionActions.SetTransitionProfile(slowFade);
}
```

<!-- members AbstractPixel.SceneTransitions.TransitionProfile -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
