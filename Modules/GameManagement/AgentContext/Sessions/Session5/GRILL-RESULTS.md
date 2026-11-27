# Session 5 grill results: Game Management UI

Grilled with the owner on 2026-11-27 (questions Q1-Q25). Every answer below was agreed unless a change is noted.
"Confirm on stills" means the owner agreed but could not picture it from text; the prototype must show it and get
an explicit OK before Session 6 builds it.

Inputs: [research/EDITOR-UI-PRINCIPLES.md](research/EDITOR-UI-PRINCIPLES.md) (Unity Editor design rules),
[research/SCENE-SYSTEM-UI.md](research/SCENE-SYSTEM-UI.md) (the scene system's look, which every screen copies).

## Rules for every screen

- **Shared look (Q9).** The generic helpers in `SceneSystemUI` (`Section`, `StatusRow`, `Dot`, `Hint`, `Button`,
  `PrimaryButton`, `HelpBoxWithButton`, `ChoiceCard`, `Badge`, `PageHeader`, `Terminology`, `Logo`, `Separator`,
  colours) move to a new shared Editor assembly, **`AbstractPixel.Core.Editor.UI`**. Both modules use it. Scene-only
  members stay in the scene module. It's a move with no behaviour change at the start of Session 6, with the scene
  page's tests as the safety net. Converting it to USS `var(--unity-*)` tokens is later work.
- **Names on screen:** state, sub-state, parent, main state, rank, Requester, Reactor, problem, state list (the
  catalog). Never "priority", "register", "condition" or "trigger".
- **No strings configure anything:** object fields, pickers, enums and InputActionReferences only.
- **Problem shape (from the scene system):** a help box (Error or Warning) with a where/what/why sentence, no
  "Error:" prefix, one verb fix button with a tooltip saying what it changes, and a **Learn more** docs link. Each
  fix has Undo and writes one log line.
- Every card and window has a "?" docs button, as on the scene system. The layout holds at a 275 px Inspector width.

---

## 1. Project Settings > Abstract Pixel > Game Management

**Layout (Q1), numbered cards like the Scene System page:**

1. **Setup**: the settings asset.
2. **States**: the catalog list, its problems, the build check.
3. **Between scenes**: the no-state time scale.
4. **Input**: the input source.
5. **Debugger**: journal size and the release opt-in.

A problem that belongs to no state sits in the card it concerns. There's no separate problems card.

**1. Setup (Q2, Q11): the settings-asset picker, resolved**
- Row 1: `● Set up  [ObjectField: GameManagementSettings]`. Picking or dropping another asset calls
  `GameManagementSettingsAssets.Use` (Preloaded Assets).
- Row 2: `Asset path  [read-only path]  [Move…]`. Move… picks a folder inside Assets/.
- No asset: a yellow help box "No game management settings asset: states and checks are off." with a **Create
  settings** primary button. It creates the asset (and its catalog, Q3) in `Assets/GameManagement`.
- Not in Preloaded Assets: "Builds can't read these settings yet…" with **Fix now**.
- **Owner change:** no multi-asset "Keep this one" list and no prompt. The asset in the field is used; any other
  settings asset is ignored silently (what `Find` already does). The scene page has that list; this is a
  deliberate difference.

**2. States (Q3, Q4, Q5, Q10, Q15)**
- The catalog asset is separate but hidden. It's created with the settings, and one small read-only
  `Catalog [GameStateCatalog]` link sits at the top of the card. *(Confirm on stills.)*
- Header: `● 6 states, 1 problem` (green, yellow or red dot), a **Check again** button and "?".
- One reorderable list with drag handles. Row: `≡ | rank | State [object field] | Opens over | severity icon | Select`.
  *(Confirm on stills.)*
  - Main states are numbered 1…n (higher wins). "Opens over" reads "Main state".
  - Sub-state rows show muted **"sub"** instead of a number, and "Opens over: Main Menu, Pause" from `Parents`.
    Tooltip: "A sub-state opens over any of its parents; its place in this list doesn't matter." (This matches the
    code: only main states are compared by catalog order.)
  - The row's problems sit in help boxes under the row, as in `SceneGroupsDebugger`.
- One line above the list says that higher wins.
- Adding: **Add state…** (a picker of `StateSO` assets not yet listed), **New state…** (creates a `StateSO` in a
  States folder next to the settings and adds it at the bottom), and dragging `StateSO` assets onto the list. There
  is no "+ empty slot". The row's `−` removes the state from the list and never deletes the asset.
- Empty list: "No states yet." with **Set up states…**, which opens the onboarding window at "Which menus?" (Q10:
  one place asks which states and menus exist).
- After setup, a muted line under the list: "States use the sample's defaults. To change what a state does, select it
  and edit it in the Inspector."
- Toggle **Check states before a build**, on by default. Errors stop a build; warnings never do.

**3. Between scenes (Q6)**: a dropdown, "When no state is active": **Keep last time scale** (default) / **Set time to
1**. Muted hint: "Keep last stops loaded shared scenes running before the new level is ready."

**4. Input (Q7)**: a dropdown, "Input source": **None** / **Project-wide actions** / **Chosen asset**. The
`InputActionAsset` field shows only for Chosen asset. A warning shows in this card when Project-wide is selected but
the project has none, or Chosen asset has no asset.

**5. Debugger (Q8, Q23)**:
- `Journal size` (int, at least 8).
- **Show/hide debugger** (InputActionReference, empty by default).
- **Include the debugger in release builds** (toggle, off).

The last two fields are new; Session 8 adds them.

**States:** no asset → only the Setup card is active and the other cards are disabled with "Create settings first"
(an inference that follows Q2). Clean → `● N states, no problems` and nothing else.

---

## 2. Problem list (validator)

**Severity (Q12):** **Error** = the setup can never work as written (a state can never open), red. **Warning** = it
works but is messy, yellow. There's no info level.

**Problems (Q13, Q14):**

| Problem | Source | Severity | Text | Fix |
|---|---|---|---|---|
| Empty slot | `FindEmptySlots` | Warning | "Slot 4 in the state list is empty. It does nothing." | **Remove slot** |
| Duplicate | `FindDuplicates` | Warning | "Pause is listed twice. Only its higher place counts." | **Remove lower copy** |
| Missing parent | `FindParentsMissingFromCatalog` | Error | "Settings opens over Pause, but Pause isn't in the state list, so Settings can never open over it." | **Add Pause** (at the bottom) |
| Self-parent or parent loop | new (Session 6) | Error | "Settings lists itself as a parent, so it can never open." / "Settings and Options open over each other, so neither can open first." | **Remove parent** |
| Empty parent slot | new (Session 6) | Warning | "Settings has an empty parent slot. It does nothing." | **Remove slot** |
| Same parent twice | new (Session 6) | Warning | "Settings lists Pause as a parent twice." | **Remove copy** |
| State not in the list (Inspector and drawer only) | `Catalog.Contains` | Error | "Not in the state list, so every request for it is refused." | **Add to list** |

There's no Fix All. Text above is the target wording; Session 6 may tighten it while keeping where/what/why.

**Multi-parent conflict display, resolved (Q14):** a sub-state with several parents whose effects are "Same as
parent" behaves differently over each parent (Settings: time 0 over Pause, time 1 over Main Menu). That's the
intended design, **not a problem**. It shows as a read-only table in the state Inspector, **"What this state does
over each parent"**: one row per parent, with columns time / cursor / HUD / input maps holding the resolved values.
The only multi-parent problems are the three new rows above.

**Where problems show (Q15):** one source and three views:
- the Settings page row (an icon on the row, the help box under it, the header count);
- the state Inspector's Problems block (top, hidden when empty);
- the pre-build check (errors only, when the toggle is on). It names each state and where to fix it, with a log line
  prefix of `[Game management]` and the asset as context.

---

## 3. State Inspector (`StateSO`) and other components

**Layout (Q16):**
1. **Problems**, hidden when empty.
2. **Role line**, read-only: "Main state, rank 2 of 4" or "Sub-state, opens over Main Menu, Pause", plus an **Open in
   Project Settings** link (to that state's row). If the state isn't in the list: the red box with **Add to list**.
3. **Parents**: a list of state references. Tooltip: "Empty = main state".
4. **Effects**: four rows, Time scale / Cursor / HUD / Input. Each has a two-way switch, `[Same as parent | Own]`.
   - Own shows the value field. Same as parent shows the inherited value, muted.
   - Time scale on Own: a slider plus an **Ease** foldout (seconds, curve).
   - Input on Own: the InputActionReference list, each entry with its map name as a small chip.
   - **Q17:** on a main state the switch reads `[Keep last | Own]`, because no parent exists.
5. **What this state does over each parent**: the Q14 table, sub-states only.
6. **Advanced** foldout, closed by default: Execution delay and Leave on scene change.

Fields use PropertyField with Undo. Every field has a tooltip.

**Other components (Q18):** Requesters and Reactors keep Unity's stock inspector with tooltips and get no custom
inspectors. One shared **state field drawer** applies to every `StateSO` reference (Requesters, Reactors, Parents):
the normal object field plus a red icon with a tooltip when the state isn't in the list. *(Confirm on stills.)*

**States:** no settings asset → the role line becomes an info box, "States need game management to be set up.", with
**Open Game Management** (copies the scene inspector's pattern).

---

## 4. Onboarding window (UI Toolkit; it produces uGUI + TextMeshPro menus)

**Opening (Q19):** menu `Tools/External Project Tools/Game Management/Set Up Game Management`. It opens by itself once
per Editor session while no settings asset exists (never in batch mode, Play mode or tests), with a "Don't show
again" toggle stored per project. The Setup card can turn it back on.

**Look (Q19, Q25):** like `SceneSystemOnboardingWindow`:
- the Abstract Pixel logo; **under it, a pill header "Game Management"** (rounded, accent colour, small bold text) on
  every page;
- one question per page, `PageHeader` and choice cards;
- the footer: "Step N of M", Back on the left, Next on the right.

The pill is a shared `SystemPill` in `AbstractPixel.Core.Editor.UI`, and **the scene setup window gets a matching
"Scene System" pill** in the same Session 6 move (owner OK).

**Pages (Q20):**
1. **Welcome**: terms state, sub-state, Requester, Reactor.
2. **Settings folder**: default `Assets/GameManagement`, checked as you type ("Looks good." / a red status row).
3. **Which menus?**: choice cards with checkboxes for Main Menu, Pause (brings Settings), Game Over and Game Win.
   Play is shown ticked and greyed, "Always made". Each choice creates its state with the sample's defaults (Q10).
4. **How to set up each menu**: one page with one row per chosen menu, `(• Copy the library's menu  ○ Use my own)`.
5. **Input**: input source dropdown, plus the asset for Chosen asset.
6. **Ready**: what will be made, as green rows; a blocking problem shows as a warning box; "Change these later in
   Project Settings > Abstract Pixel > Game Management".
7. **All set**: for each "Use my own" menu, a short checklist and **Learn more**.

**The two paths (Q21):**
- **Copy the library's menu**: copies the library's graphics-free uGUI + TMP menu prefab to
  `<settings folder>/Menus/`, with its `StateMenuReactor` and Requesters already set to the new state.
- **Use my own**: the row shows an optional object field, "Your menu (prefab)", and the line "Needs extra setup: wire
  your buttons. **Learn more**". If you give a prefab, Finish adds a `StateMenuReactor` set to the state on its root;
  you wire the buttons yourself (`StateButtonRequester`). If you leave it empty, only the state is made and All set
  lists the steps.

**Running it again (Q22):** it opens at "Which menus?" with existing menus ticked and locked, to add a skipped menu
later.

---

## 5. Runtime debugger

**Two views of `GameStateMachine.Journal` (Q23):**
- **In-game overlay** (UI Toolkit; Editor, Development Builds, and release only when opted in). Read-only and
  top-right, so it never takes input or the cursor. It runs on unscaled time and works at time 0. It has a
  semi-opaque dark background and tabular numbers. It shows:
  - the stack, `Play › Pause › Settings`, with suspended states dimmed;
  - the current effects: time, cursor, HUD, input maps;
  - the last refusal in yellow with its reason;
  - the last 8 journal lines.

  It's shown and hidden by the Debugger card's InputActionReference. In the Editor there's also a shortcut, F8 by
  default, that can be rebound in Unity's Shortcuts. The overlay shows the key hint.
- **Game State Debugger window** (Editor only, dockable). The full journal, newest first: time / Entered·Applied·
  Left·Refused icon / state / Requester / reason. It has a **Refusals only** filter, and clicking a row shows its
  details.

**Before/after effect diff, resolved (Q24):** **in scope, in a small form.** A row's details show only what changed,
for example "Time 1 → 0, Cursor Locked → Free, Input +UI −Player", plus the stack before → after, from
`TransitionRecord.EffectsBefore/After` and `StackBefore/After`. **Nice-to-have for Session 8:** a full side-by-side
compare of every effect.

**States:** empty journal → "No transitions yet." In a release build without the opt-in, nothing is built in.

---

## The three resolved questions

| Question | Answer |
|---|---|
| Multi-parent conflict display | Not a problem. It's a read-only "What this state does over each parent" table in the state Inspector. The only problems are self-parent or loop (Error), an empty parent slot (Warning) and the same parent twice (Warning). |
| Settings-asset picker field | Setup card: `● Set up [ObjectField GameManagementSettings]` + `Asset path [read-only] [Move…]`, Create settings, Fix now. There's no multi-asset list: other assets are ignored. |
| Debugger diff view | In scope as changed-only lines in the debugger window's row details. A full side-by-side compare is a Session 8 nice-to-have. |

## Open items

- **None undecided.** Q3, Q4 and Q18 are agreed but the owner wants to **confirm them on prototype stills** before
  Session 6 builds them.
- Facts for the prototype or Session 6 agent to check, not decisions: whether the library already ships base menu
  prefabs to copy (Q21), and the exact docs topic IDs for each problem's Learn more.
