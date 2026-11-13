# Controller Verification

How to tell, **pass or fail**, whether rumble, the light bar, the player LEDs and the adaptive triggers work on a
controller, in any game that uses this Rumble module. Covers the Editor, Development Builds and release builds.

How the system works: [README.md](README.md). Each game adds its own **state table** (what the pad should show in
each game state; section 4).

---

## 1. The 30-second check (Editor or Development Build)

Needs the `RumbleDebugOverlay` component next to the `RumbleService`.

1. Start the game and press any button on the pad.
2. Press **F9** once. A short panel appears:

   ```
   PLAYSTATION  DualSense (PS5)  ·  USB cable
   WORKING  the DualSense accepts our reports (Usb, 307 sent, 0 refused).

   THE PAD SHOULD SHOW NOW  (if it doesn't match, it's not working)
     Light bar:  ██████ purple  #8925F9
     Player LEDs:  all off
     Rumble:  off
     Triggers:  free
   ```

3. **Check the top line names your pad.** The first word is the controller type, in colour:

   | Top line | Colour | Means |
   |---|---|---|
   | **PLAYSTATION** DualSense (PS5) / DualShock 4 (PS4) · USB cable / Bluetooth (wireless) | blue | The game talks to the PlayStation pad directly; the connection is read from the pad itself |
   | **XBOX** Xbox-style pad (XInput) · connection not reported | green | An Xbox pad, or any pad acting as one. Windows doesn't say whether it's wired or wireless |
   | **STEAM** PS5 pad / Xbox pad / Steam Deck controls through Steam Input | greyish blue | Steam owns the pad and the game talks to Steam |
   | **GENERIC** *product name* | grey | Something else (Switch Pro, cheap HID pad): rumble at most |
   | **NO CONTROLLER** | grey | No pad bound (keyboard / mouse in use, or the pad is gone) |

   **A PlayStation pad that shows XBOX** means Steam Input or DS4Windows is disguising it. That's a setup **FAIL**
   unless you are testing that on purpose.

4. Read the verdict word on the second line:

   | Verdict | Means | Do |
   |---|---|---|
   | **WORKING** (green) | The game is sending, and the pad or Steam accepts it | Go to step 5 |
   | **WAIT** (yellow) | Normal for a moment: Bluetooth connection light (~6 s), window not focused, or the pad briefly refusing | Wait 10 s. If it's still WAIT, it's a **FAIL** |
   | **NOT WORKING** (red) | Something blocks output. The line says what and how to fix it | **FAIL**, unless it's a known limit (section 3) |
   | **WAIT · BLUETOOTH** (orange) | The pad is on Bluetooth and its link looks shaky: refusing reports, dropping, or silent | Do what the line says. If it's still there after 10 s, it's a **FAIL** |
   | **NOT WORKING · BLUETOOTH** (orange) | The pad is (or just was) there over Bluetooth, but the link or its reports failed | **FAIL**, unless it's a DualShock 4 without Steam Input (section 3) |

5. **Look at the pad.** Does it match *THE PAD SHOULD SHOW NOW*?
   - Light bar is the same colour → pass. Breathing colours dim and brighten, so the hex number keeps changing:
     match the colour name, not the number.
   - Player LEDs show the same count → pass.
   - Rumble: when it says *on*, you feel it → pass.
   - Triggers: when it says *resistance*, L2/R2 push back → pass.

   **WORKING + the pad matches = PASS.** If it says WORKING but the pad doesn't match, that's a **FAIL**: the game
   thinks it's sending, but the pad isn't showing it. Screenshot it (section 6).

Press **F9** again for the full details (for bug reports), and a third time to hide the panel.

## 2. Expected result per controller

**PASS** = the panel shows this verdict and the pad shows these features. Anything else is a **FAIL**.

