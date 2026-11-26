# Abstract Pixel Library

One Unity package (`com.abstractpixel.library`) with reusable code for any Abstract Pixel game. It is used by Project Rally Shift (Unity 6.3, as a git submodule in `LocalPackages/`) and Abstract Pixel Lab (Unity 7 alpha, through a `file:` entry). The library must also work on Unity 6.7.

## Two rules before adding any script

1. **Check that it doesn't exist already.** Before writing a new class, search the whole package for the idea, not only for the name:
   - Grep for the class name, likely synonyms and the Unity API it wraps (for example `Singleton`, `Pool`, `Timer`, `ServiceLocator`, `SceneReference`, `Logger`).
   - Look at the map below, especially `Core/Runtime/CommonUtilities`.
   - If something close exists, extend it or reuse it. Never add a second version. If two versions already exist, merge them into one (keeping GUIDs and saved data) instead of adding a third.
2. **It must be generic.** Code goes in the library only if more than one kind of game could use it unchanged:
   - No game names, game tags, layers, scene names, or references to a game's assets or types.
   - Anything a game might want differently is a setting: an inspector field, a ScriptableObject or an interface.
   - Code made for one game belongs in that game's project. Move it into the library only once it has been made generic.

If a request breaks either rule, say so and suggest where the code should go instead.

## Map

| Folder | What it holds | Ships in a build |
|---|---|---|
| `Core/Runtime` | Small shared tools: singletons, service locator, object pool, timers, logger (`SuperLogger`), scene data types, polymorphic attributes, UI helpers, entity factory | yes |
| `Core/Editor` | Editor tools for Core: attribute drawers, search dropdown, screenshot taker, mesh and texture tools | no |
| `Modules/<Name>` | One feature per folder: `Runtime/`, `Editor/`, `Tests/`, `Samples/`, its own asmdefs, README, sometimes a GLOSSARY | Runtime only |
| `EditorModules/<Name>` | Editor-only modules shared by other modules. The first is `Documentation`, the generic docs engine (topic content stays in its module) | never |

Modules: SaveSystem, SceneManagment (scene groups, scene sets, relocation, scene screens, docs), SceneTransitionSystem, Settings, GameManagement, LevelManager, InputRebinding, Rumble (plus Steam), GhostSystem, TooltipSystem, BroAudioTimeline.

Folder typos (`SceneManagment`, `Singltons`, `Utilties`) are known. Rename one only as a planned task that keeps GUIDs: renames break `.meta` links and serialized data.

## Saving tokens

- Start with the module's `README.md` and `GLOSSARY.md`, then its asmdefs (they show what a module depends on). Open source files only after that.
- Skip `*.meta`, `Samples/` and `Tests/` unless the task is about them. Also skip `Editor/Documentation/Content` (long topic text).
- Find things with Grep on type names. Don't read whole folders.
- The scene module is the largest (about 43k lines). Its plan, map and handoffs live in Rally's `.scratch/scene-coordinator/` (`plan.md`, `map.md`). Read the section you need, not the whole file.
- Root docs: `CHANGELOG.md` (history), `DOCUMENTATION-PLAN.md` and `EditorModules/Documentation/EDITOR-DOCS-UI-GUIDE.md` (how the in-editor docs are built and styled).

## How code is written here

- **Tests first.** Write an EditMode test, see it fail, write the code, see it pass. Never weaken or delete a test to make it pass.
- **Test as the developer and the player would.** A feature is not done until it was used in Play mode through real input (keys, gamepad, UI) with the project's real settings and preferences, and what the player sees was checked. Tests that only call methods or invoke buttons missed real bugs (Esc never paused Sample 01).
- **Plain English.**
  - No `var`, full words from the module's glossary, one idea per line.
  - Comments say why, not what.
  - Error messages say what went wrong, where, and how to fix it.
- **One type per file.** Each class, struct, enum or interface gets its own script, in a folder named for its part of the module (for example `Machine/Effects/`, `Machine/Clock/`). At most one extra small type may share a file, and only when it belongs to the main type (a tiny enum or record used only there). Never dump several classes, interfaces or data types into one script: the old `Machine/Effects.cs` (enums, settings, struct, two interfaces, a clock and a resolver in one file) is the example to avoid.
- **Small files.** Keep files to about 400 lines and split them by responsibility. Never merge files into a longer one. Delete dead code; don't comment it out.
- **Small daily API.** The 90% use case needs only a few public calls, and advanced features are reachable from them. Use `internal` for anything game code doesn't need. Make the wrong way impossible, or report it with the right way named.
- **Version guards.** Wrap Unity 7-only APIs in `#if UNITY_7000_0_OR_NEWER` so 6.3 and 6.7 still compile. Don't use `GetInstanceID`.
- **Domain reload off.** Reset statics with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Optional packages** (Addressables, BroAudio, Steam) go behind asmdef `versionDefines`. Without the package, the module still compiles and gives one clear error that names the package.
- **Saved data survives.**
  - Renaming a field, class or asset never loses data: use `FormerlySerializedAs`, `MovedFrom`, and `git mv` with the `.meta` file.
  - Editor code that changes assets records Undo and marks the asset dirty.

## Layout rules

- Every folder of code has its own asmdef, named `AbstractPixel.<Module>[.Part][.Editor|.Tests]`. Editor asmdefs set the Editor platform only.
- A runtime asmdef never references an editor asmdef.
- Samples live in `Modules/<Module>/Samples/` as editor-only assemblies (with the `UNITY_EDITOR` constraint and `autoReferenced: false`). They are never copied into a project's `Assets` and never go into a build.
- Each module has a README. A module with its own words also has a `GLOSSARY.md`, and new names come from it.

## Git

- Commit only what your task changed: stage by path, one commit per task, with the task ID first in the message. Never push.
- Never stage `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`.
- Library commits go in this repo. The Lab never commits library files. Rally commits only the new submodule pointer.
- After a Play session, revert churn you didn't mean to make, such as a `SceneSystemSettings.asset` rewrite or font assets.
