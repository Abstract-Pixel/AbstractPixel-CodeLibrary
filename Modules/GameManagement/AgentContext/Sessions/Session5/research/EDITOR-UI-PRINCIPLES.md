# Editor UI Principles for Unity Tool Screens

Purpose: a checklist a reviewer can hold against a screenshot, plus per-screen guidance for our five screens (Project Settings ordered list, validator problem list, ScriptableObject inspector, setup wizard, runtime debug overlay).

How sources were used: everything below is paraphrased. "Unity ES" = Unity Editor Design System (unityeditordesignsystem.unity.com, the current successor to the old Editor HIG; the foundations.unity.com mirror did not resolve from this machine). Where a point is my inference rather than a source statement, it says "(inference)". Source numbers like [S3] are in section 4.

Gaps to know about: Unity does not publish exact label-column pixel widths or inspector row heights in these pages (the Inspector 50/50 label/field split is the closest). The runtime overlay has almost no Unity or NN/g guidance of its own, so that section leans on general rules and is marked as inference. No Unity blog or GDC talk on editor-tool UX turned up with usable text.

---

## 1. Checklist (numbered, one line each)

Reviewer: mark each Pass / Fail / N/A against the screenshot.

1. Looks native: stock Unity controls, Inter-style 12 px text, theme colors, no custom skin or branding. [S1, S2]
2. Works in both light and dark skin; no hard-coded colors that vanish in one theme. [S1, S3]
3. Labels are Title Case, no trailing punctuation (no colons), about 3 words or fewer. [S1, S4]
4. Buttons are verb phrases of 1-3 words; "..." only when the action needs more input. [S1, S4]
5. Labels left-aligned, button text centered, padding kept between text and neighbors. [S1, S5]
6. Groups are separated by vertical space (about 8 px between groups, about 2 px between fields), not by extra boxes. [S6]
7. Boxes/containers appear only where the content is interactive (reorderable list, buttons) or a callout (help box). [S6]
8. Foldouts are used for larger sets users may hide; small always-visible groups of about 2-5 fields get a plain header instead. [S6, S7]
9. Frequently used controls are never hidden behind a foldout; most-needed controls come first. [S6, S8]
10. At most two levels of disclosure (always-visible, then one "advanced" layer); no deep nesting. [S9, S7]
11. Dependent controls sit directly below the control that gates them; unavailable controls are disabled with a tooltip saying why, rarely-relevant ones are hidden, buttons are never hidden. [S1, S10]
12. Every input has a label and a tooltip (full sentence, sentence case, about 200 characters or fewer, no jargon); disabled and icon-only controls have tooltips too. [S1, S11]
13. Severity uses Unity's semantic colors and icons: red octagon for error, yellow triangle for warning, plain/white speech bubble for info; color is never the only cue (icon and wording differ too). [S1, S12, S13]
14. A message about one control sits in a help box directly below that control; a message about the editor's operation goes to the Console; blocking dialogs only for critical or destructive cases. [S12, S13]
15. Message text reads problem, then cause, then fix; plain language; specific and searchable; does not begin with "Error" or "Warning"; no blame; under about 200 characters. [S12, S13, S14]
16. If a user can fix the problem in the editor, the message carries a button that does it (one click), not just a description. [S13, S14]
17. Errors appear at the source and only after something is actually wrong; likely mistakes are prevented earlier (disable, clamp, default). [S14]
18. Empty lists and empty windows show an empty state (cause, then one clear action), centered, with no error or warning icon, instead of a blank area. [S15]
19. Sensible defaults are pre-filled so the screen is usable untouched; wizard remembers earlier choices. [S16]
20. Every edit goes through SerializedObject (or Undo) so Ctrl+Z works, and dirty/save state behaves like built-in inspectors. [S17]
21. Long text truncates with an ellipsis (middle truncation for names where the end matters) and shows the full text in a tooltip. [S5]
22. Layout holds at the window's minimum size: no horizontal scrollbar at the 275 px Inspector minimum, text can grow for translation. [S1, S3]
23. Multi-step flows show all steps, mark the current one, and have buttons named for their destination, in the same place on every step. [S16, S18]
24. Reorderable lists show a handle or drop line, add and remove controls, and a header; row height is consistent (about 21-22 px). [S19]
25. Reviewer test: a first-time user can say what the screen is for and what to do next within five seconds, and the main action is the most visible thing. (inference, based on S9, S20)

---

## 2. Per-screen guidance

