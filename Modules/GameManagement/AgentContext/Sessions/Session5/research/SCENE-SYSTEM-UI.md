# Scene system editor UI: visual language for GameManagement to copy

Read-only research. Paths are relative to `LocalPackages/AbstractPixelLibrary/` (`SM` = `Modules/SceneManagement`; the folder is spelled `SceneManagement` now, not `SceneManagment`). Scratch docs: `ProjectRallyShift/.scratch/scene-coordinator/` (`plan.md`, `session-3c-report.md`, `session-d-progress.md`).

## Key finding first

The scene system's settings UI is **UI Toolkit built in C# with inline `element.style.*`, not USS/UXML.** There are **no UXML files** in the scene module. The only scene-module USS is `SM/Editor/ScenePicker/ScenePicker.uss`. The shared look lives in one static C# class, `SceneSystemUI` (`SM/Editor/Settings/SceneSystemUI.cs`), whose colours are hard-coded `Color` values chosen by `EditorGUIUtility.isProSkin`. Only the docs window (`EditorModules/Documentation/Styles/*.uss`) follows the "USS only, `var(--unity-*)`" rule of `EditorModules/Documentation/EDITOR-DOCS-UI-GUIDE.md`. GameManagement can copy the look two ways: call `SceneSystemUI` helpers (needs a reference to the SceneManagement Editor asmdef, or a move of the helpers to a shared Editor module), or redo the same values as USS. The guide's rules are the better target for new code.

## Surfaces

| Surface | Toolkit | File |
|---|---|---|
| Project Settings page "Scene System" | UI Toolkit (`activateHandler`) | `SM/Editor/Settings/SceneSystemSettingsProvider.cs` |
| Scene Groups Debugger (a section of that page) | UI Toolkit | `SM/Editor/Settings/SceneGroupsDebugger.cs` |
| Preferences page "Scenes" | UI Toolkit | `SM/Editor/OpenWithGroup/SceneOpeningPreferencesProvider.cs` |
| Setup / onboarding window | UI Toolkit EditorWindow | `SM/Editor/Settings/SceneSystemOnboardingWindow.cs`, `SceneSystemSetupView.cs` |
| Scene group inspector | UI Toolkit `CreateInspectorGUI` | `SM/Editor/SceneGroupEditor.cs` |
| Scene field drawer (reference picker) | UI Toolkit `CreatePropertyGUI` plus IMGUI `OnGUI` fallback | `Core/Editor/SceneDataTypes/SceneFieldPropertyDrawer.cs`, `SceneStatusBox.cs` |
| Scene picker popup (toolbar) | UI Toolkit `PopupWindowContent`, `ListView`, USS | `SM/Editor/ScenePicker/ScenePickerPopup.cs`, `ScenePicker.uss` |
| Scene set removal window | UI Toolkit EditorWindow | `SM/Editor/Settings/SceneSetRemovalWindow.cs` |
| Relocation inspectors, spawn point overlay | UI Toolkit, reuse `SceneSystemUI` | `SM/Relocation/Editor/SpawnPointEditor.cs`, `RelocatableEditor.cs` |
| In-editor docs window | UI Toolkit plus USS | `EditorModules/Documentation/**`, content in `SM/Editor/Documentation/**` |

IMGUI appears only in the scene field's `OnGUI` fallback (for IMGUI custom editors) and in the "?" tab-bar and title-bar buttons, which Unity draws in IMGUI.

## 1. Project Settings page "Scene System"

