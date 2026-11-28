# Decisions from the planning grill (2026-11-25)

The user agreed with each recommendation unless a change is listed. These override anything else in AgentContext.

## Round 1

1. **Destination.** Commercial-ready means it could ship on the Unity Asset Store now. Target about 90%. The real goal is
   stability, easy setup, easy changes, easy debugging, and spending no tokens on game management in future games.
   Rally is the dogfood project: migrate it as you go, and it must keep working as it does on Steam.
2. **Time model.** One owner works out `Time.timeScale` from the top state. No save and restore.
3. **Sub-states.** Keep and redesign with parents. Change: a sub-state may have **several parents** (one Settings for
   Main Menu and Pause), set by dragged references, no strings.
4. **Priority.** Ordered state catalog. Change: needs **UI validation** that says what conflicts, where, and the
   possible fixes, in an elegant, non-overwhelming way.
5. **Remove `GameStateComponent`.** The user is skeptical but agrees if setup stays easy and every Rally flow works as
   now. See round 2, Q1.
6. **Request queue** instead of silent drops; every refusal logged with a reason.
7. **Guards in three layers:** editor validation, play-mode watchdog, wrong setups made impossible by field design.
8. **Debugger:** always-on cheap transition journal; UI Toolkit overlay in the Editor and Development Builds; release opt-in.
9. **Reactors DRY:** one composed state-watch helper.
10. **Cuts** (`PauseStateSO`, revert flag, HUD-on-leave flag, merged time fields). Change: only if the result behaves
    exactly as before, with less code and easier setup.
11. **Plan location:** `AgentContext/` in the module. Commit only when asked.

Extra from round 1: a **documentation session** on the scene system's documentation engine, and an **onboarding window**
that creates the menus every game needs (Main Menu, Pause, Game Over, Game Win). Settings is out of scope (it needs its
own commercial-ready plan).

## Round 2

1. **Parity before deletion.** Change: the user's flow list was only examples. Before implementing, **inventory every
   flow** the current system handles in Rally, build the new system, verify each flow matches or improves, and only then
   delete the old system. Answer to the user's question: yes, game management gets its own page,
   **Project Settings > Abstract Pixel > Game Management**, next to Scene System, plus its own topics in the in-Editor docs.
