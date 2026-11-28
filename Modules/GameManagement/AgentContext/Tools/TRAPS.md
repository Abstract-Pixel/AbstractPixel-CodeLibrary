# Known traps and shared tools

Every game management session reads this before it touches a repo or an Editor. When you hit a new trap that another
session would hit too, add one line here (builder and verifier both). Delete a line when its cause is fixed.

## Tools (use these before writing your own)

Run from the Lab in Git Bash. `T=LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/Tools`

| Need | Use |
|---|---|
| State of all four repos and both Editors | `bash $T/repo_status.sh` (`--no-editors` for git only) |
| Wait for a recompile | `bash $T/wait_compile.sh <lab\|rally> <assembly>` then read the console |
| Which passed checks are still valid | `bash $T/check_stale.sh <session folder>/CHECKS.md` |
| Open, resize, close or capture an Editor window | `EditorProbe.cs`: `Open`, `Resize`, `CloseWin`, `Grab` |
| Queued keyboard or gamepad input in Play mode | `EditorProbe.cs`: `Key`, `Pad` |
| Read and press pop-ups, focus an Editor | `../ProjectRallyShift/AgentScripts/Tools/` |

`EditorProbe.cs` entries run with `unity command script run --file "$(cygpath -w $T/EditorProbe.cs)" --entry
"EditorProbe.<Method>" --args '[...]'`. Before `Grab`, bring the Editor to the front and wait 5 s. Read every still.

A probe only one ticket needs stays in the scratchpad. If a second ticket would need it, add it to `Tools/`. Nothing
saved under `Sessions/` or `Tools/` may point at a scratchpad path.

## Editor traps

- Stop Play mode before `unity command test run` (`unity command editor.playmode stop`). A test run started in Play
  hangs and blocks every later command. If it already hung, a real Ctrl+P in the Lab frees it.
- Command names differ: the Lab takes `editor status` and `script run`; Rally takes `editor_status` and `run_script`.
- `recompile_status` can be stale. The DLL must be newer than the source (`wait_compile.sh`). A checkout or merge
  changes file times without changing code, and Unity then keeps the old DLL. If you get STALE right after one,
  compare the file's content with what was last compiled; if code did change, the DLL really is stale.
- Never `capture editor_window` on the Lab: it crashes Unity 7 alpha.
- `EditorApplication.delayCall` does not run while the Editor is in the background. Foreground it first.
- `EditorWindow.SendEvent` does not reach the shortcut manager.
- Screen-pixel stills capture whatever is on top. Close old Project Settings and Properties windows first, and focus
  Rally with `focus_unity_project.ps1`.
- A Properties window opened right after closing another can come up blank. Open it without a prior close.
- A window needs 3 to 5 s to paint after opening.
- UnityEvent calls added by script run only in Play mode. EditMode tests set them to `EditorAndRuntime`.
- Library menu prefabs are saved with the root switched off, so a reactor on the root never runs.
- Unity's `ListView` overrides a row's padding. Pad the row's body.

## Repo traps

- A module test run can add a dead preload line to the Lab's `ProjectSettings.asset`. Check it after tests.
- Rally's recompile regenerates `ProjectRallyShift.slnx`. Revert it.
- Saving a sample state asset rewrites old serialized fields. Back it up first and restore it byte for byte.
- With no settings of its own the Lab uses Sample 01's, so "Add to list" writes into the library's sample catalog.
- Creating `PanelSettings` can create a stray `Assets/UI Toolkit/` theme in the Lab.
- Before saying a feature works in Rally: Rally's library is on the tested commit, its DLL is newer than the source,
  and you opened the feature the way a user would.

## Shell traps

- Long Bash heredocs fail on some quote patterns. Write files with the Write tool.