- Path `Project/Abstract Pixel/Scene System`, `SettingsScope.Project`, `[SettingsProvider]` with a keyword set (`SceneSystemSettingsProvider.cs:62-67`). Opened by `Open()`, `OpenAt(section)`, `OpenAtGroup` (scrolls and highlights a row).
- Top to bottom: page title row (`PageTitleWithHelp`: 19 px bold title, spacer, "?" button), then eight numbered sections: Setup, Start-up, Scene sets, Loading scenes, New scenes, Scene Groups Debugger, Sample scene groups, Relocation. Each is `SceneSystemUI.Section(parent, title, tooltip, helpTopicId)`: 14 px bold header whose explanation is its **tooltip** (header tints blue on hover so the tooltip is discoverable), a "?" at the right, then a `Card` body. The debugger header also holds a status summary and a "Check again" button.
- Scene sets are collapsible cards (`SetCard`): the default set has a green outline with a thick left edge and a "Chosen default scene set" badge. Click selects a card (`ShowSelected`). + and - footer under the list (`AddRemoveFooter`), "Make default" button, muted usage count, a "Scene groups using it" foldout of asset links. Rename: + creates "New Scene Set" with its name as a text field; Enter or blur renames, Esc keeps, an empty or taken name shows a red line (5A2 decision 16).
- Settings asset shown as a read-only path box (`AssetPathField`, click pings) with a "Move..." button; asset references use `ReadOnlyObjectField` (picker hidden, drop reverted).
- Empty states: one muted `Hint` saying what to do ("Available once the scene system is set up.", "No loading scene yet: the setup clones a ready-made one, or add your own with +."), or a yellow `StatusRow` ("No scene sets yet. Click + to make one.", "No default scene set: a new scene group starts without one. Click Make default on a scene set.").
- Not set up: yellow "Not set up yet" row plus a Warning `HelpBox` ("Settings asset missing: ... Click Get started below to create one.").
- Problem examples: Warning `HelpBox` plus "Fix now" for "Builds can't read these settings yet: the settings asset is missing from Player Settings > Preloaded Assets ... The next build adds it back, or fix it now." (`:349`); duplicate settings assets get a Warning box and a "Keep this one" button per asset (`:436-445`).

## 2. Scene Groups Debugger

- `SceneGroupsDebugger.Fill` (`SceneGroupsDebugger.cs`), a section of the page above.
- Inside the card: "Check scene groups before a build" `Toggle` (saved in the settings asset, Undo-recorded), an optional migration `HelpBox(Info)` with "Preview migration" and "Migrate", then either one `Hint` when empty ("No scene groups yet. Right-click a scene in the Project window and pick Create Scene Group.") or a bold header row and one **block per scene group**: the row, then that group's problems directly under it.
- Row columns: "1." number (muted, right aligned), group name as asset link (click selects), main scene `ObjectField` (drag or picker, no string ids), scene set `PopupField<SceneSet>` (None plus every set), status cell, then two fixed 92 px buttons "Select group" and "Open group". Columns have width, grow, min and max, shared by header and rows so they line up. Under 1100 px card width the buttons move under the row (`OnCardResized`, `ApplyLayout`).
- Summary next to the section title: `StatusRow` "16 groups, 0 problems", green at 0, red otherwise.
- Status cell: green dot "No problems", red dot "N problem(s)", yellow dot "Boot group, unused". One line with ellipsis, full text in the tooltip.
- Problems reuse `SceneGroupProblemsView` (same view as the inspector), indented to sit under the group name.
- Every edit goes through the same code as the inspector (Undo, overrides, rename tracking).

## 3. Preferences page "Scenes"

`SceneOpeningPreferencesProvider.cs`. Path `Preferences/Abstract Pixel/Scenes`, `SettingsScope.User`. Layout: 10 px left and 6 px top padding, `PageTitleWithHelp`, two `Toggle`s with the label forced to 320 px so checkboxes line up as on Unity's own pages, one muted `Hint` ("Play always loads them, so your managers run."), a third toggle (docs follow sample scenes). Each toggle has a tooltip and follows changes made elsewhere (subscribe on `AttachToPanelEvent`, unsubscribe on `Detach`). Values live in `EditorPrefs` keyed with `PlayerSettings.productGUID` because EditorPrefs are shared by all projects (`SceneOpeningPreferenceStore`, `EditorPrefsBoolStore`, `IBoolPreferenceStore` for tests). The same preferences also appear in setup, the Tools menu and the picker footer: one store, four views. No headings, cards or boxes: Preferences stays plain.

## 4. Setup / onboarding window

