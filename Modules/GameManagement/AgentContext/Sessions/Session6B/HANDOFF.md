# Session 6B handoff: state inspector look approved

`plan: gm-commercial, step 6B`. Written 2026-11-27. The owner called the **state inspector** design (main state and
sub-state) final. **No product code was written. The real inspector is not built.**

## 1. Long-name requirement (read before building the inspector)

**The real UI must accommodate long names, everywhere in this view:** the "What this state does over each parent"
panel and every name in it, input action map names, parent state names (including really long ones) and long text in
general. It is an implementation requirement. The owner asked for no further prototype change, so **the prototype does
not handle it fully and no screen has been checked with long names.**

The full rules are in `../../UI-STYLE.md`, "Long names (required in the build)". The handling there was written by the
agent at the owner's request; the owner has not reviewed it on screen. In short:

- 275-1200 px: no sideways scrolling or overlap, and no control pushed off, covered or squeezed.
- Names: one line, cut with "…", full name on hover. Sentences wrap and are never cut.
- Parent names: "Opens over" chips, parent object fields and the table's Parent column.
- Input action map names: the chip on each input action row, and the table's Input column (several maps, joined and
  cut; the tooltip lists every map).
- Table value cells and teal tags stay inside their columns; a changed cell's tooltip gives both full values.
- Never compare or save displayed text.
- **Done only when** checked at 275, 480, 760 and 1200 px with long test names (list in UI-STYLE.md) and every still
  has been read.

## 2. Sample data: what does not really work yet

| In the prototype | What the build must do |
|---|---|
| **Input is sample data.** Both sides of the table's Input column are the fixed text "UI", and so are the chips and the inherited "UI, from each parent" line. **The Input comparison does not work yet.** It has never been tested against different values. | Take each input action's map from the action. Work out each parent's maps. Compare which maps are on (order and repeats make no difference). Note that `GameEffects.SameMaps` compares in order. |
| Parent values for Time, Cursor and HUD are hard-coded for "Main Menu" and "Pause" (`StateParentSample.cs`). Any other parent shows `?` or the state's own value, with no teal. | Resolve each parent's values from the real assets, as `StateEffectResolver` does. |
| No row shows "keeps the last value". | A parent chain with no Own value has no fixed value. Show that. |
| No parent is itself a sub-state. | Such a parent can sit on different stacks, so one row may have more than one result. Decide how to show it. |
| The inherited lines under the switches are fixed text. | Build them from the resolved values. |

Input effect, as the code works today (`StateEffectResolver.MapsOf`, `UnityEffectSink`, `InputActionAssetSource`): each
listed action turns on its whole map; every other map in the input source is turned off; an empty list on Own turns all
maps off; with no input source the maps are left alone.

## 3. What the owner approved

The look is written down in `../../UI-STYLE.md`, "State inspector". Stills in `stills/` (all foldouts that matter
opened; the real views start collapsed): `03-state-inspector-sub-state-{480,760,1200}.png` and
`04-state-inspector-main-state-{480,760,1200}.png`. The stills use short sample names.

These replace parts of Session 5 `GRILL-RESULTS.md` §3:

- General info (type tag, Open in Project Settings, Opens over, Is sub-state, Parents, per-parent table) is above
  Problems. It replaces the role line, so the main state's rank is no longer shown there.
- "Effects" is called **Software Effects**.
- A new "Is sub-state" checkbox shows or hides the parent panels.
- Teal in the table marks values that really differ from the parent's.

The owner also chose (2026-11-27): until the developer has picked an Own value, switching to Own copies **parent 1's**
value. Window numbers: **03 is the sub-state and 04 the main state** (`OVERVIEW.md` lists them the other way round).

**Window 05, state field drawer:** the owner chose to **drop the separate look review**. It is not approved, was not
restyled, and has no 6B stills. Its look is decided during the build; the only spec is GRILL-RESULTS §3 (normal object
field plus a red icon with a tooltip when the state isn't in the list). Show the owner stills of it then.

## 4. Decide when building

1. **"Is sub-state" has no field in `StateSO`.** Today a state is a sub-state when `Parents` is not empty. Either work
   the checkbox out from the list (then a ticked box with no parents is not saved) or save a new field. In the
   prototype, unticking hides the parents but keeps them; the machine would still treat a state with parents as a
   sub-state, so the build must settle this.
2. **"Own value not chosen yet" is not saved.** The prototype remembers it only while the view lives. Either save a
   marker or copy parent 1 on every switch to Own, which overwrites an earlier choice. The owner's choice above needs
   the first, or the owner's OK for the second.
3. A sub-state with no parents, or "Keep last" parents: what does Own start from? The prototype keeps the stored value.

## 5. Not done and not verified

- The real inspector and the state field drawer (call it **6B-build**). It has no session prompt yet.
- Long names (section 1).
- Never checked: drag-reorder of parents, choosing a parent in Unity's object picker (set by script only), teal
  pressed and focus colours, hover, the look in Unity 6.3, widths below 480 px.
- Look only: "Open in Project Settings", "Learn more", the fix buttons and the input action `+` / `−` do nothing.
- The validator's new problem rules (GRILL-RESULTS §2) are not written.
- **Still open from Session 4B:** Rally EditMode tests and parity scenarios, and a clean rerun of the Lab sample's
  PlayMode flow test. Clear both before any product code changes.
- **Also still open: 6A-build** (the real Project Settings page, Session 6A handoff §4).

## 6. Prototype

Lab project, `Assets/GameManagementUIPrototype/`. **Untracked, never committed. Don't delete it:** it is the source of
truth for exact values. State inspector files: `UI/StateInspector.uxml` and `.uss`; in `Editor/`,
`StateInspectorPrototypeView.cs`, `StateEffectRows.cs`, `StateParentTable.cs`, `StateParentSample.cs`,
`StatePreviewSample.cs`, `StatePreviewSampleEditor.cs`, `StatePreviewWindows.cs`. Menu items 03 and 04 open real
floating Properties windows on in-memory samples, so Unity draws its own header.

Learned on the way:

- On the Unity 7 alpha, `HelpBox` has a `unity-help-box__top-container`; buttons added to the box itself drop to their
  own line. The view adds them to that container when it exists. Check the approved Settings page the same way.
- A Properties window builds its contents only once Unity repaints it. Scripted captures must bring Unity to the front
  first.

## 7. Next session

**Superseded (2026-11-27 replan).** The route is in `../../Wayfinder/map.md`. The watchdog moved after every UI build
(ticket 10). The long-name requirement and the real Input comparison are must-haves in ticket 06 (6B-build).

## 8. Budget

No budget was set for this session, so none was tracked and no warning fired.