### 2.1 Project Settings page with an ordered list editor

What the sources say:
- Project Settings are shared project-wide; per-user options belong in Preferences. Page name equals the feature or window name, and the content title matches the page name. Sub-page names do not include the word "settings". [S3]
- Content panel: label and value in two columns split 50/50, like the Inspector. Content padding about 8 px vertical, 4 px horizontal. Group with foldouts; do not add icons, branding or extra titles. [S3]
- Group foldouts: 4 px bottom padding, 15 px indent per nested level. [S3]
- One scrolling page for connected flows or content that may grow; tabs only for roughly 3-5 categories. [S3]
- A page with nothing configured yet should show onboarding-style empty content. [S3]
- Reorderable lists: use the new list view for new UI (the older IMGUI ReorderableList only where not porting). Turn reordering on only when order has meaning. Items about 21-22 px tall, 2 px padding top and bottom, controls 6 px apart. Show the list size field when bulk adds are likely. Footer add/remove should stay visible. Not for hierarchical or multi-column data. [S19]

Apply to us:
- Because order matters in our list, say so in one line above it ("Items run top to bottom") and show the order visibly (numbers or a drop line while dragging). (inference)
- Put the list under a short header plus the list's built-in foldout/size field; do not wrap it in another box. [S19, S6]
- Per-row content: one primary field (the name), one or two secondary fields. Anything more goes in a foldout or a detail pane below the list.
- Empty list: a centered one-line explanation plus one "Add ..." button. [S15]
- Row-level problems: a small severity icon at the row's left or right edge with a tooltip; full wording appears in a help box under the list or in the validator. (inference)
- Provide Reset only if it is undoable; label it as a verb ("Reset To Defaults") and keep it away from the Add button. (inference, based on S14 error prevention)

### 2.2 Validator / problem list

What the sources say:
- Three severities with fixed look: error (red, octagon), warning (yellow, triangle), info (white/grey, speech bubble). [S13]
- Wording: problem, cause, solution; polite, plain, specific; no "Error:" prefix; short. [S13, S12]
- Add a button when the user can fix it directly in the editor; a link only when outside docs are needed first. A help box in UI Toolkit can hold one button and one link. [S12]
- NN/g: put the error near its source, make it noticeable without relying on color alone, scale intrusiveness to severity, offer likely fixes as one-click choices, keep what the user entered. [S14]
- Console is for editor operations; persistent, important messages also go there. Click-to-ping a context object. [S13, S1]
- Use a list view for flat lists; alternating row backgrounds help scanning dense rows. [S19b]

Apply to us:
- Sort by severity (errors first), then by place in the settings. Show counts in a summary line ("2 errors, 1 warning") using the same icons.
- Each row: icon, one-sentence problem with the cause, then a single "Fix" button (verb, for example "Add Missing Scene") and a way to jump to the offending item. Rows with no safe auto-fix show a link or a plain "how to fix" sentence instead of a dead button. (inference, based on S12, S14)
- Do not repeat the word "Error" in row text. Group identical problems ("3 items have no name") rather than listing three lookalikes. (inference)
- A "Fix All" button only if every fix is safe and undoable; otherwise per-row fixes only.
- Clean state is an empty state with a calm message ("No problems found"), no green fireworks and no warning icon. [S15]
- Show problems inline next to the setting that causes them (help box under the control) and also in this summary list; the summary should never be the only place. [S13, S14]
- Info-level messages are collapsed or filtered by a toggle so they cannot bury errors. (inference)

### 2.3 ScriptableObject custom inspector

What the sources say:
- UI Toolkit inspectors bind with a binding path and PropertyField, giving automatic binding and Undo support; use a property drawer for plain serializable classes. Custom inspectors apply to MonoBehaviour and ScriptableObject. Keep editor code in an Editor folder or editor-only assembly. [S17]
- Inspector padding is about 18 px left, 4 px right, 8 px top and bottom; the Inspector is dense, so group with spacing, not boxes. [S6]
- Use the Inspector for simple vertical content and a dedicated window for dense or wide content; avoid empty panels. [S21]
- Unity's old guidance: show the controls most users need first. [S8]
- Odin-style grouping (foldout group, tab group) works because groups nest and keep declaration order; the equivalent in plain Unity is foldouts and tabs. [S22]
- Bold is for component headers and strong emphasis, not for ordinary grouping. [S6]
- Dependent fields: below the field that gates them. [S1]