- `SceneSystemOnboardingWindow` (`[InitializeOnLoad]`, menu `Tools/External Project Tools/Scene System/Set Up Scene System`). Opens **by itself once per Editor session** when no settings asset exists (never in batch mode, Play or tests; queued with `delayCall` then `EditorIdleQueue`). "Don't show again" (10 px toggle) is stored per project in `EditorUserSettings`; the Setup section can turn it back on. Min 460x520, max 690x780, 14 px side padding, "?" in the tab bar via `ShowButton`.
- Body `SceneSystemSetupView`: Abstract Pixel logo (`Resources/AbstractPixelLogo`), centred `Separator`, one question per page, footer with muted 10 px "Step N of 7", Back (100 px) left, Next (120x24, bold) right. Pages: Scene System (welcome), Settings Folder, New Scenes, Launch Scene, Loading Scene, Scene Sets, Opening Scenes, Ready, All set. Running it again after setup shows Opening Scenes only, with Done.
- A page = `PageHeader` (20 px bold centred title plus one muted line), an optional **Terminology** block (muted bold heading, 1-3 grey 11 px lines, the word in bold italic, an 8-15 word definition from `SceneSystemTerms`; a word is explained once and later steps carry it as a tooltip), then `ChoiceCard`s: radio dot, bold title, one grey line, tooltip with detail, selected card has a 2 px blue border, unavailable cards are disabled with the reason as their line ("Needs the Addressables package.").
- Folder step validates live: green "Looks good." or a red `StatusRow` with the problem. Ready step lists what setup will make as green rows; a blocking problem is a Warning `HelpBox`. "Change them later: ..." line tells where each setting lives afterwards.
- Logic is apart from the view (`SceneSystemSetup.cs`, plain `SceneSystemSetupChoices`) so it is testable. Decisions: `plan.md` 5A2 items 12-15 (~line 750-756).

## 5. Inspectors and drawers (reference pickers)

- **Scene group inspector** (`SceneGroupEditor.cs`): help row ("One place the player can be..." plus "?"), the shared problems view (titled "Problems", hidden when empty), a boot-group Info box, main scene field, scene set popup, loading scene popups, sample toggle, an "Advanced" foldout (state in EditorPrefs), then buttons ("Add scenes to Build Settings", "Move to Addressables", "Open Scene System"). A part that follows its scene set shows a grey outlined `Badge` "From set 'X'"; an edited part shows a bold white-on-blue `Badge` "Override" with "Revert to set". Without settings: Info `HelpBoxWithButton` "Scene sets need the scene system to be set up." plus "Open Scene System".
- **Scene field drawer** (`SceneFieldPropertyDrawer`): `SceneAsset` `ObjectField` (drag or picker, **no strings typed**), a disabled "Scene Name" `TextField`, then `SceneStatusBox`. The scene's name, GUID and path are copied on pick (`CopySceneIdentity`) so a rename or move is survivable. Right-click: "Move to Addressables" and "Move to Build Settings" (greyed when impossible; the label says "(needs the Addressables package)"). It listens to `sceneListChanged`, `projectChanged` and Addressables changes so the box updates live. Binding raises change events (even None to None), so clearing is only honoured when the field showed that scene before (a 3C bug: opening the Inspector wiped a Missing scene's path).
- **Scene picker popup** (`ScenePickerPopup`): Scenes and Scene Groups tabs, search field, `ListView` of 20 px rows (number, icon, title, label button, 8 px status dot in a 20 px hit column, folder button), footer "Open with:" toggles, right-click menu. Enter opens, Shift+Enter selects.
- **Relocation inspectors**: `Card()` blocks, bold position count, "N reached by relocatables" as `Hint`, problems as `StatusRow` (yellow warning, red error), slope as muted text.
- **Scene set removal window**: Warning `HelpBox` ("This scene set is used by N groups. Choose what happens to them, then remove the set."), `ChoiceCard` options, red `HelpBox(Error)` for an invalid choice, muted "This can't be undone: the set moves to your system's trash.", Cancel plus `PrimaryButton`.

## 6. In-editor docs