| Controller | Steam Input for the game | Top line should say | Verdict should say | Light bar | Player LEDs | Triggers | Rumble |
|---|---|---|---|---|---|---|---|
| DualSense, USB | off | PLAYSTATION DualSense (PS5) · USB cable | WORKING … (Usb, … 0 refused) | ✅ | ✅ | ✅ | ✅ |
| DualSense, Bluetooth | off | PLAYSTATION DualSense (PS5) · Bluetooth (wireless) | WAIT ~6 s, then WORKING … (Bluetooth, …) | ✅ after ~6 s | ✅ after ~6 s | ✅ | ✅ |
| DualSense, USB or Bluetooth | on | STEAM PS5 pad through Steam Input | WORKING Steam owns the pad | ✅ | ❌ expected | ✅ | ✅ |
| DualShock 4, USB | off | PLAYSTATION DualShock 4 (PS4) · USB cable | WORKING the pad accepts what we send | ✅ | – | – | ✅ |
| DualShock 4, Bluetooth | off | PLAYSTATION DualShock 4 (PS4) · Bluetooth (wireless) | NOT WORKING · BLUETOOTH … Unity backend can't send to it | ❌ expected | – | – | ❌ expected |
| DualShock 4, USB or Bluetooth | on | STEAM PS4 pad through Steam Input | WORKING Steam owns the pad | ✅ | – | – | ✅ |
| Xbox, wired or Bluetooth | off | XBOX Xbox-style pad (XInput) | WORKING rumble only | – | – | – | ✅ |
| Xbox, wired or Bluetooth | on | STEAM Xbox pad through Steam Input | WORKING Steam owns the pad | – | – | – | ✅ |
| Generic / lookalike pad | either | XBOX (if it acts as one) or GENERIC | WORKING rumble only, or NOT WORKING no controller found | – | – | – | ✅ if it shows up as an Xbox pad |
| Steam Deck built-in controls | always on | STEAM Steam Deck controls through Steam Input | WORKING Steam owns the pad | – | – | – | ✅ (Deck haptics) |

✅ must work · ❌ expected = it can't work in this setup, not a bug · – the pad has no such feature

Also a **FAIL** in any row: an error or exception in the Console or Player.log, or the same warning repeated over
and over.

Tested on hardware so far (Nov 2026): DualSense USB (blue PLAYSTATION line confirmed) and an Xbox-style lookalike
pad. The XBOX, GENERIC and Bluetooth lines have been rendered with fake devices only. STEAM has only been checked as
text. All other rows still need real hardware.

## 3. Reading NOT WORKING and WAIT

| The panel says | Pass / fail | What to do |
|---|---|---|
| WAIT … connection light … starts in N s | Normal on Bluetooth | Wait. Should turn WORKING within ~6 s |
| WAIT … window is not focused | Normal | Click into the game |
| WAIT … refusing reports right now | Normal if it clears in a few s | If it stays more than 5 s: **FAIL**, screenshot |
| NOT WORKING no controller found | **FAIL** (or no pad connected) | Press a button on the pad. If it's still there, close DS4Windows / DSX / HidHide |
| NOT WORKING · BLUETOOTH … Unity backend can't send to it | Expected for DualShock 4; **FAIL** for DualSense | Turn Steam Input on, or use USB |
| NOT WORKING · BLUETOOTH … disconnected N s ago | **FAIL** (Windows lost the link) | Turn the pad off and on, or re-pair it; USB always works |
| NOT WORKING · BLUETOOTH … refused every send / every report | **FAIL** | Turn the pad off and on; close DS4Windows / DSX; screenshot F9 details and F10 |
| WAIT · BLUETOOTH … refusing our Bluetooth reports | Normal if it clears in a few s | If it stays more than 5 s: **FAIL**, screenshot F9 details |
| WAIT · BLUETOOTH … dropped N times in the last minute | **FAIL** (unstable link) | Move closer to the PC / adapter, away from USB 3 ports, or use USB |
| WAIT · BLUETOOTH … no data from the pad for N s | Press any button | If it clears: fine, the pad was just resting. If it stays while you press: **FAIL** (link stuck) |
| NOT WORKING the pad refused every send | **FAIL** | Turn the pad off and on; close DS4Windows / DSX; screenshot |
| NOT WORKING the DualSense refused our reports in every format | **FAIL** | Screenshot both F9 views and F10 |
| NOT WORKING LightBar (or other) failed … switched off | **FAIL** | Screenshot; turn the pad off and on to retry |
| NOT WORKING backend mode is Off | Setting | Backend Mode on the `RumbleOutputProfile`, or remove `-rumble-backend=off` |
| WORKING rumble only … on a PlayStation pad | **FAIL** (setup) | Steam Input or DS4Windows is disguising the pad as an Xbox pad: switch to Auto, or turn Steam Input off |

**How the Bluetooth check works.** It only looks at pads that report a connection. Today those are PlayStation
pads, whose HID descriptor states the report size: 78 bytes or more means Bluetooth. Xbox and Steam pads say
"connection not reported", so they never get a Bluetooth verdict. The check is based on these signals:
1. **Dropped:** the pad was bound over Bluetooth, and Windows removed it.
2. **Refused:** the pad or driver refuses our reports (counted on every write).
3. **Known limit:** a DualShock 4 on Bluetooth gets nothing from the Unity backend.
4. **Unstable:** two or more drops within a minute.
5. **Silent:** no input report for 5 s. This is only a hint, because a resting pad may send nothing; that's why
   it asks for a button press.

