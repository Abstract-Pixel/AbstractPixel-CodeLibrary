# Scene Management

Opens, closes and switches groups of scenes for a single-player Unity game: a main scene plus the manager scenes and shared scenes that come with it, with optional preload, Addressables, a loading scene, relocation and a screen effect around each change.

## Start here

- **Read the scene documentation in the Editor:** Tools > External Project Tools > Scene System > Open Scene Documentation. Every Scene System window, inspector and Project Settings section also has a help button that opens it at the part about that control. The pages are Markdown in `Editor/Documentation/Pages/`; the sample pages are the `Samples/NN .../README.md` files.
- **Set up a project:** Tools > External Project Tools > Scene System > Set Up Scene System, then Project Settings > Abstract Pixel > Scene System.
- **Change scene:** `SceneActions.ChangeTo(group)`, `Retry()` and `Preload(group)`, each returning a `SceneChangeResult`; or `MonoSceneActions` on a UnityEvent.
- **Words:** [GLOSSARY.md](GLOSSARY.md). Use these terms in code, docs and issues.
- **Why it is built this way:** [docs/adr/](docs/adr/), one short file per decision that is hard to reverse.
- **Samples:** [Samples/README.md](Samples/README.md), ten samples from a no-code button to a launch scene.

What used to be here (editor tools, the scene picker, scene messages and their order, the loading scene, which loader opens a scene, relocation) now lives in the scene documentation's Guide pages, with every fact kept.
