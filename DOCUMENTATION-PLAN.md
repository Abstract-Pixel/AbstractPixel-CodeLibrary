# Documentation plan

How the Scene documentation window is built, how every page gets brought to the same standard, and what was learned
building the sample pages (November 2026), including what did not work.

Read this first, then `EditorModules/Documentation/EDITOR-DOCS-UI-GUIDE.md` (Unity look and wording rules).

## 1. Where things are

| What | Path |
|---|---|
| Library repo (edit here) | `C:\Users\avig7\Desktop\AllUnity\ExternalCodeAssets\AbstractPixel-CodeLibrary` |
| Branch with this work | `docs-restyle-sample` (commits `ef0fd56` … latest) |
| Abstract Pixel Lab (Unity 7000.0.0a16) | `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\Abstract_Pixel_Lab` — uses the library through `"com.abstractpixel.library": "file:C:/Users/avig7/Desktop/AllUnity/ExternalCodeAssets/AbstractPixel-CodeLibrary"` |
| ProjectRallyShift (Unity 6000.3.15f1) | `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\ProjectRallyShift` — uses its own clone of the same repo through `"file:../LocalPackages/AbstractPixelLibrary"`; that clone is on branch `scene-system-rework` at `8ea6b51`, behind this work |
| Docs code | `Modules/SceneManagement/Editor/Documentation/` (`Content/`, `Rendering/`, `Window/`, `Styles/`, `HelpEditors/`) |
| Pages | `Modules/SceneManagement/Editor/Documentation/Pages/` (`guide/`, `reference/`, `types/`, `Contents.md`) |
| Design decisions (ADRs) | `Modules/SceneManagement/docs/adr/` |
| Docs tests | `Modules/SceneManagement/Tests/Documentation/` |
| Screenshot tool | `Modules/SceneManagement/Editor/Documentation/Tools~/capture-window.ps1` (Unity ignores `Tools~`) |
| Reference pages | **Guide › Loading scene** (`Pages/guide/loading-scene.md`) and **Types › SceneActions** (`Pages/types/SceneActions.md`) |

Open the window: Tools > External Project Tools > Scene System docs, or the "?" on any scene system inspector or
settings page. From code: `SceneDocumentationWindow.OpenAt("guide/loading-scene")`.

## 2. The page format

Every page carries the line `<!-- layout new -->` under its title. The old render paths are gone (November 2026), so every
page renders in this layout; keep the line on new pages for readers of the raw Markdown.

How each Markdown construct is drawn on a new-layout page:

| Write this | You get |
|---|---|
| `# Title <!-- level 1 -->` | Page title with its level pill |
| One short paragraph under the title | The page's lede: what this is and when you need it |
| `## Section <!-- level 0 -->` | Numbered section box (1., 2., …): header bar, then everything up to the next `##`. Text has a 14 px margin; tables, rows and code meet the box edges |
| `### Part` or a paragraph that is only `**Bold text**` | Numbered part (1.1, 1.2, …): a foldout strip across the box, **closed by default**, open for the session once opened |
| Part titled exactly `Try it in the sample` + `<!-- action open-sample 06 "Open sample 06" -->` | The Open sample button and the sample folder sit on the strip itself |
| `<!-- action open-settings "Loading scenes" "Open Loading scenes" -->` followed directly by a `**Label:** value` list | A button box: an inner box with the button on its header strip, the rows inside, closed by default |
| A bullet list where every item starts `**Label:**` (2+ items) | Fact rows: label column, value column, zebra stripes |
| An ordered list (no `done-when`) | Numbered steps in circles |
| An ordered list whose items each hold a nested ordered list | Phases: inner boxes numbered 1, 2, 3, closed by default, steps 1.1, 1.2, 2.1 … |
| An ordered list with `<!-- done-when OnGameStarted -->` items | A step list that ticks while you play (Play-mode messages) |
| `<details><summary>More: …</summary> … </details>` | An inner box (foldout) inset 10 px, closed by default; rows inside run to its edges |
| `> [!TIP]`, `> [!NOTE]`, `> [!WARNING]` | Callouts, inset 10 px on both sides, coloured left edge. Never full width |
| A table | Edge-to-edge table, header row, zebra rows |
| A table row `\| **Group title** \| \| \|` (bold first cell, others empty) | A group row; rows after it are indented until the table ends. Put ungrouped rows first |
| `` `TypeName` `` or `` `Type.Member` `` | A link to that type page (and member row) when a `types/*.md` page names it; tooltip shows its summary |
| `[words](term:slug)` | Glossary term with tooltip |
| `<!-- locate "<scene path>" "<Object/Path>" "<Component>" -->` | "Find it in" bar: scene › object › component with Unity icons and **Select in Hierarchy** |
| `<!-- type-header Full.Type.Name -->` | Declaration box (`public static class SceneActions`) and Namespace / Assembly / File rows |
| `<!-- members Full.Type.Name -->` | One numbered section per member kind (Properties, Fields, Methods, Events, Values): type on the left, modifiers + name + parameters, one-line summary, "More" with the remarks. Reads the `///` comments: `<summary>` is the one line, `<remarks>` lines starting `- ` are the bullets |
| `<!-- decision -->` / `<!-- adr-list -->` | ADR header with status pill / the decisions list |

