# Animation Command Catalog

Check this list before writing any UI/feedback animation. Reuse a command where you can; write a new one only when
none fits.

**Rule for new commands:** generic ones go in this package (`Core/Runtime/AnimationScripts/`). Commands that
already live in the project stay there for now (moving them to the package is a separate task).

Source tags: **[pkg]** = this folder. **[proj]** = `Assets/General/Common/CodeAnimationLibrary/` in the game project.

## Framework
| Type | Source | Use |
|---|---|---|
| `IAnimationCommand` | pkg | Interface: `AnimationName`, `ExecuteAnimation()` coroutine. |
| `AnimationCommandQueue` | pkg | Runs commands in sequence. `Initialize(runner)`, `AddComandToQueue(cmd)`, `SetActive`. |
| `AnimationCommandEventBroadcaster` | pkg | Static started / in-progress / ended events. |
| `AnimationBlueprint<T>` | proj | Serialized setup → `CreateRuntimeInstance()`. |
| `AnimationDurationSettings` | proj | Fixed or random-range duration. |
| `ObjectFlickerDelaySettings` | proj | Per-object random delay for the flicker command. |

The project folder also holds its own copies of `IAnimationCommand`, `AnimationCommandQueue` and
`AnimationCommandEventBroadcaster`. Check which namespace the scripts you're working with use.

## Composition
| Command | Source | Use |
|---|---|---|
| `ParallelAnimationCommandGroup(runner, cmds…)` | proj | Run several commands at once. Cancel before reusing. |
| `DelayedAnimationCommand(cmd, delay, unscaled=true)` | proj | Start a command after a delay. |

## Transform / image / layout
| Command | Source | Use |
|---|---|---|
| `TransformTweenCommand(rect, duration, curve, …)` | proj | Default tween: position, scale, CanvasGroup/graphic alpha, optional tint. |
| `ImageFillCommand(image, from, to, duration, …)` | proj | Horizontal/vertical fill reveal (bars, timers). Unscaled time. |
| `RectMaskRevealCommand(mask, duration, curve, …)` | proj | Horizontal reveal of children via RectMask2D; doesn't move the layout. |
| `RectTransformAmbientMotionCommand(rect, amp, freq, …)` | proj | Gentle idle X/Y drift; optional stepped motion. |
| `RuggedAlphaShakeCommand(rect, group, …)` | proj | Stepped alpha flicker plus horizontal shake on a CanvasGroup; optional bursts. |
| `ZRotationalWiggleCommand(…)` | proj | Continuous Z-rotation wiggle. |
| `ZRotationalBurstWiggleCommand(…)` | proj | Z-rotation wiggle bursts. |
| `ObjectActivationFlickerCommand.Initialize(…)` | proj | Flicker GameObjects on/off. Run it on an object outside the targets. |

## TMP text (vertex effects; content and layout untouched)
| Command | Source | Use |
|---|---|---|
| `TMPTypewriterCommand(text, cps, unscaled)` | proj | Reveal text; keeps rich text intact. |
| `TMPCharEntranceAndWaveCommand(…)` | proj | Per-character entrance, then a wave. |
| `TMPCharJaggedWaveCommand(text, duration=0.22, …)` | proj | Quick left-to-right jagged shake; no per-frame GC. |
| `TMPCharacterSquishCommand(text, duration, strength)` | proj | Brief staggered squash/stretch (stamp/impact). |
| `TMPContinuousCharacterShakeCommand(text, unscaled)` | proj | Ongoing shake; `SetStrength`, `SetRotationStrength`. |
| `TMPVerticalOscillationCommand(text)` | proj | Smooth bob; `SetStrength(amp, period)`. |
| `TextScrollingNoiseAlphaAnimation.Initialize(…)` | proj | Scrolling noise alpha across text. |

## Post-processing
| Command | Source | Use |
|---|---|---|
| `VolumeFloatParameterCommand(param, from, to, duration, …)` | proj | Animate a URP Volume float; keeps its value when cancelled. |

## Conventions
- Menus run on unscaled time (they work while the game is paused). Check each command's `useUnscaledTime`
  default.
- Long-running commands expose `Cancel()`. Cancel them on disable or close.
- Put timings in a settings ScriptableObject (see `LeaderboardAnimationSettings`), not inline.
- Screen sequences live in a `<Screen>UIAnimations` class (Win, Pause, GameOver, MainMenu, Leaderboard).

## Feel vs. commands
Some objects (for example the level-select buttons) use the Feel package's feedbacks. Keep Feel where it already
drives an object. For new code-driven UI sequences, use these commands. Never let Feel and a command (or theme
colours) set the same property on the same object: pick one owner per property.
