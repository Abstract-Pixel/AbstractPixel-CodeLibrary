# Controller Rumble

Controller feedback in one service: **rumble** (the two motors), the **light bar** (the coloured strip on PlayStation
pads), the **player LEDs** (the small white line under a DualSense touchpad) and the DualSense **adaptive triggers**
(L2 / R2 that push back or buzz).

Read from the top. Each section goes one level deeper; stop when you know enough.

1. [Use it](#1-use-it) – the few calls you need
2. [What the game does with it](#2-what-the-game-does-with-it) – car, menus, UI
3. [Change how it feels](#3-change-how-it-feels) – which asset to edit
4. [How a call reaches the controller](#4-how-a-call-reaches-the-controller) – the pipeline
5. [Steam or Unity: which one talks to the pad](#5-steam-or-unity-which-one-talks-to-the-pad)
6. [When it doesn't work](#6-when-it-doesnt-work) – debug panel and known causes
7. [Deep dive](#7-deep-dive) – raw DualSense reports, Bluetooth, adding a backend

**Words used here**

| Word | Meaning |
|---|---|
| Pattern | An asset describing one short shake (a crash, a click). Plays to its end by itself |
| Continuous | Rumble the game re-sends every tick, like driving over gravel |
| Backend | The code that actually talks to the pad: **Steam** (Steam's controller API) or **Unity** (the Unity Input System) |
| Steam Input | Steam's controller layer. When it is on for a pad type, Steam owns that pad and the game should talk to Steam |
| Capability | Something the current pad can show: `Rumble`, `LightBar`, `AdaptiveTriggers`, `PlayerLeds` |

---

## 1. Use it

Get the service once: `ServiceLocator.TryGet(out RumbleService rumble)`. It is the only class game code talks to.

| I want to… | Call |
|---|---|
| Play a one-off shake | `RumblePatternPlayer.PlayOnce(pattern)` (no lookup needed, safe if anything is missing) |
| Rumble continuously (send every tick) | `rumble.SetContinuous(heavy, light)`, each 0..1 |
| Colour the light bar | `rumble.SetLightBar(color)`; `ClearLightBar()` goes back to the default colour |
| Light the player LEDs | `rumble.SetPlayerLeds(mask)`; 5 bits, bit 0 = leftmost. `ClearPlayerLeds()` turns them off |
| Make the triggers resist or buzz | `rumble.SetTriggers(brake, throttle)` with `TriggerEffect.Resistance(...)`, `.Vibration(...)` or `.Off` |
| Mute driving rumble (pause, menus) | `rumble.Silence(true)`; one-off shakes still play |
| Stop every one-off shake now | `rumble.StopAllPatterns()` |
| Know what the pad can do | `rumble.ActiveCapabilities` |

You never handle plugging, unplugging, switching pads, alt-tab or quitting. The service does.

No code at all: add a `RumblePatternPlayer` component next to a sound and tick *Play On Enable*, or call `Play()` from
a UnityEvent.

## 2. What the game does with it

Game code lives in `Assets/[1]Project/GameplaySystems/ControllerFeedback/`.

**Light bar, by situation**

| Situation | Light bar | Who sets it |
|---|---|---|
| Main menu | Purple, slow breath | `MenuLightBarFeedback` (LevelSelectionScene) |
| Level select | The shown stage's colour (`StageThemePalette.GlowColor`), changes with the page | `MenuLightBarFeedback` |
| Driving | Health colour (green → red); a white flash on every hit | `CarRumbleDirector` → `CarLightBarFeedback` |
| Bomb at high priority | Each bomb beat: black, ramp to red, **fast** snap back to the health colour | same |
| Paused (any menu over gameplay) | Orange, slow breath | same |
| Game Over | Red, slow breath | same |
| Win screen | Green, slow breath | same |

"Breath" = the colour dims and brightens smoothly (`LightBarBreath`). The DualSense has no built-in pulse
animation for the light bar, so the game animates the colour itself; only real changes are sent to the pad.

**Player LEDs** (Unity backend only, see section 5): a health bar. 5 lit at full health, one fewer per lost fifth, a
quick flash on every hit, blinking at 25 % health or less, all off at Game Over (`CarPlayerLedFeedback`).

**Rumble moments:** road, throttle, brake and launch kick (continuous); crash, hit, landing, bomb beat, explosion
(patterns); engine start on the main menu, the Play roar, the victory guitar and the rank stamp.

**UI:** every button that announces itself on `UIInteractionEvents` (AbstractPixel.Core) also rumbles: a light
tick on hover or gamepad select, and a very short heavy knock on click. This is `UIRumbleFeedback`, which lives in
ManagerBootScene. RallyShift's `UIAudioFeedback` raises those events whenever it plays a UI sound. Buttons that
already have their own shake carry a `SkipUIClickRumble` component (level select's Play button).

**Reusable pieces in this module** (`Runtime/Feedback/`, not tied to RallyShift):

| Class | What it does |
|---|---|
| `LightBarBreath` | Breathing colour pulse for the light bar |
| `UIRumbleFeedback` | UI hover and click rumble |
| `SkipUIClickRumble` | Marker that turns the click rumble off for one button |

Any game can use them. It only needs a component that calls `UIInteractionEvents.RaiseHovered` / `RaiseClicked`.

## 3. Change how it feels

All fields have tooltips and can be edited in Play mode (the change is felt at once and kept).

| To change… | Edit |
|---|---|
| Driving feel, triggers, health colours, hit flash, bomb beat timings, pause / Game Over / win colours and breath, player LEDs | `ControllerFeedback/CarRumbleProfile.asset` |
| One shake's shape | its pattern in `ControllerFeedback/Patterns/` (UI: `UIHover`, `UIClick`) |
| UI rumble strength / hover spacing | the `UIRumbleFeedback` component in ManagerBootScene |
| Main menu colour and breath | `MenuLightBarFeedback` in LevelSelectionScene |
| Backend, master strength, feature switches, default colour, Steam boost | `ProjectServices/Rumble/RumbleOutputProfile.asset` |
| Player strength / triggers on-off | Settings → Controls (`ControllerVibrationSetting`, `AdaptiveTriggersSetting`) |

**Making a pattern:** *Create → AbstractPixel → Rumble → Rumble Pattern*. In its inspector, **Bake from sound** shapes
it from an audio clip, and **Preview on controller** (Play mode) lets you feel it. Curves are X = time 0..1,
Y = strength 0..1. Keep hits short and strong and continuous effects soft.

## 4. How a call reaches the controller

```
 your call ──► RumbleService (stores the request)
                   │ every frame
                   ▼
              mix: for each motor the strongest request wins
              gate: pause silence, window focus, feature switches
                   │
                   ▼
              RumbleHardwareWriter: player's strength slider, sends only changes, rate-limited
                   │
                   ▼
              backend: Steam  or  Unity  ──► controller
```

- A call never talks to the pad directly; it only stores what you want.
- Up to 4 patterns play at once; per motor the strongest wins, so a weak pattern under a strong one isn't felt.
- When the window loses focus all motors stop (the light bar stays).
- While the game is paused (`Time.timeScale` 0), patterns that run on game time stay silent and resume with the game.
  UI patterns (*Use Unscaled Time*) keep playing.
- Light bar writes are capped at 30 per second, so a breathing colour never floods the pad.
- If a feature fails on the pad (e.g. the light bar write is refused), only that feature is turned off, and it is
  tried again when a different pad is connected. If the pad refuses everything 30 times in a row, output to that pad
  stops (one warning) until it reconnects.

## 5. Steam or Unity: which one talks to the pad

*Backend Mode* on `RumbleOutputProfile` (in builds also `-rumble-backend=auto|steam|unity|off`):

| Mode | Rule |
|---|---|
| **Unity** | Always Unity. Steam is never asked |
| **Steam** | Steam. If Steam can't serve the pad (not running, lists no pad, can't tell which pad is yours, or a Steam write fails) Unity takes over **at once** (*fallback*); Steam is re-checked every second and takes the pad back when it can |
| **Auto** (default) | Steam when **Steam Input is on** for the pad, otherwise Unity. **In the Editor Auto means Unity** |

**How "Steam Input is on" is detected:** Steam lists the pad to the game (`GetConnectedControllers`). Steam only hands
a game the pads it drives with Steam Input, so a listed pad is a Steam Input pad, over USB or Bluetooth alike (Steam
does the transport). The game's opt-in mask (`GetSessionInputConfigurationSettings`) is **not** used to decide: it
only holds the game's Steamworks default plus the player's per-game override, not Steam's global per-family settings.
Deciding with it was the Nov 2026 regression (Auto refused a DualSense Steam was driving, Unity's writes then bounced
off the pad). The F9 details still show it, as information.

**Fallback** (F9 short check, Player.log) means Unity serves a pad Steam was expected to serve: any Unity binding in
Steam mode, or in Auto when Steam lists the pad but couldn't serve it. Auto with Steam Input off is the normal Unity
rule, not a fallback. Features switched off after a failure (e.g. Unity's writes refused while Steam held the pad)
are allowed again whenever the backend changes.

**Steam Input can hide the pad from Unity completely** (seen on hardware, Nov 2026). With Steam Input on for the
game, Steam takes the pad as soon as the game registers with Steam (`SteamAPI.Init`, even without `SteamInput.Init`):
the pad vanishes from the Input System. Started from the Steam library, Steam should give the game a virtual Xbox pad
instead, through its overlay (expected Steam behaviour, still to be confirmed with this build on hardware). Started any other way (exe double-clicked with `steam_appid.txt`, or the Editor) the
overlay is missing, so the game gets **no pad at all**: no controller input, and no Unity backend to fall back to.
The rumble code handles it: a Steam binding stays when Unity loses the device, and in Auto / Steam mode Steam is
asked even when Unity sees no gamepad (Steam's first pad is used). The F9 device then reads "Steam pad (Unity can't
see it)" and *Fallback* reads `NOT POSSIBLE` when Steam can't serve it either. **Test Steam Input only from the
Steam library** (F9 details: *Overlay on*). Release depots must not ship `steam_appid.txt`, so
`RestartAppIfNecessary` relaunches a double-clicked exe through Steam.

Two pads through Steam (e.g. a DualSense and an Xbox pad): with Steam Input on, Windows shows each one to the game
as a virtual Xbox pad. Steam says which real pad sits behind each XInput slot (`GetControllerForGamepadIndex`), so
the pad in the player's hands is matched exactly. A raw PlayStation device is matched by type.

What each backend can do:

| | Rumble | Light bar | Triggers | Player LEDs |
|---|---|---|---|---|
| Unity, DualSense (USB or Bluetooth) | yes | yes | yes | yes |
| Unity, DualShock 4 on USB | yes | yes | – | – |
| Unity, DualShock 4 on Bluetooth | – | – | – | – (nothing is sent, see section 6) |
| Steam, DualSense / DualShock 4 | yes | yes | yes (DualSense) | **no** – Steam has no call for them |
| Steam, Xbox pad / Steam Deck / Steam Controller | rumble only (Xbox gets no Steam strength boost) | – | – | – |
| Unity, Xbox and other pads | rumble only | – | – | – |
| Steam, generic / Switch pads | left to Unity | | | |

Every pad change (plug, unplug, USB ↔ Bluetooth, Xbox ↔ PlayStation) picks the backend again and re-sends
everything (colour, LEDs, triggers), so the light bar comes back by itself.

## 6. When it doesn't work

**Pass / fail per controller, step by step:** [CONTROLLER_VERIFICATION.md](CONTROLLER_VERIFICATION.md).

**The F9 panel** (`RumbleDebugOverlay`, next to the service) works in the Editor and in **Development Builds only**.
In a release build it switches itself off.
- **First F9:** a short check.
  - Top line: the controller type in its colour, with the model and connection. **PLAYSTATION** is blue, **XBOX**
    green, **STEAM** (Deck, Steam Controller) greyish blue, and **GENERIC** / **NO CONTROLLER** grey. A pad driven
    through Steam keeps its family colour and reads e.g. "PS5 pad through Steam Input".
  - Then a verdict: **WORKING**, **WAIT** or **NOT WORKING**, plus what to do. Bluetooth failures appear in orange
    as **WAIT · BLUETOOTH** or **NOT WORKING · BLUETOOTH**.
  - Then always two lines:
    - **Steam Input:** `ON` (Steam drives the pad), `OFF for this pad` (Steam runs but lists no pad), `NOT USED`
      (Unity mode, or Auto in the Editor), `NOT AVAILABLE` (no Steam backend, Steam not running or Init failed),
      `FAILED` (a Steam write failed), or `ON, but …` (listed but not matched / type left to Unity).
    - **Fallback:** `ACTIVE - Unity serves the pad because: …` or `none (…)` with the reason.
  - Then *THE PAD SHOULD SHOW NOW*: the light bar colour as a swatch with its name, the LED count, rumble on/off and
    the triggers. If the pad doesn't match, output isn't reaching it.
- **Second F9:** full details.
  - A *Controller* section: type, model, connection now and at bind (and how it was read), device / layout /
    product / interface, Bluetooth drops, seconds since the pad last sent input, and the Bluetooth flag with its reason.
  - *Backend*: mode, active backend and the rule that picked it.
  - *Steam Input API*: Steam's last answer; Init state; overlay on/off (off usually means Steam did not launch the
    process, so it hands it no pads); Steam Deck; every pad Steam lists with its type and XInput slot (bound one
    marked); the game's opt-in mask (information only). Not asked in Unity mode.
  - *Pads Unity sees*: every gamepad with type, model, USB / Bluetooth, layout and XInput slot (bound one marked).
  - Then capabilities, every playing pattern and the mix.
  - On a DualSense on the Unity backend, also USB/Bluetooth, descriptor size, reports sent / refused and the last
    result.
- **Third F9:** hides the panel.

**F10** shows the raw bytes, and **F11** runs the Bluetooth self-test (expect `PASS`).

**The log works in every build.** Every pad or backend change (including a fallback after a failed Steam write)
writes a block like this:

```
[Rumble] Device: DualSense Wireless Controller [layout DualSenseGamepadHID, product 'DualSense Wireless Controller',
interface HID] -> Backend: Steam Input (k_ESteamInputType_PS5Controller) -> Caps: Rumble, LightBar, AdaptiveTriggers
(mode Auto, Steam (Auto)) Steam: bound PS5Controller
[Rumble] Steam Input: ON - Steam Input drives this pad | Fallback: none (Steam serves the pad) | mode Auto
Steam Input API:
  SteamInput.Init ok.  Overlay on   Steam Deck off
  Steam lists 1 pad(s) to this game
    [0] PS5Controller  XInput slot 0  <- BOUND
  Game opt-in mask 0x… (PlayStation …, Xbox …, …)  info only: the pads Steam lists are what counts
Pads Unity sees:
  PlayStation - DualSense (PS5), USB, layout DualSenseGamepadHID  <- BOUND
```

In Auto and Steam mode, a change in Steam's answer between binds (e.g. Steam starts or stops listing the pad) writes
one `[Rumble] Steam Input: … | Fallback: …` line.

The backend part says the most: `Unity (DualSense Usb)` / `Unity (DualSense Bluetooth)`, `Unity (XInputControllerWindows)`,
`Unity (DualShock4GamepadHID)`, or `Steam Input (<pad type>)`. An Xbox layout for a PlayStation pad means Steam
Input or DS4Windows is wrapping it.

Failures write one warning each, never per frame. In the Editor it's the Console. In a build it's:

```
%USERPROFILE%\AppData\LocalLow\Abstract Pixel Studios\RallyShift\Player.log
```

(`Player-prev.log` holds the run before.) When someone without the Editor reports a problem, ask for this file first.
Then ask for an F9 screenshot if they have a development build.

**Warnings you may see and what they mean**

| Log line starts with | Meaning |
|---|---|
| `Controller: PlayStation - DualSense (PS5), connection Bluetooth (…)` | Not a warning. One per bind: type, model, connection and how it was read |
| `Bluetooth: … disconnected (N drop(s) in the last minute)` | The bound Bluetooth pad was removed by Windows. Several in a minute = unstable link |
| `… is on Bluetooth and the game's raw report is not available for it` | A DualShock 4 (or a DualSense whose raw report failed) on Bluetooth with the Unity backend. Nothing is sent to it, on purpose: Unity's own call is USB-only and sends garbage over Bluetooth |
| `DualSense refused 78-byte Bluetooth reports …; trying N bytes` | First fallback over Bluetooth: retry at the size the pad's descriptor states |
| `DualSense refused Usb/Bluetooth reports …; trying … format` | We guessed the connection type wrong; switching once |
| `DualSense refused raw reports in both … formats` | Raw output is off for this pad. Rumble falls back to Unity's plain call |
| `DualSense descriptor read late` | Windows had no descriptor yet when the pad appeared; it was re-read and the format corrected |
| `Unity backend: … refused 30 sends in a row` | Output to this pad stops until it reconnects. Usually a DualShock 4 on Bluetooth, or DS4Windows / DSX |
| `<Capability> failed on …; turned off until the pad changes` | One feature was refused; the rest still work |
| `Steam Input backend failed; falling back to the Unity Input System` | A Steam write failed; Unity serves the pad until it changes |
| `Steam Input: OFF for this pad … Fallback: none (Auto rule …)` | Not a warning: Auto with Steam Input off, Unity drives the pad |
| `Steam Input: … Fallback: ACTIVE …` | Steam was expected but Unity serves the pad; the reason follows |
| `Steam no longer serves the pad …; rebinding` | Steam stopped listing the bound pad (unplugged, reconnected, Steam Input turned off) |
| `The light bar is switched off in RumbleOutputProfile` | A setting, not a bug: turn the LightBar feature back on |

**Known reasons the light bar or LEDs don't change on someone's machine**

| Cause | What happens | Fix for the player |
|---|---|---|
| Steam Input on for the game, but Backend = Unity | Steam holds the pad: Unity's writes are refused, or reach only Steam's virtual Xbox pad (no light bar, LEDs or triggers). The verdict says "Steam Input holds it" | Use Auto (default) or Steam mode; check the F9 *Steam Input API* section for why Steam didn't take it |
| DS4Windows, DSX or HidHide running | They take over or hide the pad | Close them |
| Steam Input on, game not started from the Steam library (or the Editor) | Steam takes the pad from the game and gives nothing back: no input, *Pads Unity sees: none* | Start from the Steam library, or turn Steam Input off for the game |
| Steam's per-game rumble setting off | Steam drops every rumble call; the panel can't see it | Steam → game → ⚙ → Controller settings: check rumble is on |
| Player LEDs on the Steam backend | Not possible: Steam has no player-LED call | Expected; light bar and rumble still work |
| DualShock 4 over Bluetooth on the Unity backend | Unity can't write to it, so we send nothing (one log warning) | Use Steam (Auto with Steam Input on) or a USB cable |
| DualSense just connected over Bluetooth | For the first ~6 s the pad plays its own blue connection light; we hold the light bar back (rumble works) | Expected: our colour appears after ~6 s |
| Window not focused | Motors stop on purpose | Click the game |
| Another app wrote to the pad earlier | The pad keeps the old colour until our next change | We re-send everything on every pad change and every 2 s on Steam |

## 7. Deep dive

### Files

| Class | Job |
|---|---|
| `RumbleService` | Public API; mixes and gates each frame |
| `RumblePatternMixer` | The 4 pattern slots |
| `RumbleHardwareWriter` | Player slider, rate limit, change-only sends, Steam light bar refresh |
| `RumbleDeviceBinder` / `RumbleDeviceWatcher` | Which pad: follows pad switches, unplug, focus, quit |
| `RumbleBackendSelector` | Which backend (the rules in section 5); switches off a failing feature |
| `SteamInputRumbleOutput` / `UnityInputSystemRumbleOutput` | The two backends (`IRumbleOutput`) |
| `SteamPadList`, `XInputSlot` | Which Steam pad is the one in the player's hands (one listed, XInput slot, or PlayStation type) |
| `SteamInputState`, `RumbleSteamReport`, `RumbleSteamLines`, `SteamInputDebugText`, `RumbleConnectedPads` | Steam Input on/off and fallback state; its F9 lines and Player.log block; every pad Unity sees |
| `DualSenseRawWriter`, `DualSenseOutputState`, `DualSenseHidOutputCommand`, `DualSenseBluetoothOutputCommand`, `DualSenseCrc32` | The raw DualSense report (Unity backend) |
| `DualSenseTriggerParamWriter` | Trigger data for Steam |
| `SteamOutputBoost` / `SteamImpactPunch` | Steam strength compensation |
| `RumbleDebugOverlay`, `IRumbleDebugSource` | F9 panel; how other systems appear in it |
| `RumblePadCheck`, `RumbleColorName` | The F9 verdict and "the pad should show now" lines |
| `RumblePadIdentity`, `RumblePadKind`, `RumbleConnection` | What the bound pad is (type, model, USB / Bluetooth). Built once per bind; family from `InputDeviceTracker.GetDeviceFamily` |
| `RumbleBluetoothHealth`, `RumbleBluetoothCheck`, `RumblePadTypeLine` | Bluetooth drops / silence tracking, the Bluetooth verdict, the coloured controller line |

### Raw DualSense report (Unity backend)

**Hardware-verified (Nov 2026):** this raw-report path gives full rumble and light bar on a DualSense over USB
and Bluetooth, and on Xbox wired and Bluetooth, with Steam Input off. (The Nov 2026 Steam Input regression is fixed,
see section 5; the Steam rows still need a hardware re-test.)

Unity's own DualSense code only covers rumble and the light bar over USB (its source says Bluetooth "is not
working"). So for a DualSense the Unity backend builds the pad's output message itself: one block of 47 bytes holding
motors, triggers, light bar and LEDs (`DualSenseOutputState`, same layout as the Linux driver), sent only when it
changes.

- **USB:** message id 0x02, 48 bytes.
- **Bluetooth:** message id 0x31, 78 bytes: the same 47 bytes moved by 2, plus a checksum (CRC-32) at the end that
  the pad uses to reject damaged messages.
- **Which one:** read from the pad's HID descriptor, its own description of its message size (USB says 48, Bluetooth
  78+). On Windows the descriptor can still be empty when the pad appears. It is then re-read once a second, up to
  10 times, and the format is corrected (one log line).
- **Always 78 bytes over Bluetooth.** Unity's hardware-tested fix (InputSystem PR #2436, not merged in our 1.19)
  found that Windows rejects the descriptor's 547-byte maximum.
- **If the pad refuses**, each step logs one warning:
  1. After 8 refusals over Bluetooth, retry once at the descriptor's size.
  2. Then try the other format (USB ↔ Bluetooth) once.
  3. After 15 more refusals, stop raw output for that pad. Rumble falls back to Unity's plain motor call, except over
     Bluetooth, where that call is skipped because it is USB-only.
- **Bluetooth connection light:** for the first 6 s after a Bluetooth bind the report leaves out the light bar and
  LEDs (rumble and triggers still go). Touching them during the pad's connection animation can wedge its firmware
  (also found by PR #2436). After that the hand-over is sent even if nothing changed.
- **LED hand-over:** the first writes tell the pad to stop its own start-up light show, otherwise it can ignore our
  colour.
- **Proof without a Bluetooth pad:** the self-test (F11, or `AgentScripts/ControllerHaptics/DualSenseBluetoothProbe.cs`)
  checks the checksum against the standard check value and that the real message bytes match the expected layout.

### Steam strength compensation

Steam's rumble is weaker than a direct write, its heavy motor most of all, and its trigger buzz has only 8 steps.
The Steam path alone lifts what it sends (gains, a response curve, a trigger floor and a short full-strength "punch"
on impacts); fields are on `RumbleOutputProfile`. Keep driving rumble clearly below full so impacts still stand out.
The player's slider is applied after this, so 50 % means half on both backends.

### Adding a backend

Implement `IRumbleOutput` (every method returns `false` on failure and never throws): `DisplayName`,
`Capabilities`, `TryBind(device)`, `SetMotors`, `SetLightBar`, `SetPlayerLeds`, `SetTriggers`, `Flush`, `Reset`,
`Unbind`, `Release`. Hand it in once with `rumble.SetPreferredBackend(output)` from a small installer next to the
service (see `SteamRumbleInstaller`). Optionally implement `IRumbleBackendStatus` so the rules and the F9 panel can
tell "not available", why the last bind failed (`SteamInputState`), how many pads the platform lists, "pad
reconnected", whether the pad gets the strength boost, and the platform's own details for the panel and the log.