2. **Multi-parent effects** default to "Same as parent". Change: validation must cover **multi-parent sub-states**.
3. **Input.** Change: no global input class may be assumed; each game has its own actions asset. Answer: the input source
   seam in [DESIGN.md](DESIGN.md#input-without-a-global-class). Audio volume reactor moves to the library; blur and the
   death-sequence skip stay in Rally on the state watch.
4. **Validation UI.** Keep it unified with the scene system's look. Change: **no separate checker window.** The global
   overview and problems live in the Project Settings page, modelled on the Scene Group debugger. Per-state details stay
   in their own assets.
5. **Onboarding.** Change: a page to **choose which menus** a game needs (some games have no Win). Each menu can be
   **duplicated from the library's base menus or the module sets itself up on the game's own menu**; test both paths.
   Start and Pause menus keep a Settings button. Menus get **back/cancel**. Menus are **uGUI**; only editor tooling and
   the runtime debugger use UI Toolkit.
6. **Docs session** on the documentation engine after the API stops changing; README and GLOSSARY early.
7. **Tests.** Change: **one test folder with one test assembly** under the module (`Tests/`), not scattered like the
   scene system.
8. **Out of the 90%:** Asset Store packaging, demo art, translations, old Input Manager, non-BroAudio audio. Change:
   **do fix the folder typo** (`GameManagment` to `GameManagement`) and everything that references it.
9. **Budget per session** from history plus estimate, each task with a must-have line.

## Round 3

1. **Pause is a sub-state of Play.** Closing Pause returns to Play; Rally no longer needs a "when Pause leaves, start
   Play" trigger. Settings is one sub-state with two parents: Main Menu and Pause.
2. **Rally's 7 state assets move into Rally** (`Assets/[1]Project/ProjectServices/GameManagment/States/`) with
   `git mv` and their `.meta` files. The library keeps only starter templates for onboarding.
3. **Library menus are graphics-free** uGUI with **TextMeshPro** text (never the legacy UI `Text`). Rally keeps its
   styled menus and tests the "set up on my own menu" path.
4. **Branches:** a new branch `game-management-rework` in the library and in Rally, made from `main`. If the other
   agent's merge into `main` has not landed yet, branch from `scene-system-rework` (library) and the current Rally branch.
5. **Static facade `GameStates`**, no ServiceLocator, no bridge for the old API. Session 4 updates every Rally script and
   AgentScripts probe that calls the old API.
6. **Execution delay:** the state is in control during its delay (other requests refused, "on enter" menus show); time,
   cursor, input and HUD change when the delay ends. Skip-delay stays.
7. **No state active (between scenes).** Changed: the user worries that time 1 during a scene change lets the already
   loaded shared scenes (bomb, car, timers) run before the new level is ready. Decision: the default is **keep the last
   time scale**, which matches today. "Set time to 1" is an option in the settings asset. Session 4 runs the
   transition break scenarios (see PLAN-OVERVIEW) with both options and records the result; the default changes only if
   the user asks after seeing it. The play-mode watchdog warns if no state is active and time is 0 for more than a few
   seconds outside a scene change.
8. **Closing a parent closes its sub-states first**, each step recorded in the journal.
9. **Settings asset location:** a default location, and **the user can choose where it lives** (object field on the
   Project Settings page). It must load in builds wherever it is: copy the scene system's loading method if it is
   location-independent, otherwise use Player Settings > Preloaded Assets.

## 2026-11-26: grill round 1 and follow-up

1. **No strings configure anything.** Typing a string to configure something is error-prone for any developer or agent:
   a typo or a rename fails silently. So no inspector field, settings asset or setup API takes a string ID, name or key.
   Use dragged asset references, typed values or enums. Strings remain only for labels, logs and messages.
2. **Input maps are chosen by action reference.** A state's Input effect lists `InputActionReference`s; each turns on
   the map that holds the referenced action. This replaces the string map IDs and their dropdown (Session 3), uses the
   same kind of reference the requesters already use for keys, and survives map renames. Replaced in Session 4,
   before Rally's assets are migrated, so the migration writes references.
3. **Session 4 has 18%** in two parts: Session 4 (13%) and Session 4B, naming, folders and core cleanup (5%), run as
   a fresh session after it. If Session 4 runs short after the parity gate, deleting the old system moves to 4B.
4. **Keep the given budget** even when the scope check asks for more; do the must-haves first.
5. **Unity 7 checks run full tests in the Lab,** which has the library as a git submodule used as a local package:
   move its submodule to the pushed commit and run the module tests and the sample's flow test there.
6. **Each prompt says what to read before each stage,** so agents follow decisions already made instead of making
   their own.
7. **Commit by default,** per task, and push.
8. **Requester** replaces "Condition" as the word for a scene script that makes requests.

## 2026-11-26 (Session 4): sample on the new system only

The owner decided Sample 01 Minimal Game is built on the new state machine only, not on the old components first. Its `Parity/baseline.json` is recorded on the new system in Rally (Unity 6.3) and the Unity 7 Lab run must match it. Session 4 task 4 (migrate the sample) is dropped.

## 2026-11-26 (Session 4, second run): owner calls

- **Test as the developer and the player would.** Added to DEFINITION-OF-DONE.md (item 5) and the library CLAUDE.md. Reason: the button-only flow test missed that Esc never paused Sample 01 and that Settings left the Pause menu showing.
- **Input is configured, never hard-coded, like Rally's old system.** Each adapter takes one `InputActionReference`; each device is one binding on that action (Pause: Esc, Start, the left Share/Select button; Back: Backspace, the East button). Adapters enable their action.
- **Back goes back one step by default; `BackRequest.alsoLeave` leaves more states** (Settings straight back to Play when Pause is listed). One press goes back once, so Settings > Back stops at Pause. Back on Pause resumes Play.
- **A menu hides while a sub-state covers its state** (`StateMenuReactor.hideWhileCovered`, on by default) and shows again when the sub-state leaves.
- **Scene system: the preference "Open a scene in no group with the default scene set" now also decides Play.** Off: a scene in no group plays alone, with no manager or shared scenes (replaces 4B Q5 "Play always loads them").
- **`game-management-rework` is merged into library `main`, not the other way round.** Merging `main` into the branch would break Rally (it removes `[Savable]`/`SaveCategory`, so Rally would need the save-system V2 migration, which the owner does not want now). The Lab is on `main` with the merge commits; Rally stays on `game-management-rework`.

## 2026-11-27 (Session 5): UI design grill

Full answers (Q1-Q25): [Sessions/Session5/GRILL-RESULTS.md](Sessions/Session5/GRILL-RESULTS.md). The calls that change earlier decisions or need remembering:

- **Multi-parent conflict is not a problem.** A sub-state that follows each parent's effects behaving differently per parent is intended. The state Inspector shows a read-only "What this state does over each parent" table. The only multi-parent problems are self-parent or loop (error), an empty parent slot and the same parent twice (warnings). Severity: error = can never work, warning = messy; no info level.
- **Settings-asset picker** (round 3 Q9): the scene page's Setup card (status dot + object field, asset path + Move…, Create settings, Fix now). **No multi-asset "Keep this one" list**: other settings assets are ignored silently.
- **Debugger diff view in scope** as changed-only lines in the Editor debugger window's row details; a full side-by-side compare is a Session 8 nice-to-have. The in-game overlay is read-only and shown by an InputActionReference (no hard-coded key).
- **Shared editor look:** `SceneSystemUI`'s generic helpers move to a shared `AbstractPixel.Core.Editor.UI` assembly at the start of Session 6; both modules use it.
- **Onboarding is the one place that asks which states and menus exist.** Each menu row chooses "Copy the library's menu" or "Use my own" (extra setup, Learn more). The Settings page's empty States card links to it.
- **Setup windows get a system pill header** under the Abstract Pixel logo ("Game Management"; the scene setup window gets "Scene System").
- Requesters and Reactors keep stock inspectors; one shared state field drawer flags states not in the list.

## 2026-11-28 (ticket 04): branches and push

- **Branch:** game management code is committed on `game-management-rework` and merged into `main`, never the other way. Docs and the Wayfinder map are committed on `main` only. Code that exists only on `main` (save system v2) is committed on `main`.
- **Push:** push per ticket. A closed ticket whose tests pass pushes the library branches it changed. Nothing else pushes. This replaces "never push" in the library CLAUDE.md.