`EditorModules/Documentation/` is a **generic engine** (own asmdef; `Rendering/`, `Window/`, `Styles/`); the scene module supplies content (`SM/Editor/Documentation/Content/`, Markdown pages) and registers it (`SceneDocumentationRegistration`). The module contract is `SceneSystemDocs.Open(topicId)` (a `Handler` the docs assembly sets, so the module needs no reference to it) plus `SceneSystemDocTopics` string constants (`SM/Editor/Help/`). Every surface has a "?" from `SceneSystemHelp`, a thin wrapper over `DocsHelpButtons` (class `docs-help-button`, `_Help` icon). Placement: page title row, section header, EditorWindow tab bar (`ShowButton`), page title bar (`titleBarGuiHandler`, not drawn by 6.3 for UI Toolkit pages, so the title row has one too). Docs window: toolbar (Back, Forward, Home, crumbs, "On this page", search) and a `TwoPaneSplitView` with sidebar tree and a 680 px page column. Test `SceneDocsHelpTests` checks every Open/Help call uses a constant and every `SceneGroupProblemKind` maps to a "Find a problem" entry.

## Problem UI: how warnings and errors show

1. **One shape everywhere**: Unity `HelpBox` (Error, Warning or Info) with the message, one fix `Button` at the right (`align-items: center`, `flex-shrink: 0`), and a "Learn more" link button (`SceneSystemHelp.LearnMore`, link blue). Built in `SceneGroupProblemsView.MakeRow` and `SceneSystemUI.HelpBoxWithButton`.
2. **Severity**: Error means it stops a build (every scene problem). Warning means shown but never blocks (relocation setup mistakes, `BlocksBuild = false`). Info is neutral guidance (migration, boot group, setup missing).
3. **Dots for state, boxes for problems**: `StatusRow` is a 10 px round dot plus text: green fine, yellow needs attention, red broken. Used in summaries, validation lines and small empty states.
4. **Scene status box** (`SceneStatusBox`): `HelpBox` class on a custom row, a 16 px square coloured by `SceneStatusColours` (green 50% in build, blue Addressable, grey never in build, red not in build or missing), one rich-text line, one fix button ("Add to build", "Make Addressable") hidden when there is no fix.
5. **Wording** (`Core/Editor/SceneDataTypes/SceneStatusRules.cs:68-101`, `SM/Editor/Checks/SceneGroupCheck.cs`): "Scene group 'X': scene 'Y' in Manager scenes is not in build, so it can't load." / "... is missing. There is no scene asset at 'path'. Pick the scene again." / "Scene group 'X' has no main scene. Pick the scene that makes this place." Pattern: where (group, scene, list), what, why it matters, what to do; no "Error:" prefix. Fix labels are verbs: "Add to build", "Make Addressable", "Remove from group", "Select", "Fix now", "Keep this one". Every fix has a tooltip saying what it changes ("Removes this entry from the scene group. Undo brings it back.").
6. **Same problems in three places** from one source (`SceneGroupCheck.Find` giving `SceneGroupProblem`): inspector, debugger, pre-build check. The build failure names every group and scene and where to fix: "Fix them in each scene group's Inspector or in Project Settings > Abstract Pixel > Scene System > Scene Groups Debugger, or turn off Check scene groups before a build on that page." Log lines use the `[Scene system]` prefix and pass the asset as context so a click selects it.
7. **Fix result** is one log line ("Scene group 'X': removed 'Y' from Shared scenes.") and recorded with Undo.
8. **Chips in lists**: the picker's red, green or blue dot has a tooltip with the first problem and count; a click goes where it is fixed.
9. **Destructive steps** say so plainly (removal window text above) and offer named choices first.

## Visual language summary