The Details view (F9 twice) shows the raw numbers under *Controller → Bluetooth link* and *Bluetooth flag*.

## 4. The game's state table

Section 1 proves output reaches the pad. To prove it's the **right** output, each game keeps a table next to its
feedback code. The table gives, for each game state (menu, gameplay, hit, pause, game over, win…), the expected light
bar colour, player LEDs, rumble and triggers. While testing, compare *THE PAD SHOULD SHOW NOW* with that row.

Rules for every game:
- **Steam backend:** ignore the player LEDs (Steam can't set them).
- **DualShock 4:** ignore the player LEDs and the triggers.
- **Paused:** game-time rumble stops, and UI rumble (*Use Unscaled Time* patterns) still plays.
- **Window not focused:** all motors are off, and the light bar stays.

## 5. Full checklist (per controller)

Do section 1 at each step and compare with the game's state table.

- [ ] Every menu screen, then start gameplay.
- [ ] Trigger each gameplay moment that has feedback (hit, special events).
- [ ] Pause, then resume.
- [ ] Reach the lose and win screens.
- [ ] Alt-tab away during gameplay (WAIT … not focused, rumble stops), then come back (WORKING).
- [ ] Turn the pad off and on mid-game: WORKING again within ~6 s, and the colour comes back.
- [ ] DualSense: switch USB ↔ Bluetooth; the verdict shows the new connection.
- [ ] Settings: vibration 0 % → *Rumble: off* during gameplay. Triggers off → *Triggers: free*.
- [ ] Two pads (e.g. Xbox + DualSense): the panel follows the pad you last pressed a button on.
- [ ] Quit: no errors in the Console or Player.log.

## 6. Where to run it, and what to send when it fails

| | Editor | Development Build | Release build |
|---|---|---|---|
| F9 panel | yes | yes | **no** (it switches itself off) |
| Log | Console, filter `[Rumble]` | Player.log | Player.log |
| Backend in Auto | always **Unity** | Steam if Steam Input is on | Steam if Steam Input is on |
| Force a backend | *Backend Mode* on the `RumbleOutputProfile` asset (set it back to Auto afterwards) | launch option `-rumble-backend=unity` / `steam` / `auto` / `off` | same launch option |

- **Development Build:** *File → Build Profiles →* tick **Development Build**, then build. Give this build to remote
  testers.
- **Player.log** (Windows): `%USERPROFILE%\AppData\LocalLow\<Company Name>\<Product Name>\Player.log`. Both names are
  in Project Settings → Player. `Player-prev.log` holds the run before.
- **Release build pass/fail (no panel):** check the state table by eye and feel. Then check Player.log:
  - Each bind writes `[Rumble] Controller: <Type> - <model>, connection <Usb/Bluetooth/Unknown> (<how it was read>)`.
    This is the same as the panel's top line.
  - A lost Bluetooth link writes `[Rumble] Bluetooth: … disconnected (N drop(s) in the last minute)`.
  - Then find the `[Rumble] Device: … -> Backend: …` line. The backend must match section 2, for example
  `Unity (DualSense Bluetooth)` or `Steam Input (…PS5…)`. There must be no `Exception` and no `[Rumble]` warning. The
  README (section 6) explains each warning.
- **Steam Input setting:** Steam → Library → the game → ⚙ → Properties → Controller. Launch from the Steam library
  when testing the Steam Input rows.
- **Before testing:** close DS4Windows, DSX and HidHide, and keep only one pad on (except for the two-pad check).

**When something fails, send:**
1. Screenshots: F9 (short), F9 again (details), then F10 (bytes).
2. Player.log.
3. One line: pad, USB or Bluetooth, Steam running yes/no, Steam Input on/off, the game state, and what the pad showed
   instead.

**Steam Deck:** the built-in controls always go through Steam, so expect *WORKING Steam owns the pad*. Open F9 with
Steam + X (on-screen keyboard). The log:
- Windows build (Proton):
  `~/.local/share/Steam/steamapps/compatdata/<appid>/pfx/drive_c/users/steamuser/AppData/LocalLow/<Company>/<Product>/`
- Linux build: `~/.config/unity3d/<Company>/<Product>/`

## 7. Simulation, for whoever changes this module

Fake Input System devices built with Sony's vendor id (0x054C) and a product id (0x0CE6 DualSense, 0x09CC
DualShock 4) get the real HID classes. With a Bluetooth descriptor (`outputReportSize` 547), they test detection, the
6 s light hold and the refusal chain. A fake device refuses every write, so this proves fallbacks and logging only;
only real hardware proves the pad lights up. F11 (or `DualSenseBluetoothSelfTest.Run`) checks the CRC and the
Bluetooth report layout without any pad.
