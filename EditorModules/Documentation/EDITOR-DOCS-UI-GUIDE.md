# In-Editor docs UI guide

How to build documentation windows for the library's modules inside the Unity Editor with UI Toolkit. The Scene System docs window is the first one. Read this before writing any docs UI code, USS or page layout.

Each rule cites its source in [square brackets]; the list is at the end. A rule marked **(our choice)** has no outside source and was decided for this library.

## Principles

1. **Pull, not push.** Help waits until the user asks for it. Nothing opens by itself except the first-run setup that already exists. Every docs entry point is a "?" button, a link or the menu item. [NN/g help], [NN/g onboarding]
2. **Help at the point of need.** Every Scene System window, inspector and page section links to the exact docs section about it. The user never searches for the page that explains the control in front of them. [NN/g help], [Animancer help]
3. **Jump to the cause.** When a page talks about an object, asset or field, a button selects or pings it. Showing beats describing. [PingObject], [Tutorial highlight]
4. **Every step shows a result.** A sample step says what to do and ticks itself when the scene system reports that it happened. [Diataxis tutorials], [Tutorial criteria]
5. **Keep the kinds of docs apart.** Samples and Get started are tutorials. Guide pages are how-to. Types, Code map and Find a problem are reference. "Why it works" blocks are explanation and stay short, linked rather than inlined. [Diataxis]
6. **Layer the detail.** Each page shows the short version first: a lede and the steps. "Why it works" sits in a closed foldout, and full detail is one link away in Reference. Nothing is hidden that a reader needs, and nothing is shown before they need it. [NN/g onboarding], [Diataxis]
7. **Write for a Unity developer new to the module.** They know scenes, the Inspector and Play mode. They do not know this module's words, so every term links to its definition on first use in a section.
8. **Look like Unity first.** Use Unity's controls, variables, icons and spacing. Add our own styling only where Unity has nothing for the job (pills, code blocks, step lists, timelines). [Foundations colour], [Foundations content organization]

## Matching Unity's look

### Colours

Use only `var(--unity-colors-*)` for anything Unity already has a role for. Never write a hex value for those roles. The Editor loads its dark or light sheet into every panel, so these variables follow the theme for free. [Foundations colour], [UIElementsEditorUtility]

| Role in the docs window | Variable |
|---|---|
| Window and content background | `--unity-colors-window-background` |
| Sidebar and code block background (one step darker) | `--unity-colors-default-background` |
| Toolbar | `--unity-colors-toolbar-background`, `--unity-colors-toolbar-border` |
| Body text | `--unity-colors-default-text` |
| Muted text (captions, line numbers) | `--unity-colors-label-text` at 60% opacity |
| Borders and dividers | `--unity-colors-default-border` |
| Selected tree row, arrival flash | `--unity-colors-highlight-background` |
| Links | `--unity-colors-link-text` |
| Callout background and border | `--unity-colors-helpbox-background`, `--unity-colors-helpbox-border` |
| Alternating table rows | `--unity-colors-alternated_rows-background` |
| Section header bars | `--unity-colors-inspector_titlebar-background`, `--unity-colors-inspector_titlebar-border` |
| Error and warning text | `--unity-colors-error-text`, `--unity-colors-warning-text` |

Variable values for both skins are in [USS variable reference].

Colours Unity has no role for (ladder pills, syntax colours, step states) are our own tokens. Define each one twice, in `DocsDark.uss` and `DocsLight.uss`, and load the sheet that matches `EditorGUIUtility.isProSkin`. [UIElementsEditorUtility] Keep text contrast at 4.5:1 and icons and borders at 3:1 in both skins. [Foundations accessibility]

### Type

- Keep Unity's font (Inter) for everything except code. Set no font on prose. [Foundations typography]
- Sizes come from the metric variables only:

| Use | Variable | px |
|---|---|---|
| Page title | `--unity-metrics-default-font_very_big_size` | 19 |
| Section heading, bold | `--unity-metrics-default-font_big_size` | 14 |
| Sub-heading, bold | `--unity-metrics-default-font_normal_size` | 12 |
| Body text | `--unity-metrics-default-font_normal_size` | 12 |
| Captions, pills, badges | `--unity-metrics-default-font_small_size` | 10 |