Directive names must be in `Content/SceneDocsDirectives.cs` (`Known`) or the page reports a problem.

## 3. The engine (what each piece does)

Rendering, in `Rendering/` (generic) and `Content/` (scene system specific):

- `MarkdownParser` → `DocsPage` (blocks, headings, `Layout`, `IsNewLayout`).
- `DocsRenderer.Render` → for new-layout pages hands the blocks to `DocsSections.AddBlocks`, which builds section boxes,
  part foldouts, the sample strip, button boxes, and marks the first/last block of every box with `docs-first` /
  `docs-last` (USS has no `:first-child`).
- `DocsNewLayout.cs`: `DocsFactRows`, `DocsFlow` (steps and phases), `DocsSessionFoldout` (foldouts remembered in
  `SessionState` for the session).
- `DocsRenderer.Table`: group rows; `Details`: "More" inner boxes.
- `SceneDocsHost.RenderDirective`: every directive; `CodeLinkTarget` / `TooltipFor`: type links.
- `SceneDocsMemberReader`, `SceneDocsDeclarations`, `SceneDocsMemberTables`, `SceneDocsTypeHeader`: type pages.
- `SceneDocsLocate`: the "Find it in" bar. `SceneDocsActions`: what buttons do (read-only actions only).

Window, in `Window/`:

- `SceneDocumentationWindow`: toolbar (Back, Forward, Home, sidebar, location on the left, History on the right,
  "On this page" menu, search), contents tree (follows the open page), page column, rail.
- `DocsTrail`: History (last pages, links back). `DocsHistory.RecentPages`.
- `DocsPageRail`: "On this page" tree beside the page, sticky, marks the section being read. Shown on new-layout pages
  when the window is at least 1250 px wide; otherwise the toolbar menu is used.
- `SceneDocsSampleFollower`: turns to a sample's page when you open its scene, only with the preference on
  (`Editor/OpenWithGroup/SceneDocsFollowPreference.cs`, Preferences > Abstract Pixel > Scenes, off by default), never
  for scenes a docs button opened.
- `Content/SceneDocsContents.cs` reads `Pages/Contents.md`: `##` = section, `###` = folder inside it (Types uses 8
  folders), `- [Title](path)` = page.

Styles, in `Styles/`: `Docs.uss` (window, base), `DocsNewLayout.uss` (new layout), `DocsDark.uss` / `DocsLight.uss`
(colours per skin; loaded last, so they win at equal specificity).

### Box model (the rule everything follows)

1. **Section box** (level 1): outline, header bar, no padding. Text inside has 14 px side margins; tables, fact rows,
   code and part strips run to the box edges.
2. **Part** (level 2): a numbered foldout strip across the section box, closed by default. A part runs to the next strip
   or the box end.
3. **Inner box** (level 3): button boxes, "More", phases. Inset **10 px** from the outer box on every side, its own
   1 px outline on all four sides, a header row, and **no padding inside** — rows touch its edges.
