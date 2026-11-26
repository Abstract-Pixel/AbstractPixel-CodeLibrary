# Scene documentation lives in an Editor window, not in HTML

<!-- layout new -->

<!-- decision -->

The scene system's documentation is a UI Toolkit window inside the Unity Editor, not static HTML in a browser like the save system's.

## The decision

- **Open it:** Tools > External Project Tools > Scene System > Open Scene Documentation.
- **Pages:** Markdown files in the module's Editor folder, with the sample READMEs used as sample pages.
- **One application:** users stay in Unity, and a page can act on the project: select or ping an object, open a Project Settings section, follow the sample scene that is open, and show what happens in Play.
- **No browser bridge:** the local ping server, custom URL protocols and `file://` redirect pages the HTML docs needed go away.
- **The cost:** our own Markdown renderer and window, and pages that read outside Unity only as plain Markdown on GitHub.

## Help buttons

- **Unity's "?" can't do it:** the component header "?" (`HelpURLAttribute`) can only open an `http`, `https` or `file:///` URL, and Unity reads the URL when the header is first drawn, so it cannot open an Editor window safely.
- **Our own buttons:** help buttons are drawn by the scene system's own windows, page sections and inspectors, all through one entry point, `SceneSystemDocs.Open(topicId)`.