- Bold is for headings and the current tree row only, never for emphasis inside a sentence. [Foundations typography]
- Left-align all text. Only button labels are centred. [Foundations UX essentials]
- Prose wraps (`white-space: normal`). Labels do not wrap by default in UI Toolkit, so set it on every prose class.
- Limit prose lines to about 680 px (`max-width` on the page column), left-aligned in the content pane. Long lines are hard to read in a wide docked window. **(our choice)**

### Spacing

- Content padding follows the Inspector: 18 px left, 4 px right, 8 px top and bottom. The docs page uses 18 px on both sides, because it is a reading surface. [Foundations content organization]
- 2 px between related rows, 8 px between groups, 16 px before a section heading. Prefer space over boxes. [Foundations content organization]
- Indent 15 px per nesting level (tree, nested lists). [Foundations foldout]
- The window must work at 275 px wide with no horizontal scroll. The only exception is inside a code block, which scrolls on its own. [Foundations UX essentials]

### Shapes and states

- Round corners (`--unity-metrics-default-border_radius`, 3 px) only on things you can click: buttons, chips, cards, pills. Panels and callouts keep square corners. [Foundations interactions]
- A disabled control is shown at about 40% opacity, never hidden, and its tooltip says why it is disabled. [Foundations interactions]
- No shadows and no gradients. USS has neither (no `box-shadow`, no `linear-gradient`), and Unity's palette avoids heavy effects. [Foundations colour]
- Hover feedback: a background tint with a 0.1 s transition defined on the base class, not on `:hover`. [ui-uitk common issues]

### Icons

- Built-in Editor icons only for icons, through `EditorGUIUtility.IconContent(name).image` in an `Image` or `Button.iconImage`. It adds the `d_` dark variant itself. The library ships no icon files. [EditorGUIUtility source], [Foundations iconography]
- Icons are 16 x 16 with a 1 px margin. [Foundations iconography]
- The help icon is `_Help`, the one Unity draws in component headers. [EditorGUIContents source]
- One class lists every icon name the docs use. A test loads each name in both Editors, because names can change between versions.
- The ui-uitk skill's rule against built-in theme icons is about runtime UI. Editor windows should use them to look native.

### Wording