Apply to us:
- Order: identity/summary at the top, the few commonly edited fields next, then "Advanced" in one foldout (collapsed by default). Stop at two levels. [S9]
- Keep stock look: reuse PropertyField so lists, objects and enums look like every other inspector. [S17]
- Read-only derived values use read-only fields (selectable, readable), not disabled-looking text; disable only when the user could act if something changed, with a tooltip. [S10]
- Put one-line validation as a help box right under the field that is wrong; do not stack more than two or three help boxes. [S12]
- Actions (buttons) go in one place, near the top or the bottom, not scattered; verbs, max three words. [S1]
- Use the `[Tooltip]` attribute (or tooltip property) on every field. [S11]
- Fit at 275 px width without horizontal scroll. [S1]

### 2.4 Multi-page onboarding / setup wizard window

What the sources say:
- NN/g: wizards fit newcomers and occasional tasks such as setup; offer a faster path for repeat users. Show every step, highlight the current one, short step names, fixed order, clear completed marks. Name the Back/Next buttons after their destinations. Pre-fill sensible defaults and remember past choices. [S18]
- NN/g on staged disclosure: fine when steps are independent; troublesome when steps depend on each other and force back-and-forth. [S9]
- Visa pattern (secondary, design system of a company): completed steps stay editable, buttons keep their position. [S16]
- Unity ES: for multi-step onboarding use a full-window flow; a day-zero empty state is for a few simple actions only. Empty-state copy: purpose first, one line of explanation, optionally one main button, encouraging tone. [S15, S4]
- Dialog/window titles match the label of the menu item that opened them. [S1]

Apply to us:
- Open from a clear menu item and an empty-state button, title matching the menu text.
- Left or top: step list with the current step highlighted and finished steps ticked; footer: Back and Next/Finish in a constant place. The last step is a short review of what will be written or changed (inference), and Finish does it as one undoable action if possible.
- Each step answers one question and has a default selected. Do not make users type what the project can detect. (inference)
- Show live validation on the current step only; block Next with a disabled button plus tooltip or a help box explaining why. [S1, S12]
- Keep each step to roughly one screen without scrolling at the window's default size. (inference)
- "Skip" or "Do this later" must exist and lead to a state that still works; the wizard can be reopened from Project Settings. (inference)

### 2.5 Runtime debug overlay (dense info, readable at a glance)