4. **Callouts**: inset 10 px both sides like inner boxes, never full width, keep the coloured left edge.
5. Never go deeper than this. Never pad a box; give the text its margin instead.

### Sizes

Page and rail text is 25% over Unity's: the `--unity-metrics-default-font_*_size` variables are redefined on
`.docs-page-area` (small 13, normal 15, big 18, very big 24 px). The contents sidebar and toolbar keep Unity's sizes.
Page column `max-width` 1163 px (old layout 895 px); paragraphs and lists 900 px. Buttons inside the page are set to
the normal size explicitly (Unity's button style ignores the variable).

## 4. Writing every page (the content standard)

Model every guide page on **Loading scene** and every type page on **SceneActions**.

- Say what it means for the reader, not what the code does. For each class: which members you will probably use and
  why (a "Start here" table: You want to / Use / Why), which are called for you and should be left alone.
- Say **where** a thing is set (Project Settings path, component, scene group field) and whether it is an Inspector
  field, an event, or code only. Use the exact Inspector labels (`Show the loading scene`, `Every time, Retry included`).
- Name the script a table is about, and show where it lives with a `locate` bar when it sits on an object in a sample.
- Describe samples as they really are (open the scene and the component and check; sample 06 uses
  `Sample06BarDisplay`, its event lists are empty). Credit game-specific setups to the game (Rally's MMProgressBar).
- Avoid words that collide with packages or other meanings, or say which one you mean ("Feel: a foldout on this
  component … Not the Feel package").
- Keep every existing fact (track them in a checklist per page), add no filler, one idea per row. A section opens with
  one or two lines; the rest goes into parts and "More".
- Don't put two different things in one sentence that looks like a heading ("Show a loading scene" / "Show the loading
  scene" read as the same thing until the second is called the scene group's field).
- Member comments in code: `<summary>` one line, details as `<remarks>` lines starting `- `.

## 5. Rolling it out

Order: Guide pages (12), Reference (3), Types (47 by folder), design decisions in `Modules/SceneManagement/docs/adr/`
(0003 is already done), Samples READMEs last. Per page:

1. Add `<!-- layout new -->`; restructure into sections and parts per section 2; keep every fact (write the old → new
   list for the page).
2. Rewrite wording per section 4. Check facts against the code, not against the old text.
3. Type pages: `type-header`, a "Start here" table, `members`; fix the `///` comments in the type's source (comments
   only) so summaries are one line.
4. Open the page in the window, open every foldout, screenshot, check (section 6), fix.
5. Commit per page group.

When all pages are done: remove `IsNewLayout` checks and the old render paths (`SceneDocsMembers.Render`,
`SceneDocsReference.AdrList`, old `Pager`, old `Details`), then run the docs tests.

## 6. Verifying (what to check, and only that)

- Screenshot without stealing focus:
  `powershell -NoProfile -File Modules/SceneManagement/Editor/Documentation/Tools~/capture-window.ps1 -Title "Scene documentation" -Out <png>`
  (`-Title` = start of the window title: "Scene documentation" when floating, the project name when docked; crop with
  `-X -Y -W -H` in **screen** pixels, which on a second monitor start at that monitor's offset, e.g. 1912).
- Drive the window from Pipeline `script eval`: `SceneDocumentationWindow.OpenAt("<topic>")`, then
  `UQueryExtensions.Query<Foldout>(w.rootVisualElement).ForEach(f => f.value = true)` to open everything,
  `w.PageScroll.schedule.Execute(() => w.PageScroll.scrollOffset = new Vector2(0, y)).StartingIn(300)` to scroll (set it
  scheduled; set directly it gets overwritten), and `w.RenderContext.Page.Problems.Count` must be 0.
- Look at the changed parts only, closed and open: every box shows **all four borders** (a missing left or top edge is
  a bug), callouts are inset on both sides, rows touch inner-box edges, no box has padding inside, text never touches a
  box edge, nothing clips or overlaps, strip buttons sit on the title line, numbering is right, the rail lists the
  page's sections and parts.
- Check both skins once per page group (`UnityEditorInternal.InternalEditorUtility.SwitchSkinAndRepaintAllViews()`;
  switch back afterwards and confirm `EditorGUIUtility.isProSkin` is what it was).
- Check both editors once per page group: the Lab (Unity 7 alpha) and ProjectRallyShift (Unity 6.3).
- Run the docs EditMode tests once before each commit; nothing else unless you changed it.

## 7. Findings

### What did not work

- **The interactive sandbox** (a fake copy of Project Settings and a scene group, a simulated run, a mini log): too
  complicated, overwhelming, and it felt fake even though it called the real `LoadingSceneRoute.ChooseFor`. Removed.
  Explain with the real thing instead: real settings paths, `locate` bars, open-sample buttons.
- **Left lines (rails) to show where a section ends**: too many lines; replaced by boxes.
- **Padding inside boxes, and boxes inside boxes inside boxes**, each with padding: confusing. The rule in section 3
  replaced it.
- **Full-width nested boxes**: the inner box merged with the outer one and lost its outline. Inner boxes are inset
  10 px with their own outline.
- **Full-width callouts**: read as part of the box; keep them inset.
- **Combining location and history in one breadcrumb**: confusing. Location left, History right.
- **A plain text list for "On this page"**: looked flat; it is a tree like Unity's now.
- **Restating the type docs in guide pages** ("Reads… Smooths it… Raises…"): readers could not tell what was an
  Inspector field, what was code, or what it did for them.
- **Claiming a sample does something without opening it** ("as the sample does: drag MMProgressBar…" — sample 06 does
  not). Check every sample claim in the scene.
- **Asking the reader to find a component with no cue**: add a `locate` bar.
- **Auto-turning the docs page when a scene opens**: docs buttons that opened scenes made the page jump back to the
  sample. Now a preference, off by default, and never for docs buttons.

### Gotchas (cost time once; don't repeat)

- **USS has no `:first-child`, `:last-child` or `:not()`**. `:not()` makes Unity drop the **whole** stylesheet, so
  everything new loses its style at once; the other two are ignored silently. Mark elements with classes in code.
- **Skin sheets load last** (`DocsStyles.Apply`: Docs, DocsNewLayout, then Dark/Light): at equal specificity the skin
  wins. Setting `border-color` in the layout sheet also overwrites a callout's coloured left edge; setting only three
  sides leaves the fourth uncoloured (the missing left border on inner boxes and the type declaration box). Set all
  four sides deliberately.
- **Unity's button and toolbar styles ignore inherited font sizes**; set them for `.unity-button` and `Label` inside
  toolbars explicitly.
- **A button on a foldout strip**: add it to `foldout.Q<Toggle>().Q(className: Toggle.inputUssClassName)` and stop the
  `ClickEvent`; a click then runs the button and leaves the foldout alone (checked with simulated pointer events).
- **`Write`/`Edit` fail on files changed since last read**; re-read first. Bash heredocs break on apostrophes inside
  `'…'`; write scripts to a file.
- **Screenshots**: `CopyFromScreen` captures whatever window is on top (it captured the owner's browser) and brings
  Unity forward. Use the `PrintWindow` tool above. Never run Pipeline `capture editor_window` on the Lab's Unity 7 alpha
  (it crashes the Editor). Image viewers may cache by file name; use a new name per shot.
- **Pipeline**: after `script recompile`, wait with `unity status --until-ready` and poll `script recompile_status`; the
  connection drops during the reload. The Lab pins port 7860. **ProjectRallyShift has an older Pipeline package with
  different command names** (its console logs "No command named 'editor.status'"); list its commands with
  `unity command --project-path <Rally path>` before using any.
- **Unity 6.3 compatibility**: Rally runs 6.3. Guard Unity-7-only APIs with `UNITY_7000_0_OR_NEWER` (the docs code
  already does for `NoAutoStaticsCleanup`); check every new UI Toolkit API against 6.3. USS variables that do not exist
  in 6.3 silently fall back.
- **The Rally clone is a separate working copy.** Bring the branch over with git (fetch from the library repo path),
  check `git status` there first, and never discard local changes in it.
- **Not yet run**: `SceneDocsMemberReaderTests`, `SceneDocsNewLayoutTests` (written, never run) and the existing docs
  tests after the contents tree became nested and the old member and ADR views were bypassed.
