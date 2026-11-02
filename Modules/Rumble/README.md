# Controller Rumble

Rumble, PlayStation light bar and DualSense adaptive triggers, through one service.
Game code **asks**; `RumbleService` decides what the pad can show, mixes everything and talks to the hardware.

```
Game code ──requests──► RumbleService ──► backend (Steam Input, or Unity Input System) ──► pad
```

## Where to configure things

| I want to change… | Edit this asset | Location |
|---|---|---|
| Backend (Auto / Steam / Unity / Off), master strength, write rates, feature on/off, rest light colour | `RumbleOutputProfile` | `Assets/[1]Project/ProjectServices/Rumble/RumbleOutputProfile.asset` |
| How driving feels: road, throttle, brake, triggers, light bar colours, which pattern plays for crash/hit/landing/bomb/explosion | `CarRumbleProfile` | `Assets/[1]Project/GameplaySystems/ControllerFeedback/CarRumbleProfile.asset` |
| The shape of one moment (crash, explosion, start-up…) | a `RumblePattern` | `Assets/[1]Project/GameplaySystems/ControllerFeedback/Patterns/` |
| The player's strength and trigger on/off | "Controller Vibration" slider and "Adaptive Triggers" toggle (Settings → Controls → Controller Feedback) | `ControllerVibrationSetting`, `AdaptiveTriggersSetting` |
| How strong Steam Input feels compared to Unity | `RumbleOutputProfile` → *Steam Input motor boost* / *Steam Input impact punch* | see "Steam Input compensation" below |

Every field has a tooltip. Curves read the same everywhere: **X = 0..1** (speed share or time share), **Y = 0..1** (strength).
Edit the assets during Play mode: you feel the change at once and it is kept.

**Speed feel** (`CarRumbleProfile`): road hum, bumps and throttle grow with speed and stay silent at parking speed. Shape them with the *Road* and *Throttle* curves.

**Which motor** (`CarRumbleProfile` → *Which motor*): pick Heavy (left grip, deep shake), Light (right grip, fast buzz) or Both, separately for the speed rumble and the brake.

**Motor switches** (`RumbleOutputProfile`): *Heavy Motor Enabled* and *Light Motor Enabled* turn a motor off for every effect. *Motor Dead Zone* sends anything weaker as 0, because PlayStation pads buzz even at tiny values.

**Pattern fields** (each `RumblePattern`):

| Field | Meaning |
|---|---|
| Heavy Motor Left / Light Motor Right | Strength over the pattern's duration for each motor |
| Mute Driving Rumble | The road/throttle/brake rumble goes quiet while this plays, so the moment is felt on its own |
| Flash Light Bar | The PlayStation light bar shows the flash colour while this plays |
| Buzz Triggers | L2 and R2 vibrate while this plays (DualSense, both backends) |
| Use Unscaled Time | Pause and slow motion don't stretch it. Leave on |

**Dev toggle:** select `RumbleOutputProfile.asset` and change `Backend Mode`. This works live in Play mode. In builds, use the launch option `-rumble-backend=auto|steam|unity|off`.
In the Editor, **Auto means Unity**: Steam can bind a pad inside the Editor, but it only drives pads for games Steam launched. Pick *Steam Input Only* to test Steam in the Editor on purpose (Steam must be running and Steam Input enabled for the game). Builds try Steam first.

## Designer workflow

1. **Make a moment:** *Create → AbstractPixel → Rumble → Rumble Pattern*.
2. **Shape it from its sound (recommended):** in the pattern's inspector open **Bake from sound**, drop in the clip, copy the start / end trim / delay / fades from how the sound is played (its BroAudio entity), press **Bake**. The heavy motor follows the deep part of the sound, the light motor the sharp part (clicks, strums); quiet moments become gaps. Re-bake any time the sound changes.
   Or draw the two curves by hand.
3. **Feel it:** enter Play mode, keep the Game view focused, press **Preview on controller** in the pattern's inspector. Tweak curves or *Strength* and press again; Play-mode edits to the asset are kept.
4. **Hook it up:** one `RumblePatternPlayer.PlayOnce(pattern)` next to the sound's `BroAudio.Play(...)`, or a `RumblePatternPlayer` component. Nothing else to wire.
5. **Check both backends:** flip *Backend Mode* between *Unity Input System Only* and *Steam Input Only* live and press Preview again.

Tips: a pattern louder than the driving rumble is felt on top of it; to make a moment stand alone, tick *Mute Driving Rumble*. Keep impacts short and strong, and continuous effects soft: contrast is what makes a hit feel like a hit.

## Steam Input compensation