- Menu items use Title Case, like the existing `Tools/External Project Tools/Scene System/Set Up Scene System`. [Foundations UI copy]
- Window titles, page titles, headings, tabs, buttons, tooltips and help text use sentence case. This matches the existing Scene System page ("Scene sets", "Loading scenes", "Set up the scene system") and /unslop. **(our choice; Unity's own pages disagree with each other here)** [Foundations UI copy], [Foundations button]
- A button is a verb and at most three more words, with no article. A button that opens a window ends in "...". [Foundations UI copy]
- Labels have no final punctuation. [Foundations UX essentials]
- Address the reader as "you". Use the module's `GLOSSARY.md` terms and never its _Avoid_ words.
- Every description goes through /unslop: at most three sentences a paragraph, a section starts with what you do, no volatile facts (hashes, counts, branch names).

### Help boxes, tooltips and empty states

- **Help boxes** sit below the control they are about, stay under 200 characters, and say problem, then cause, then fix. Do not start with "Error" or "Warning", since the icon says it. At most one fix button and one link. [Foundations help box], [Foundations errors]
  - `HelpBox` gets its own button and link only from Unity 6000.5. On 6.3, build the row yourself: a `HelpBox` plus a button row under it, with the `unity-help-box` classes. [HelpBox source]
- **Tooltips** go on every control that does something. At most 200 characters, a full sentence, no keyboard shortcuts written in. A tooltip explains a control, it never replaces a docs section. [Foundations tooltip]
- **Empty states** are never blank. Show a centred, sentence-case line saying what to do ("Pick a page on the left"), with at most two small starter buttons, and no warning icon. [Foundations empty states]

### Toolbars and tabs

- One toolbar at the top. Most-used actions on the left, groups separated by a gap, no filler. [Foundations toolbar]
- Tabs have one or two words, at most four in a row, and a `view-data-key` so they keep their state. Remove a tab rather than disable it. [Foundations tab]

## Extra styling

These parts have no Unity equivalent. All of them use the tokens above, square corners unless clickable, and our own colour tokens in both skins. Never rely on colour alone: each one also has text or an icon. [Foundations accessibility]

| Part | Look |
|---|---|
| **Ladder pill** | A small rounded pill, 10 px text: "0 no code", "1 daily code", "2 choices", "3 full control". Green, blue, purple, orange, the same as the Save System docs. Shown next to page and section titles and in sample tables. |
| **Kind badge** | A muted monospace badge before a type or member name: class, component, asset, interface, enum, method, property, event. |
| **Callout** | Help box background, a 3 px left border in the callout's colour and a 16 px icon. Four kinds: Note (neutral), Warning (warning colour), You should see (green), Why it works (blue). Never more than two in a row. |
| **Code block** | See "Code blocks" below. |
| **Step list** | Numbered circles. Pending is a hollow circle, done is a filled green circle with a tick, the current step's row gets the highlight background. The step text says what to do; the result line under it is muted until the step is done. |
| **Event timeline** | One row per scene message or screen state: time since Play, a kind badge, the message name in monospace, one line saying why it fired, and a link to its Guide section. Newest at the bottom; it scrolls to stay at the end unless the user scrolled up. |
| **Action chip** | A small rounded button with an icon and a verb: Select, Ping, Open, Play. It is for actions that leave the page (select an object, open a window). |
| **Card** | Home page entry points ("Get started", "Learn by playing", "Look something up"): a rounded clickable box with a title, one line and a hover tint. |
| **Parts diagram** | Boxes made of VisualElements, linked by lines drawn with `Painter2D` in `generateVisualContent`. Each box is a link to its page. No images. [ui-uitk painter2d] |
| **Term link** | A glossary word with a dotted underline in body text colour. Hover shows the definition as a tooltip; click goes to the page that defines it. |
| **Arrival flash** | When a link or "?" opens a section, the window scrolls to it and its heading row shows the highlight background, fading out over 1 s. |

## Code blocks

- **Font:** the Editor's own monospace font, the one the Console uses: `EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf")`, set in C# with `FontDefinition.FromFont`. If it fails to load, fall back to `<mspace>`. [ConsoleWindow source], [Rich text tags] Check in both Editors that it loads.
- **Colours:** a small C# tokenizer turns keywords, types, strings, comments, numbers and the highlighted member into `<color>` rich-text tags, with colour values taken from our dark or light token set. [Rich text tags]
- **Header row:** a title (the file name or what the code does), then Copy and Open in IDE action chips on the right. Copy uses `EditorGUIUtility.systemCopyBuffer`. Open in IDE uses `AssetDatabase.OpenAsset(script, line, column)` at the block's first line. [OpenAsset], [Project Auditor]
- **Body:** `default-background`, a 1 px `default-border`, a muted line-number gutter, selectable text (`selection.isSelectable = true` and `focusable = true`), and its own horizontal scroll. Lines do not wrap. [SelectableLabel source]
- **Source:** a code block taken from a sample or library script names that `.cs` file. A test checks that every non-comment line of the block still appears in the file, so the block cannot drift from compiled code, and Open in IDE jumps to the block's first line in the file. [Save System data-source test], [Custom Inspector findings]
- **Text around it:** introduce every block with one sentence. Show left-out code with a `// ...` comment and no Copy button on that block. [Google code samples]

## Screenshots

Use a screenshot only where a picture explains a control faster than words: the scene picker, a Project Settings section, a component such as a relocatable or spawn point, the scene group inspector, a setup step. Everywhere else, embed the live control read-only or draw a diagram.

- **Capture inside the Editor only**, by grabbing the window's own pixels (`GUIView.GrabPixels`, as `AgentScripts/Tools/CaptureEditorWindow.cs` in Rally does). Never use a Windows screenshot. Size the window to a fixed size first, so a later capture is comparable with `visual_diff.sh`.
- **Crop to the control** and keep the Editor chrome only when it shows where the control lives.
- **Show sample content only:** sample scenes, sample scene groups, sample assets. No game names.
- **Marks are drawn by the window, not burned into the PNG.** A page lists each mark as a number, a position in 0 to 1 of the image's width and height, and a short label. The window draws a numbered circle (highlight colour, white number) at that spot and lists the labels under the image. Marks stay sharp at any zoom, follow the skin, and survive a re-capture if the layout did not move.
- **Files:** `Images/<module>-<what>.png` next to the pages, imported as Editor GUI textures with no mipmaps and no compression, so they stay sharp.
- **Alt text** under every image says what it shows, for readers who skim.

## Layout and hierarchy

```
[EditorWindow] "Scene System docs"
├── [Toolbar]  Back, Forward, Home | crumbs | "On this page" menu | search field
└── [TwoPaneSplitView] (fixed sidebar, 220 px default, remembered)
    ├── [Sidebar] tree: Home, Guide, Reference, Types, Samples
    └── [ScrollView] page column (max 680 px of prose, 18 px padding)
        ├── title row: ladder pill, title, kind badge (types)
        ├── lede: one or two sentences
        ├── sections: heading row with "#" copy-link and pill, then content
        └── pager: previous and next page
```

- The window is dockable, not a utility window, so it can sit beside the Inspector.
- Under about 500 px wide the sidebar collapses behind a toggle button in the toolbar. **(our choice)**
- "On this page" is a toolbar dropdown, not a third column, so the window works narrow. **(our choice)**
- Hierarchy reads top to bottom: page title, then section heading bars, then content. The only bold text is headings. [Foundations content organization]
- One page shows one topic. A long page uses section foldouts only for "If it goes wrong" and member details, which readers open on demand.

## Interaction conventions

- **Links navigate inside the docs; buttons and chips act.**
  - Unity keeps blue link text for links that leave the Editor. A docs page is a reading surface, so page-to-page links use the link colour too, underlined on hover only. Anything that opens another Editor window, selects or pings is an action chip, never a link. A link to the web has an external icon. **(our choice, a deliberate step away from [Foundations label])**
- **Deep links:** every heading has a stable id. `page#section` opens the page, scrolls to the section and flashes it. Ids are kebab-case and never change once published; a test checks every id that code passes.
- **History:** Back and Forward work like a browser, and Alt+Left and Alt+Right do the same. The current page and scroll position are serialized fields, so they survive a domain reload.
- **Keyboard:** the arrow keys move in the tree, Ctrl+F focuses search, Esc clears it, Enter opens the first result.
- **Search** covers page titles, headings, glossary terms and type names. It shows results in the sidebar in place of the tree, grouped by category.
- **Actions are read-only.** They open, select, ping, frame, enter or leave Play, and open a settings section. They never change settings, Build Settings or assets. Each one's tooltip names what it will open or select.
- **Highlighting a field** in another window uses `Highlighter.Highlight(windowTitle, text)`. It matches the window title exactly, cannot see inside collapsed foldouts or nested scroll views, and warns when nothing matches. So call it only after the target is open and visible, and fall back to selecting the object. [Highlighter source]
- **The "?" button** is the `_Help` icon drawn with an icon-button style, the same everywhere:
  - In an EditorWindow's tab bar, through the `ShowButton(Rect)` method Unity calls by reflection, as the Inspector's lock and the render pipeline's help windows do. [HostView source], [EditorWindowWithHelpButton]
  - In a Project Settings page title bar, through `SettingsProvider.titleBarGuiHandler`. [SettingsProvider source]
  - In a section heading row on the Scene System page and in custom inspectors, as a UI Toolkit icon button at the right end.
  - A component header's own "?" can only open a URL (`http`, `https` or `file:///`) through `Application.OpenURL`, and `HelpURLAttribute.URL` is also read when the header is first drawn. It cannot open the docs window safely. [Help source], [EditorGUI source]

## Building it

- Style with USS classes, never `element.style`. One `Docs.uss` holds layout and the `var(--unity-*)` roles; `DocsDark.uss` and `DocsLight.uss` hold our own colour tokens. Class names are kebab-case with a `docs-` prefix. [ui-uitk]
- No USS property Unity lacks: no `gap`, `z-index`, `box-shadow`, `:nth-child` or gradients. Use child margins and explicit classes. [ui-uitk]
- Rich-text link events (`PointerUpLinkTagEvent` and the rest) are in `UnityEngine.UIElements.Experimental` in both Editors. Use them in one adapter class only, so a later API change touches one file. [Link tag events]
- One code path for Unity 6.3 and Unity 7. Where an API differs (for example `HelpBox` buttons from 6000.5), use `#if UNITY_7000_0_OR_NEWER` with a working 6.3 branch, or build the 6.3 version for both.
- Pages are built when opened and cached until the source file changes. Large lists (search results, members) use `ListView`. [ui-uitk common issues]
- Unsubscribe from every scene system event when the window closes or Play stops, because domain reload may be off.

## Before calling a docs UI change done

- Stills of the window in both skins and both Editors, at 275 px and at a normal width, shown to the user and compared with the visual diff tool.
- Every control has a tooltip; every disabled control says why.
- No horizontal scroll outside code blocks at 275 px.
- Keyboard: tree, search, Back and Forward work without the mouse.
- Contrast of our own tokens at 4.5:1 for text and 3:1 for icons in both skins.
- No Console errors or warnings from the window in either Editor.

## Sources

- [Foundations accessibility] https://www.foundations.unity.com/fundamentals/accessibility
- [Foundations button] https://www.foundations.unity.com/components/button
- [Foundations colour] https://www.foundations.unity.com/fundamentals/color-palette
- [Foundations content organization] https://www.foundations.unity.com/patterns/content-organization
- [Foundations empty states] https://www.foundations.unity.com/patterns/empty-states
- [Foundations errors] https://www.foundations.unity.com/patterns/errors-and-messaging
- [Foundations foldout] https://www.foundations.unity.com/components/foldout
- [Foundations help box] https://www.foundations.unity.com/components/help-box
- [Foundations iconography] https://www.foundations.unity.com/fundamentals/iconography
- [Foundations interactions] https://www.foundations.unity.com/fundamentals/interactions
- [Foundations label] https://www.foundations.unity.com/components/label
- [Foundations tab] https://www.foundations.unity.com/components/tab
- [Foundations toolbar] https://www.foundations.unity.com/components/toolbar
- [Foundations tooltip] https://www.foundations.unity.com/components/tooltip
- [Foundations typography] https://www.foundations.unity.com/fundamentals/typography
- [Foundations UI copy] https://www.foundations.unity.com/fundamentals/ui-copy-style
- [Foundations UX essentials] https://www.foundations.unity.com/fundamentals/ux-essentials
- [USS variable reference] https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-uss/variables/built-in-variable-reference
- [Rich text tags] https://docs.unity.com/en-us/engine/current/manual/uitoolkits/uielements/uie-work-with-text/uie-rich-text-tags/uie-supported-tags
- [Link tag events] https://docs.unity.com/en-us/engine/current/manual/uitoolkits/uielements/uie-work-with-text/uie-rich-text-tags/add-hyperlinks-in-text
- [OpenAsset] https://docs.unity.com/en-us/engine/current/script-reference/unityeditor/assetdatabase/openasset
- [PingObject] https://docs.unity.com/en-us/engine/current/script-reference/unityeditor/editorguiutility/pingobject
- [EditorWindowWithHelpButton] https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.0/api/UnityEditor.Rendering.EditorWindowWithHelpButton.html
- [Tutorial criteria] https://docs.unity3d.com/Packages/com.unity.learn.iet-framework@3.1/manual/framework-documentation.html
- [Tutorial highlight] https://docs.unity3d.com/Packages/com.unity.learn.iet-framework@4.0/manual/highlight-guide.html
- [Diataxis] https://diataxis.fr/start-here/
- [Diataxis tutorials] https://diataxis.fr/tutorials/
- [NN/g help] https://www.nngroup.com/articles/help-and-documentation/
- [NN/g onboarding] https://www.nngroup.com/articles/onboarding-tutorials/
- [Google code samples] https://developers.google.com/style/code-samples
- [Animancer help] https://kybernetik.com.au/animancer/docs/help/
- [Project Auditor] https://github.com/Unity-Technologies/ProjectAuditor/blob/0.11.0/Documentation~/GettingStarted.md
- UnityCsReference (`master` is Unity 7, `6000.3` is Unity 6.3), https://github.com/Unity-Technologies/UnityCsReference:
  - [UIElementsEditorUtility] Editor/Mono/UIElements/UIElementsEditorUtility.cs
  - [EditorGUIUtility source] Editor/Mono/EditorGUIUtility.cs
  - [EditorGUIContents source] Editor/Mono/GUI/EditorGUIContents.cs
  - [ConsoleWindow source] Editor/Mono/ConsoleWindow.cs
  - [SelectableLabel source] Modules/PackageManagerUI/Editor/UI/Common/SelectableLabel.cs
  - [HelpBox source] Modules/UIElements/Core/Controls/HelpBox.cs
  - [Help source] Editor/Mono/Help.cs
  - [EditorGUI source] Editor/Mono/EditorGUI.cs (HelpIconButton, HelpButtonCache)
  - [HostView source] Editor/Mono/HostView.cs
  - [SettingsProvider source] Editor/Mono/Settings/SettingsProvider.cs
  - [Highlighter source] Editor/Mono/Tutorial/Highlighter.cs
- [ui-uitk], [ui-uitk common issues], [ui-uitk painter2d]: the ui-uitk skill and its references.
- [Custom Inspector findings]: the Custom Inspector asset, read from the Lab's git history before its removal (commit 898e105's parent). Ideas only, no code copied.
- [Save System data-source test]: `SaveSystemDocsTests.TheLinesThatMatter_InEachSampleReadme_StillAppearInItsScripts` on library `origin/save-system`.

