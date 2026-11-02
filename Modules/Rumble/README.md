# Controller Rumble

Rumble (both motors), the PlayStation light bar and DualSense adaptive triggers, behind one service.
Game code **asks** for feedback. `RumbleService` decides what the connected pad can show, mixes every request,
and talks to the hardware through a backend (Steam Input or the Unity Input System).

## The whole system in one picture

```
 Gameplay code                              RumbleService  (the public API, one per game)
 ─────────────                              ──────────────────────────────────────────────
 RumblePatternPlayer.PlayOnce(pattern) ──►  Play(pattern) ──► RumblePatternMixer (4 one-shot slots)
 CarRumbleDirector (game, every tick)  ──►  SetContinuous / SetTriggers / SetLightBar
 Pause, finish, game over              ──►  Silence(true / false)
                                                 │  each frame: mix → gating
                                                 ▼
                                            RumbleHardwareWriter  (Steam compensation, player slider,
                                                 │                 rate limit, send only changes)
                                                 ▼
                                            IRumbleOutput   (chosen by RumbleDeviceBinder + RumbleBackendSelector)
                                             ├─ SteamInputRumbleOutput        (Steam Input API)
                                             └─ UnityInputSystemRumbleOutput  (Unity Input System, raw DualSense report)
                                                 │
                                                 ▼
                                              controller
```

**From a gameplay call to the controller, one frame:**
1. Game code calls `RumbleService` (or `RumblePatternPlayer.PlayOnce`). A call only stores the request; nothing is sent yet.
2. In `RumbleService.Update` the playing patterns are sampled. For each motor the **strongest** value wins:
   `max(continuous × (1 − mute from patterns), strongest pattern)`, times the profile's *Master Intensity*.
3. Gating: game silence (pause, finish), window focus, feature switches, the motor dead zone and the motor on/off switches.
4. `RumbleHardwareWriter` applies the backend compensation (Steam only), then the player's *Controller Vibration*
   slider (last, so 50 % always means half). It sends only real changes, at most *Output Update Rate* times a second.
5. The active `IRumbleOutput` writes to the pad. If a write fails, that feature is turned off for the session and the
   rest keeps working.

**Which pad and which backend:** `RumbleDeviceBinder` follows the pad the player is using. It ignores a resting second
pad, and keeps the connected pad when the last input was mouse or keyboard. `RumbleBackendSelector` picks the backend
for that pad from *Backend Mode*: Auto uses the preferred backend (Steam) if it can serve the pad, otherwise Unity.
You never handle devices, unplugging, alt-tab or quitting: the service resets the pad itself.

## `RumbleService`: the front door (public API)

`RumbleService` is the **only** script game code talks to. Get it with `ServiceLocator.TryGet(out RumbleService rumble)`.
Everything else in the module is internal plumbing.

| Call | What it does |
|---|---|
| `Play(pattern, strength = 1)` | Starts a one-shot pattern (crash, explosion, menu shake). Up to 4 play at once; a 5th replaces the weakest |
| `StopAllPatterns()` | Stops **every** playing one-shot immediately |
| `SetContinuous(low, high)` | Ongoing rumble, 0..1 per motor (low = heavy/left, high = light/right). Re-send it every tick |
| `SetTriggers(brake, throttle)` | DualSense trigger effects: `TriggerEffect.Resistance(start, strength)`, `TriggerEffect.Vibration(start, amplitude, frequency)` or `TriggerEffect.Off` |
| `SetLightBar(color)` / `ClearLightBar()` | PlayStation light bar colour / back to the rest colour |
| `Silence(true / false)` | See "Silence and one-shots" below |
| `SetPreferredBackend(output)` | Hands in the preferred backend (Steam, or your own). See "Adding a backend" |
| `ActiveCapabilities` | What the current pad can show right now (`Rumble`, `LightBar`, `AdaptiveTriggers`) |
| `RumbleService.PlayerIntensity` / `RumbleService.AreTriggersEnabled` | Player settings (static), set by `ControllerVibrationSetting` and `AdaptiveTriggersSetting` |
| `RegisterDebugSource` / `UnregisterDebugSource` | Show a non-pattern feeder (like the car) in the debug overlay |

The simplest call needs no service lookup: `RumblePatternPlayer.PlayOnce(myPattern)`. You can also add a
`RumblePatternPlayer` component: tick *Play On Enable*, or call `Play()` from a UnityEvent. Both do nothing,
safely, when the pattern or the service is missing.

