# Library docs standard

Short rules for anyone (person or AI) who writes or changes a module's docs. This applies to every module, not only the save system.

## 1. Location and tech

1. Each module's docs are in `Modules/<Module>/Documentation/`, a folder Unity shows. The start page is `index.html`, the only page loose at that folder's root. Every other page lives in `Pages/`, so opening the folder shows one obvious file to open plus named folders, not a wall of files.
2. Pages are hand-written static HTML. They open from `file://`, with no build step and no internet. Nothing loads from a CDN.
3. All pages share `assets/docs.css` and `assets/docs.js`. A page has no inline `<style>`.
4. Only one thing is generated: `assets/glossary-data.js` (the term tooltips), from `GLOSSARY.md`. There is no glossary page — every term's definition lives on its own Types page instead. Tests check the rest against the code.
5. An Editor menu item opens the docs: Tools > External Project Tools > `<Module>` > Open Guide.

## 2. Page types

| Type | Job | Named |
|---|---|---|
| Home | What the module does, a mind map of its parts, three ways in | `index.html` |
| Task page | How to do one job, with its sample, code, and problems | By the task, such as "Save data", in `Pages/` |
| Type page | One public type, Unity ScriptReference style | `api/<TypeName>.html` |
| Reference index | Every public type, grouped by folder or responsibility | `Pages/reference.html` |
| Code map | Every script: its part, its job, its file, and when to use it | `Pages/code-map.html` |
| Find a problem | One-line symptoms that link into task and type pages | `Pages/problems.html` |
| Sample README | One sample: try it, what you'll see, the lines that matter | `Samples/<NN Name>/README.html` |

Every glossary term's plain-English definition lives on a Types page (its own if one matches the term, otherwise the closest related one) — there is no separate glossary page. `GLOSSARY.md` stays the source of truth for the team's words and still drives `assets/glossary-data.js` (the term tooltips).

A task page covers one job and links out for the rest. It doesn't hold everything in one long page.

## 3. Task page anatomy

Every section of a task page follows this order:

1. **Heading and badges.** Sentence case, named by what the reader wants to do, plus the ladder level pill (0 no code, 1 daily code, 2 choices, 3 full control).
2. **Try it in the sample.** A box at the top of the section: which sample, 1-3 steps, and what you'll see. The sample comes first, so the reader is curious enough to click it.
3. **Or do it yourself.** The smallest code that works, in a titled, highlighted block.
4. **Show the result.** An Inspector screenshot, a Console line, the saved JSON, or a "You should see" box.
5. **Short notes.** At most three short paragraphs, or a short list. One idea per sentence.
6. **Warnings in place.** A warning sits next to the line it is about, not in a list at the end.
7. **If it goes wrong.** 2-5 symptom and fix lines for this feature only.

## 4. Type page anatomy (after Unity's ScriptReference)

1. Header: type name, kind (component, class, static class, interface, attribute, struct, enum), namespace, file path, which part of the system it is in (with a code map link), ladder level.
2. A summary of one or two sentences.
3. A **Try it** chip that links to the sample that uses it.
4. Components only: an **In the Inspector** section with a screenshot when one exists, and a three-column table (Inspector label, API member, what it does). The table links to **In code** and back. This restores Unity's paired Manual and Scripting links.
5. **In code**: a 3-8 line example first. A full script follows only when the short one isn't enough.
6. **Members**, grouped as Properties, Methods, Events, Static, and Attributes or Values for enums. Each member gets one row: its signature and one line. Long notes expand in place. There are no per-member pages. Rows are alphabetical inside each group. Inherited Unity members aren't listed.
7. **If it goes wrong**, for this type only.
8. **See also**: a single line of links.

## 5. Code blocks

1. Every block has a title that says what the code is: "Health.cs: a script with one saved field", not "Code".
2. Syntax highlighting comes from `docs.js`, with a small offline C# highlighter. Write `<pre><code class="cs">`.
3. Code is shown in real C# shape: classes, methods and statements each get their own lines; braces go on their own lines; no two declarations share a line. `[Save] int x = 1;` stays on one line. When a field has two attributes, each goes on its own line.
4. Every block is a full shape that compiles, or is plainly marked as a fragment ("inside your MonoBehaviour").
5. Comments explain the result, not the syntax.
6. A copy button is added automatically.

## 6. Words

1. Run every description through `/unslop`. Keep only what tells the reader what to do or what the system does.
2. A paragraph has at most three sentences. A section starts with what you do, not with background.
3. Use the module glossary's terms and avoid its _Avoid_ words.
4. Link text names the target: "see Save spawned copies", not "click here".

## 7. Glossary and links

1. Every special term uses one class, `a.term`, everywhere it appears in prose: `<a class="term" href="../api/SavableBridge.html#save-id">save ID</a>`. There is no separate glossary page to link to — the href always points straight at the term's Types page, at the term's own `id` on that page where one exists.
2. Terms are blue and underlined, turn green on hover, and show their definition in a tooltip (from `assets/glossary-data.js`) on hover. Clicking goes to the term's Types page. `a.type` keeps its own mono-badge style for type names inside structured tables (such as the reference index), not for terms in prose.
3. Link a term on its first use in each section, not every repeat.
4. `assets/glossary-data.js` is generated. Edit `GLOSSARY.md`, then run Update Glossary Data. A test fails if it goes stale.

## 8. Navigation

1. Every page has the same left tree, built by `docs.js` from one list, with the current page highlighted. There is also an "On this page" list, breadcrumbs, and previous and next links.
2. While you scroll, the URL updates to the section you're in, so the browser's back button returns to that section.
3. A link to a sample carries where you came from, which `docs.js` adds by itself. The sample page then shows "Back to <section name>" at the top.
4. Each section heading has an `id` and an anchor link.

## 9. Diagrams and screenshots

1. Diagrams are inline SVG that uses the theme colors, so they work offline in light and dark mode. Mermaid isn't used.
2. Home has a small mind map: the parts of the module, each one linking out. The code map has the detailed version: every script by folder, its job, and its arrows.
3. Screenshots go in `Documentation/images/<module>-<what>.png`. Their `alt` text says what the picture shows. Retake a screenshot when its window changes. On the Unity 7 alpha, capture with Windows (Snipping Tool, `PrintWindow`), never with the Pipeline's `capture editor_window`.
4. When there is no screenshot, show the result as text: the Console line, the saved JSON, or the Inspector values.

## 10. Tables and badges

1. Each table has one job. Labels in a column don't wrap: level pills sit above their name, and rows are vertically centered.
2. Color coding: ladder levels are green 0, blue 1, purple 2, orange 3; member kinds (method, property, event, attribute, type) each keep one color. The same colors are used on every page.

## 11. Troubleshooting

1. A feature's problems are on that feature's page, under "If it goes wrong".
2. A warning the reader needs while doing the task sits next to the code.
3. `problems.html` only lists symptoms, one line each, linked to the fix. It holds no explanations.

## 12. Keeping docs right

1. Change the docs in the same commit as the code.
2. No volatile facts in the guide: no commit hashes, test counts or branch names.
3. Sample README code is copied from the sample's scripts (`<pre data-source="Scripts/...">`), and the docs test checks it.
4. The docs tests fail on any of these: a broken link or anchor, a sample with no README, stale glossary tooltip data, a public type with no page, a type page that misses a member, a missing "?" target.

## Owner

Abstract Pixel Studios owns these docs. Whoever changes a module's code updates its docs in the same commit.