## Checked in a running Editor (scene documentation, session D)

- **RobotoMono:** `EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf")` returns the font in Unity 6.3 (True) and Unity 7.0a16 (True); code blocks and member names render in it.
- **ShowButton** draws the "?" in an EditorWindow's tab bar in 6.3.
- **titleBarGuiHandler is not drawn for a UI Toolkit page** (`activateHandler`) in 6.3: put the page's "?" in the page's own title row as well.
- **Highlighter** is not used by any page yet; not checked.
- **Theme values on Unity 7:** not checked visually. Grabbing window pixels (`GUIView.GrabPixels`) crashed Unity 7000.0.0a16 (D3D12 `GrabIntoRenderTexture`), so stills come from 6.3 only; the Lab is checked with probes.

## Rules learned

- **Icons:** load them with `EditorGUIUtility.FindTexture`, which logs nothing for a missing name (`IconContent` logs an error). Several names from older lists do not exist in 6.3. Buttons and chips use flat glyphs in the text colour; picture icons are kept for Note, Warning and the help "?" only (user, session D).
- **Glyphs:** Inter has no ⧉ or ⌖; check each glyph renders before using it.
- **Inline code:** a muted accent colour on a faint `<mark>` box. Do not force `<mspace>` on the body font: wide letters overlap.
- **Leading spaces** in a Label are dropped: indent code with no-break spaces.
- **Graphs:** layered (columns left to right, rows top down under 520 px), curved arrows behind the boxes, hover tracing, and a box with a map of its own opens it; show only the core boxes first, with "Show everything" for the rest (user, session D).
- **Steps tick in order:** a message ticks only the next step that waits for one, so a later step never ticks early.
- **Start-up messages fire before Play counts as entered:** listen from `RuntimeInitializeOnLoadMethod(AfterAssembliesLoaded)`, after the scene system resets its statics.
- **Unity 7 analyzers:** a type with static members needs `[Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]` behind `#if UNITY_7000_0_OR_NEWER`; use `CompilationPipeline.GetAssemblies()` instead of `AppDomain.GetAssemblies()`.