## Silence and one-shots (exact behaviour)

There are two kinds of rumble:
- **Continuous** (`SetContinuous`, `SetTriggers`): what the game sends every tick, such as driving.
- **One-shots** (`Play(pattern)`): a pattern with a fixed duration that plays to its end by itself.

`Silence(true)` turns off **the continuous motors and the triggers**. It does **not** touch one-shots: they keep
playing and are still felt. This is on purpose. Menus run while the game is silenced (pause, win screen), and their
shakes (engine start, victory guitar, rank stamp) are one-shots that must still play. The light bar keeps working too.

One-shots **can** be stopped, but only all together, with `StopAllPatterns()`. A single one-shot can't be stopped,
because `Play` returns no handle to it. The car uses this: when it becomes silenced (pause, finish, game over),
`CarRumbleDirector` calls `StopAllPatterns()` first, so a crash shake doesn't continue into the pause menu. On game
over it then plays the explosion afterwards.

Separately, when the game window loses focus, **all** motors go to 0 (one-shots included) until it is focused again.

## The car (game code, `Assets/[1]Project/GameplaySystems/ControllerFeedback/`)

The car is a **client** of `RumbleService`, not part of the module. `CarRumbleDirector` runs 30 times a second and:
- sends the **continuous** driving rumble with `SetContinuous`. `CarContinuousRumble` works it out from road and
  throttle by speed, the brake and the launch kick;
- sends the **triggers** with `SetTriggers`. `CarTriggerFeedback` works out the brake shudder, throttle kick and resistance;
- plays **one-shots** for events with `Play`. `CarRumbleMoments` handles crash, hit, landing, bomb pulse and explosion;
- drives the light bar (health colour, hit flash) and calls `Silence` for pause, finish and game over.

All its values are on `CarRumbleProfile.asset`. The car's continuous rumble is not a pattern, so it shows in the
debug overlay through `CarRumbleDebugSource`.

## Debug overlay (find conflicts)

`RumbleDebugOverlay` sits on the `Rumble` object in ManagerBootScene. In the Editor or a development build, press
**F9** in Play mode. The key and whether it starts visible are fields on the component. It lists, live:
- **One-shot patterns:** each playing slot with its name, strength, current heavy / light value and time left.
  Flags: `MUTES-DRIVING` (the driving rumble is pushed down under it), `LIGHT-BAR` and `TRIGGER-BUZZ`.
- **Continuous sources:** for example *Car (continuous)*, with its road+throttle, brake and launch-kick parts and its
  trigger effects, or "silenced".
- **Mix:** the continuous rumble requested, how much the patterns mute it, the mixed value, what was actually sent to
  the pad (after the backend boost and the player slider), and the requested triggers.

**Reading it:**
- When several patterns play at once, the **strongest value per motor wins**, so a weak pattern under a strong one
  isn't felt at all.
- A `MUTES-DRIVING` pattern pushes the car's rumble down.
- Tune pattern strengths and flags (or the *Strength By Speed* curves in `CarRumbleProfile`) until each moment reads
  on its own.

While hidden, the overlay costs one key check per frame. In a release build it switches itself off.

**To show your own system in it:** implement `IRumbleDebugSource` (`DebugName`, `ReadDebugLine()`). Then call
`RegisterDebugSource`, which is safe to call every frame, and `UnregisterDebugSource` when you're done.

## Adding a backend (`IRumbleOutput`)

A backend is one class that turns the service's final values into hardware writes. The two built-in ones are
`SteamInputRumbleOutput` and `UnityInputSystemRumbleOutput`. Write a new one for another API, such as a platform SDK
or a dedicated trigger or haptics library.

**What to implement** (every method returns `false` on failure and must never throw):

| Member | Job |
|---|---|
| `DisplayName` | Text for logs and the debug overlay |
| `Capabilities` | What this backend can do for the bound pad: `Rumble`, `LightBar`, `AdaptiveTriggers` |
| `TryBind(device)` | Can it serve this Input System device? Find the device's handle; return `false` if not |
| `SetMotors(low, high)` | Both motors, 0..1 |
| `SetLightBar(color)` | Light bar colour. If unsupported, return `true` and do nothing |
| `SetTriggers(brake, throttle)` | Trigger effects (`TriggerEffect`: Off, Resistance or Vibration) |
| `Flush()` | Called at the end of each tick, for backends that batch writes |
| `Reset()` | Motors to 0, triggers off, light bar restored |
| `Unbind()` | Forget the pad without writing to it (it was unplugged) |
| `Release()` | Free native resources on quit |