What the sources say (thin; mostly Unity's Rendering Debugger as the reference):
- Unity's runtime debug UI groups controls into tabbed panels of widgets, navigable by keyboard or gamepad, with a show/hide shortcut and a way to show only the active item compactly; it can be disabled for release builds. [S23]
- NN/g heuristics: visibility of system status, recognition over recall, minimalist design. [S20]
- Unity ES: do not rely on color alone; contrast at least 4.5:1 for text and 3:1 for icons and controls; text must tolerate size changes. [S1]

Apply to us (inference, built on the above):
- Default view shows 5-8 key values at most, with a label column (dim) and value column (bright), values aligned right or on a fixed column so eyes can scan down.
- Group by topic with a plain header; extra detail behind a toggle or tab. Highlight only what is abnormal (state mismatch, error) using the severity colors plus an icon or text marker, not color alone.
- Semi-opaque dark background behind text so it reads over any scene; anchor to a screen corner; never cover the center of the screen.
- Use a monospaced or tabular-number font for changing numbers so they do not jitter; do not update text faster than readable (about 4-10 times per second for values).
- Toggle with one key and a controller option; show the key hint on screen; compile out or hide in release builds.
- Make it scale with resolution; check it at the smallest supported size.

---

## 3. Anti-patterns (flag these in review)

- Boxes inside boxes around every group; bold headers on everything. [S6]
- Foldouts hiding the main controls, or foldouts nested more than two levels deep. [S6, S9]
- A foldout manually inserted inside a list instead of the list's own header. [S19, S7]
- Custom colors, fonts, logos or decorative icons that clash with the editor skin. [S1, S5]
- Color as the only severity cue; yellow/red text with no icon. [S1, S13]
- Messages that start with "Error:" or "Warning:", blame the user ("you did it wrong"), or use codes and internal names. [S13, S14]
- A bare "Something went wrong" or "Invalid settings" with no cause and no fix. [S14]
- A Fix button that does something surprising or cannot be undone; a "Fix All" that touches unrelated assets. (inference)
- Blank area where an empty list is; an empty state that uses a warning icon. [S15]
- Hiding buttons instead of disabling them; disabled controls with no tooltip. [S1, S10]
- Labels with colons, sentences as labels, labels over 3 words, ALL CAPS. [S1, S4]
- Tooltips used for warnings or as the only place important advice lives. [S11]
- Modal dialogs for ordinary validation; dialogs with no opt-out that repeat. [S13]
- Direct field writes that skip Undo; changes that do not mark the asset dirty. [S17]
- Wizard with generic "Next" only, hidden step count, or steps that depend on each other out of order. [S18, S9]
- Same problem listed repeatedly instead of grouped. (inference)
- Hard-coded pixel heights/spacing instead of EditorGUIUtility.singleLineHeight and standardVerticalSpacing (Unity advises using these). [S24]
- Horizontal scrolling at narrow widths; text clipped with no tooltip. [S1, S5]
- Overlay that shows every available value at once, or relies on small, low-contrast text. (inference, S1)

---

## 4. Sources

Unity (primary)
- [S1] UX Checklist (accessibility, color, errors, labels, tooltips, windows): https://unityeditordesignsystem.unity.com/foundations/ux-checklist
- [S2] Editor Foundations, UX Essentials (not reachable here; listed in site nav): https://foundations.unity.com/fundamentals/ux-essentials
- [S3] Project Settings and Preferences pattern: https://unityeditordesignsystem.unity.com/patterns/project-settings-preferences
- [S4] UI Copy Style: https://unityeditordesignsystem.unity.com/fundamentals/ui-copy-style
- [S5] Label component: https://unityeditordesignsystem.unity.com/components/label
- [S6] Content Organization pattern: https://unityeditordesignsystem.unity.com/patterns/content-organization
- [S7] Foldout component: https://unityeditordesignsystem.unity.com/components/foldout
- [S8] Older Unity Editor HIG, inspector organization (marked deprecated by Unity): https://unitytech.github.io/unityeditor-hig/best-practices/inspector_organization
- [S10] Interactions fundamentals (disabled, read-only states): https://unityeditordesignsystem.unity.com/fundamentals/interactions
- [S11] Tooltip component: https://unityeditordesignsystem.unity.com/components/tooltip
- [S12] Help Box component: https://unityeditordesignsystem.unity.com/components/help-box
- [S13] Errors and Messaging pattern: https://unityeditordesignsystem.unity.com/patterns/errors-and-messaging
- [S15] Empty States pattern: https://unityeditordesignsystem.unity.com/patterns/empty-states
- [S17] Create a custom Inspector (UI Toolkit manual): https://docs.unity.com/en-us/engine/current/manual/uitoolkits/uielements/uie-support-for-editor-ui/uie-how-to-create-custom-inspector
- [S19] Reorderable List component: https://unityeditordesignsystem.unity.com/components/reorderable-list
- [S19b] List View component: https://unityeditordesignsystem.unity.com/components/list-view
- [S21] Authoring Flows pattern (Inspector vs dedicated window): https://unityeditordesignsystem.unity.com/patterns/authoring-flows
- [S23] Rendering Debugger (runtime overlay structure): https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.0/manual/Rendering-Debugger.html
- [S24] EditorGUIUtility script reference (labelWidth, singleLineHeight, standardVerticalSpacing, wideMode): https://docs.unity.com/en-us/engine/current/script-reference/unityeditor/editorguiutility
- Icon guidance (status colors, light/dark variants, never color alone): https://unityeditordesignsystem.unity.com/fundamentals/iconography
- Contextual Tooling (prefer context-specific features, show only usable tools): https://unityeditordesignsystem.unity.com/patterns/contextual-tooling

Tool-UX writing
- [S9] Nielsen Norman Group, Progressive Disclosure (two levels, defer rare items, clear triggers): https://www.nngroup.com/articles/progressive-disclosure/
- [S14] Nielsen Norman Group, Error-Message Guidelines (12 guidelines): https://www.nngroup.com/articles/error-message-guidelines/
- [S18] Nielsen Norman Group, Wizards: https://www.nngroup.com/articles/wizards/
- [S20] Nielsen Norman Group, 10 Usability Heuristics: https://www.nngroup.com/articles/ten-usability-heuristics/
- [S16] Visa Product Design System, Wizard pattern (secondary, for editable completed steps and fixed button position): https://design.visa.com/patterns/wizard
- [S22] Odin Inspector, group attributes (foldout and tab groups): https://odininspector.com/tutorials/using-attributes/group-attributes