| Aspect | Scene system value | Source |
|---|---|---|
| Page title | 19 px bold, "?" at the right of the title row | `SceneSystemUI.PageTitle` |
| Section header | 14 px bold, 14 px above, 5 px below, explanation in tooltip, blue tint on hover | `Section` |
| Section body (card) | dark 0.20 grey (light 0.87), 1 px border 0.13 (light 0.66), radius 5, padding 8 and 10 | `Card()` |
| Inner card, list footer | 0.235 grey (light 0.92) | `InnerCardBackground` |
| Selected card | 0.24/0.29/0.35 (light 0.76/0.83/0.92) | `SelectedCardBackground` |
| Default accent | green outline 0.40/0.68/0.44 (light 0.20/0.50/0.26) | `DefaultCardBorder` |
| Status colours | green 0.30/0.75/0.36, yellow 0.96/0.74/0.19, red 0.89/0.29/0.24; 10 px dot | `Green`, `Yellow`, `Red` |
| Muted text | 0.62 grey (light 0.30); hints 11 px | `Hint`, `MutedText` |
| Override and selection blue | 0.17/0.52/0.92 | `OverrideBlue` |
| Link blue | 0.30/0.49/1 (light 0.10/0.33/0.80) | `LinkBlue` |
| Primary button | the normal button tinted blue 0.27/0.35/0.45 (light 0.79/0.86/0.95) | `PrimaryButton` |
| Radii | 5 cards, 6 choice cards, 3 badges and fields, 2 flat buttons | various |
| Spacing | 3 px between status rows, 14 px between sections, 4-6 px between fields | `SceneSystemUI` |
| Fonts | Unity's Inter only; bold for headings and names; sizes 20 wizard, 19 page, 14 section, 12 body, 11 hint, 10 caption | |
| Icons | built-in only, loaded with `EditorGUIUtility.FindTexture` (`_Help`, `console.warnicon.sml`, `console.infoicon.sml`, `Valid`); flat text glyphs on chips; status is a drawn dot or square, not an icon | `DocsIcons`, `ScenePickerIcons` |
| Severity | Error, Warning, Info HelpBox plus green, yellow, red dot; red, grey, blue status square for scene status | above |
| Empty state | one muted sentence saying what to do; no warning icon (guide); sometimes a yellow dot row | guide, `Hint` |
| Wording | sentence case, "you", verb buttons, tooltip on every control, glossary terms, no "Error:" prefix | guide "Wording" |
| USS tokens (docs window, picker) | `var(--unity-colors-*)`, `--unity-metrics-default-font_*_size`; own tokens in `DocsDark.uss` and `DocsLight.uss` | guide "Colours" |

## Reusable pieces

- `SM/Editor/Settings/SceneSystemUI.cs`: `PageTitle`, `PageTitleWithHelp`, `Section` (4 overloads), `Card`, `Hint`, `StatusRow`, `Dot`, `Row`, `Spacer`, `Button`, `PrimaryButton`, `PingAndSelect`, `AssetPathField`, `ReadOnlyObjectField`, `AssetLink`, `Separator`, `SetCard`, `ShowSelected`, `AddRemoveFooter`, `Badge`, `ChoiceCard`, `PageHeader`, `Terminology`, `HelpBoxWithButton`, `Logo`, colours `Green`, `Yellow`, `Red`, `TextColour`, `LinkBlue`. Caveat: namespace `AbstractPixel.SceneManagement.Editor`, scene-specific asmdef. Scene-only members (`SceneSetPopup`, `SceneField`, `SceneList`, tooltip constants) must stay behind if the rest moves to a shared module.
- `SM/Editor/Checks/SceneGroupProblemsView.cs`, `SceneGroupProblem.cs`, `SceneGroupProblemFix.cs`, `SceneGroupProblemFixer.cs`: problem/fix data shape and view. Generalise to `Problem {Kind, Message, BlocksBuild, Fix, Target}` plus a fixer with `FixLabel` and `FixTooltip`.
- `Core/Editor/SceneDataTypes/SceneStatusBox.cs`, `SceneStatusColours.cs`, `SceneStatusRules.cs`, `SceneStatusReader.cs`: the "pure rules, reader, box" split (rules are pure and unit-tested; the reader touches the project).
- `SM/Editor/Help/SceneSystemHelp.cs`, `SceneSystemDocs.cs`, `SceneSystemDocTopics.cs`: wrapper pattern over `EditorModules/Documentation/Window/DocsHelpButtons.cs` (`Button`, `LearnMore`, `HelpRow`, `TabBarButton`, `TitleBarButton`; classes `docs-help-button`, `docs-learn-more`, `docs-help-row`).
- Docs engine styles: `EditorModules/Documentation/Styles/Docs.uss`, `DocsDark.uss`, `DocsLight.uss`, `DocsNewLayout.uss`. Classes: `docs-callout` (`-note`, `-warning`, `-tip`, `-why`), `docs-pill-0..3` (ladder pills), `docs-chip`, `docs-step` (`-done`, `-current`), `docs-code`, `docs-table`, `docs-kind-*`, `docs-status-*`. C# elements: `DocsCallout`, `DocsChip`, `DocsPill`, `DocsStepList`, `DocsCodeBlock`, `DocsGraphView`, `DocsIcons`, `DocsTheme`.
- `SM/Editor/ScenePicker/ScenePicker.uss`: `.scene-picker`, `__tabs`, `__search`, `__list`, `__empty` (60% opacity), `__footer`, `__message` (warning text colour), `.scene-picker-row__*` (flat hover-only buttons, status dot, heading row, `--cannot-open` at 50%). Good template for any list popup.
- `Core/Editor/EditorIdleQueue.cs`, plus the onboarding pattern (auto-open once per session, per-project "Don't show again" in `EditorUserSettings`).
- Preference store pattern: `IBoolPreferenceStore`, `EditorPrefsBoolStore`, `SceneOpeningPreferenceStore` (per-project key from `productGUID`, in-memory store for tests).