**Where to register it:**
- There is one *preferred* slot (Steam uses it today) plus the built-in Unity fallback.
- Call `SetPreferredBackend(yourOutput)` once at start-up, from a small installer component next to `RumbleService`,
  like `SteamRumbleInstaller` in the game.
- With *Backend Mode* set to Auto, your backend is tried first and Unity is the fallback.
- *Steam Input Only* means "preferred backend only"; the name is historical.
- The Steam compensation (see below) applies to whatever sits in the preferred slot.

**Minimal example:**

```csharp
public class MyTriggerOutput : IRumbleOutput
{
    public string DisplayName => "My Trigger SDK";
    public RumbleCapabilities Capabilities => RumbleCapabilities.Rumble | RumbleCapabilities.AdaptiveTriggers;

    public bool TryBind(InputDevice _activeDevice) { return MySdk.IsConnected(_activeDevice); }
    public bool SetMotors(float _low, float _high) { return MySdk.Rumble(_low, _high); }
    public bool SetLightBar(Color _color) { return true; }   // not supported: nothing to do
    public bool SetTriggers(TriggerEffect _brake, TriggerEffect _throttle) { return MySdk.Triggers(_brake, _throttle); }
    public bool Flush() { return true; }
    public bool Reset() { return MySdk.Rumble(0f, 0f); }
    public void Unbind() { }
    public void Release() { }
}

[RequireComponent(typeof(RumbleService))]
public class MyTriggerInstaller : MonoBehaviour
{
    private void Start()
    {
        RumbleService rumbleService = GetComponent<RumbleService>();
        rumbleService.SetPreferredBackend(new MyTriggerOutput());
    }
}
```

(`MySdk` stands for your library.)

## Where to configure things

| I want to change… | Edit this | Location |
|---|---|---|
| Backend (Auto / Steam / Unity / Off), master strength, write rates, motor switches, dead zone, features, rest colour, Steam compensation | `RumbleOutputProfile` | `Assets/[1]Project/ProjectServices/Rumble/RumbleOutputProfile.asset` |
| How driving feels: road, throttle, launch kick, brake, triggers, light bar, and which pattern plays for crash/hit/landing/bomb/explosion | `CarRumbleProfile` | `Assets/[1]Project/GameplaySystems/ControllerFeedback/CarRumbleProfile.asset` |
| The shape of one moment | a `RumblePattern` | `Assets/[1]Project/GameplaySystems/ControllerFeedback/Patterns/` |
| Player strength and trigger on/off | *Controller Vibration* and *Adaptive Triggers* (Settings → Controls → Controller Feedback) | `ControllerVibrationSetting`, `AdaptiveTriggersSetting` |

Curves read the same everywhere: **X = 0..1** (speed share or time share) and **Y = 0..1** (strength). Every field has
a tooltip. Edit assets during Play mode: you feel the change at once, and it's kept.

**Pattern fields:**

| Field | Meaning |
|---|---|
| Heavy Motor Left / Light Motor Right | Strength over the pattern's duration for each motor |
| Strength | Scales both curves |
| Mute Driving Rumble | The continuous rumble is pushed down while this plays, so the moment is felt on its own |
| Flash Light Bar | The light bar shows the flash colour while this plays |
| Buzz Triggers | L2 and R2 vibrate while this plays (DualSense, both backends) |
| Use Unscaled Time | Pause and slow motion don't stretch it. Leave it on |

**Backend Mode** (the dev toggle) works live in Play mode. In builds, use the launch option
`-rumble-backend=auto|steam|unity|off`.
- In the Editor, **Auto means Unity**, because Steam only drives pads for games it launched.
- Pick *Steam Input Only* to test Steam in the Editor on purpose. Steam must be running, with Steam Input enabled for
  the game.
- Every device or backend change logs one `[Rumble] Device: … -> Backend: …` line.

## Designer workflow

1. **Make a moment:** *Create → AbstractPixel → Rumble → Rumble Pattern*.
2. **Shape it from its sound (recommended):** in the pattern's inspector, open **Bake from sound** and drop in the clip.
   Copy the start, end trim, delay and fades from how the sound is played (its BroAudio entity), then press **Bake**.
   - The heavy motor follows the deep part of the sound; the light motor follows the sharp part (clicks, strums).
   - Quiet moments become gaps.
   - Re-bake any time the sound changes. You can also draw the two curves by hand.