Steam plays DualSense rumble through its own API, which is weaker than our direct USB write (the Unity backend):
- Steam's motor **1.0 is softer** than a direct 1.0, and the **heavy (left) motor most of all**.
- Steam's trigger vibration has only **8 amplitude steps**, so a soft trigger kick (0.3 → step 2) is barely felt.

So the Steam path (only) lifts what it sends. All fields are on `RumbleOutputProfile` and work live:

| Field | What it does | Default |
|---|---|---|
| Steam Heavy / Light Motor Gain | Multiplies each motor | 2.2 / 1.6 |
| Steam Response Curve | Below 1 lifts soft values more than strong ones, so detail is still felt | 0.6 |
| Steam Trigger Vibration Gain / Floor | Multiplies trigger vibration (throttle kick, brake shudder, bomb buzz) and never lets it drop below the floor (0.5 = step 4 of 8) | 2.5 / 0.5 |
| Steam Impact Rise Threshold | A heavy-motor jump at least this big in one tick counts as an impact (crash, hit), not driving | 0.4 |
| Steam Impact Punch Seconds / Light | On an impact, both motors are held at full for this long (heavy 1, light at least this) | 0.15 / 0.8 |

**Trade-off:** Steam can't go above its own 1.0, so strong moments flatten at the top and the time a hit is held is the only way left to make it hit harder. **Don't push the gains too high:** if driving rumble already sits at 1.0, a collision has nothing to rise above and disappears. Driving should sit clearly below 1.0 so impacts stand out. The Unity path is never changed by any of these. The player's *Controller Vibration* slider is applied **after** the compensation, so 50 % always means half on both backends.

## Public API

### Play a one-shot moment (most common)

```csharp
RumblePatternPlayer.PlayOnce(myPattern);          // from code, next to BroAudio.Play(...)
```
Or add a **`RumblePatternPlayer`** component next to a sound. It plays on enable (tick *Play On Enable*), or call `Play()` from a UnityEvent.
Both are safe when the pattern or the service is missing: they simply do nothing.

### `RumbleService` (find it with `ServiceLocator.TryGet(out RumbleService rumble)`)

| Call | What it does |
|---|---|
| `Play(pattern, strength = 1)` | Plays a one-shot pattern |
| `StopAllPatterns()` | Stops every one-shot |
| `SetContinuous(low, high)` | Ongoing rumble (0..1 each). Re-send it every tick; low = left motor, high = right motor |
| `SetLightBar(color)` / `ClearLightBar()` | PlayStation light bar colour / back to the rest colour |
| `SetTriggers(brake, throttle)` | DualSense trigger effects, e.g. `TriggerEffect.Resistance(start, strength)`, `TriggerEffect.Off` |
| `Silence(true/false)` | Driving rumble and triggers off (pause, finish). One-shot patterns still play, so menus can shake |
| `ActiveCapabilities` | What the current pad can show (`Rumble`, `LightBar`, `AdaptiveTriggers`) |
| `RumbleService.PlayerIntensity` | Player strength 0..1, set by the settings slider |

You never need to handle devices, unplugging, alt-tab or quitting. The service resets the pad itself.

## How to…

- **Add a new moment:** create a pattern with *Create → AbstractPixel → Rumble → Rumble Pattern*, draw its curves, then add one `PlayOnce` call (or a `RumblePatternPlayer`) where its sound plays.
- **Add a new pad type or backend:** write one class that implements `IRumbleOutput`, then hand it to the service with `SetPreferredBackend` (see `SteamRumbleInstaller`).
- **See what is happening:** select the `Rumble` object (ManagerBootScene) in Play mode. Its *Debug* fields show the device, backend, capabilities and live motor, light and trigger values. Every device change logs one `[Rumble] Device: … -> Backend: …` line.

## What lives where

| Class | Job |
|---|---|
| `RumbleService` | The front door; mixes requests and writes each tick |
| `RumbleBackendSelector` | Picks Steam or Unity for the active pad; turns off a feature that fails |
| `RumblePatternMixer` | Plays up to 4 one-shots at once |
| `RumbleHardwareWriter` | Sends only real changes, at a limited rate; applies the Steam compensation |
| `SteamOutputBoost` / `SteamImpactPunch` | The Steam compensation (gain, curve, trigger floor, impact punch) |
| `RumbleDeviceBinder` | Which pad rumbles; after mouse/keyboard input it keeps (or picks) the connected pad, so the first shake at boot is not lost |
| `RumbleAudioEnvelopeBaker` (Editor) | Bakes a pattern from a sound; used by the pattern inspector |
| `RumbleDeviceWatcher` | Listens for pad switches, unplugging, focus and quit |
| `SteamInputRumbleOutput` / `UnityInputSystemRumbleOutput` | The two backends |
| `CarRumbleDirector` (game) | Turns the car and game events into rumble requests |