## Decisions about problem/error UI (source refs)

- **Priorities for editor work: stability, then ease of use ("fewest clicks and least room for error; mistakes show in the Scene view and the group check before Play"), then readability ("understood at a glance")**. `plan.md:1012` (user, 2026-11-21).
- **Fix a problem where you see it, in one click.** Scene field gets one status box and one fix button (`plan.md` AD3, ~172-174). Status stays in one place, in Unity's help box style (`session-3c-report.md:22`).
- **Group check with fix buttons, "check all", optional pre-build check** (E4, `plan.md:198`, 858-867). The build failure names every group and scene (`session-3c-report.md:52`).
- **Listed twice is an error that stops a build** (Q5, `plan.md:875`). Relocation setup mistakes are warnings that never block (`SceneGroupProblem.BlocksBuild`).
- **Polish**: "close to Unity's look but not plain: section headers, grouping, status indicators, warnings" (`plan.md:881`). Relocation look: "Tasteful, little text, one format, a clear priority", reusing `SceneSystemUI` helpers and colours (`plan.md:1066`).
- **Wording**: "is included in build settings" became "is in build (Build Settings)" (`session-3c-report.md:42`); messages end with what to do ("Pick the scene again."). Setup option lines say what picking does in plain words; Terminology lines are 8-15 words in glossary wording (`plan.md` 5A2 decision 15). Banned _Avoid_ words from `GLOSSARY.md`; "scene set", never "scene preset".
- **A click goes to where the problem is fixed**: red chip opens the group in the Inspector (same problems view and fixes), green goes to the debugger row; the tooltip's second line says what a click does (5A2 decisions 4-7, `plan.md:745-748`).
- **Help at the point of need, pull not push** (guide principles 1-3): every window, section, inspector and problem has a "?" or "Learn more" to its docs section (`plan.md:1144`); a test enforces that every `SceneGroupProblemKind` has a "Find a problem" entry (`session-d-progress.md:88`).
- **Help box rule** (guide "Help boxes, tooltips and empty states"): below the control, under 200 characters, problem then cause then fix, no "Error" or "Warning" prefix, at most one fix button and one link. **Empty states** are never blank: one centred sentence-case line saying what to do, at most two starter buttons, no warning icon.
- **Disabled, never hidden**: about 40% opacity and a tooltip that says why (guide "Shapes and states"; done in `ChoiceCard`, `AddMoveItem`).
- **Designer-first**: nothing typed as a string id; everything is picked from an asset field, popup or the scene picker (`ObjectField` of `SceneAsset`, `PopupField<SceneSet>`).
- **Hardening walks do each workflow the way a designer does**, through the real buttons and menu items, never the method behind them (`plan.md:803`, 822-823).

## Cautions for GameManagement

- The guide says "style with USS, never `element.style`", but `SceneSystemUI` does the opposite with hard-coded colours. New code should use the guide's `var(--unity-*)` roles so the theme follows. The two colour sets differ slightly (card greys versus `--unity-colors-helpbox-*`), so pick one.
- On Unity 6.3 `HelpBox` has no built-in button; the library appends a `Button` child. `titleBarGuiHandler` is not drawn for a UI Toolkit settings page on 6.3.
- Grabbing window pixels crashes Unity 7000.0.0a16; judge the look from 6.3 stills.
- `SceneSystemHelp` depends only on a `Handler` delegate, so the docs assembly sits above the module. Copy that indirection, not a direct reference.