3. **Feel it:** enter Play mode, keep the Game view focused, and press **Preview on controller** in the pattern's
   inspector. Tweak the curves or *Strength* and press it again; Play-mode edits to the asset are kept.
4. **Hook it up:** add one `RumblePatternPlayer.PlayOnce(pattern)` call next to the sound's `BroAudio.Play(...)`, or add a
   `RumblePatternPlayer` component. Nothing else needs wiring.
5. **Check both backends:** switch *Backend Mode* between *Unity Input System Only* and *Steam Input Only* live, and press
   Preview again.
6. **Check it in context:** press F9 during the moment and look at the overlay for anything that hides or mutes it.

Tips:
- A pattern louder than the driving rumble is felt on top of it.
- To make a moment stand on its own, tick *Mute Driving Rumble*.
- Keep impacts short and strong and continuous effects soft: contrast is what makes a hit feel like a hit.

## Steam Input compensation

Steam plays DualSense rumble through its own API, which is weaker than our direct USB write (the Unity backend):
- Steam's motor **1.0 is softer** than a direct 1.0, and the **heavy (left) motor most of all**.
- Steam's trigger vibration has only **8 amplitude steps**, so a soft trigger kick (0.3 → step 2) is barely felt.

So the Steam path, and only that path, lifts what it sends. All fields are on `RumbleOutputProfile` and work live:

| Field | What it does | Default |
|---|---|---|
| Steam Heavy / Light Motor Gain | Multiplies each motor | 2.2 / 1.6 |
| Steam Response Curve | Below 1, lifts soft values more than strong ones, so detail is still felt | 0.6 |
| Steam Trigger Vibration Gain / Floor | Multiplies trigger vibration (throttle kick, brake shudder, bomb buzz) and never lets it drop below the floor (0.5 = step 4 of 8) | 2.5 / 0.5 |
| Steam Impact Rise Threshold | A heavy-motor jump at least this big in one tick counts as an impact (crash, hit), not driving | 0.4 |
| Steam Impact Punch Seconds / Light | On an impact, both motors are held at full for this long (heavy at 1, light at least this value) | 0.15 / 0.8 |

**Trade-off:** Steam can't go above its own 1.0, so strong moments flatten at the top, and the length of time a hit is
held is the only way left to make it hit harder.
- **Don't push the gains too high.** If the driving rumble already sits at 1.0, a collision has nothing to rise above
  and disappears. Driving should sit clearly below 1.0 so impacts stand out.
- None of these settings change the Unity path.
- The player's *Controller Vibration* slider is applied **after** the compensation, so 50 % means half on both backends.

## What lives where

| Class | Job |
|---|---|
| `RumbleService` | The public API: stores requests, then mixes and gates them each frame |
| `RumblePatternMixer` | The 4 one-shot slots: samples them, and the strongest value per motor wins |
| `RumbleHardwareWriter` | Steam compensation, player slider, rate limit, sends only changes |
| `SteamOutputBoost` / `SteamImpactPunch` | The Steam compensation (gain, curve, trigger floor, impact punch) |
| `RumbleDeviceBinder` | Which pad rumbles: follows real pad switches, retries Steam, applies the dev toggle |
| `RumbleBackendSelector` | Which backend serves that pad; turns off a feature that fails |
| `RumbleDeviceWatcher` | Listens for pad switches, unplugging, focus and quit |
| `IRumbleOutput` | The backend contract; `SteamInputRumbleOutput` and `UnityInputSystemRumbleOutput` implement it |
| `DualSenseHidOutputCommand` / `DualSenseTriggerParamWriter` | The low-level DualSense report (Unity) / the trigger buffer (Steam) |
| `RumblePattern` / `RumblePatternPlayer` | A one-shot asset / the one-line way to play it |
| `RumbleDebugOverlay` / `IRumbleDebugSource` | The F9 debug panel / how other systems show up in it |
| `RumbleAudioEnvelopeBaker`, `RumblePatternEditor` (Editor) | Bakes a pattern from a sound; the pattern inspector's Preview and Bake |
| `CarRumbleDirector`, `CarContinuousRumble`, `CarTriggerFeedback`, `CarRumbleMoments`, `CarRumbleDebugSource` (game) | The car, as a client of the service |
| `SteamRumbleInstaller` (game) | Hands the Steam backend to the service |
