# ImageFadeProfile

<!-- layout new -->

The basic fade's settings: its colour and times. A transition profile.

<!-- type-header AbstractPixel.SceneTransitions.ImageFadeProfile -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/Transitions/BasicFade/ImageFadeProfile.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#use-the-basic-fade).

## Start here

Make one with Create > Utility > SceneRelated > Transitions > ImageFadeProfile. It also has every `TransitionProfile` field.

| You want to | Use | Why |
|---|---|---|
| Fade to another colour | `FadeColor` | The colour the screen fades to. Default black. |
| Make the fade faster or slower | `Duration` | Seconds each fade takes. |
| Wait before the fade | `StartDelay` | Seconds before each fade starts. |
| Shape the fade | `FadeCurve` | Easing from transparent to the fade colour. |

## In the Inspector

Under the header Fade Settings:

| Field | What it does for you | Change it when |
|---|---|---|
| Fade Color | The colour the screen fades to. Default black. | Your game fades to white or a brand colour. |
| Start Delay | Seconds to wait before each fade starts, unscaled time; fractions count. Default 0.5. | The fade should start at once. |
| Duration | How long each fade takes in seconds, unscaled time. Default 1. | The fade feels slow or abrupt. |
| Fade Curve | The easing curve: X is time (0 to 1), Y is alpha (0 to 1). Default linear. | You want an ease in or out. |

<!-- members AbstractPixel.SceneTransitions.ImageFadeProfile -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
