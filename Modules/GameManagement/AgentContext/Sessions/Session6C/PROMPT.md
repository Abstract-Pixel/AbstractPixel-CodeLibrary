# Prompt for Session 6C

> You are working in the AbstractPixelLibrary game management module (library at
> `Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary`; Rally is ProjectRallyShift). plan: gm-commercial, step 6C.
> Read `Modules/GameManagement/AgentContext/Sessions/Session6C/OVERVIEW.md` first and follow its reading list.
> Definition of done: `Modules/GameManagement/AgentContext/DEFINITION-OF-DONE.md`.
>
> 1. Settle the four points in the OVERVIEW's "Settle first": clear Session 4B's open checks, agree the branch with me,
>    don't push unless I say so, and get my OK on the watchdog's numbers.
> 2. **S6C-T1.** Write failing EditMode tests for the watchdog's rules, then the rules as plain C#: wanted effects
>    against applied effects, and no active state with time 0 outside a scene change. Must-have: both rules, plus tests
>    that they stay silent during eases, execution delays and scene changes.
> 3. **S6C-T2.** Connect it to the running game through the driver. Must-have: each warning appears once per problem,
>    not every frame; it says what, where and how to fix it, starts with `[Game management]`, changes nothing in the
>    game, and resets with domain reload off.
> 4. Finish as below.
>
> **Carry forward, do not drop (you don't build it in 6C).** The state inspector design is approved but not built.
> **Its UI must accommodate long names:** everywhere in the "What this state does over each parent" view and every
> name shown in it, including **UI input action map names**, **parent state names, including really long ones**, and
> long text in this view in general. Names stay on one line, are cut with "…" and show in full on hover; sentences
> wrap; nothing overlaps, scrolls sideways or pushes a control off from 275 to 1200 px. Full rules and test names:
> `AgentContext/UI-STYLE.md`, "Long names (required in the build)". **Input is still sample data:** both sides of the
> table show a fixed "UI", so the Input comparison does not work yet and must be built, not reused
> (`Sessions/Session6B/HANDOFF.md` §2). Put both points in your handoff and in the next session's OVERVIEW and PROMPT.
> Any prompt that builds the inspector (6B-build) must include the long-name requirement as a must-have with its own
> verification.
>
> **Done and verified**
>
> - End state: the two watchdog warnings exist and work in the Editor's Play mode. No inspector or settings UI changed.
> - Tests: written first; the whole `AbstractPixel.GameManagement.Tests` assembly passes, with the count in the handoff.
> - Clean compile with no new errors or C# warnings.
> - Play mode, real input (keys, gamepad, UI buttons), the sample's real settings: run its normal flows (start, pause,
>   settings, game over, win, retry, quit to menu, scene changes) and get **no** warning. Then cause each problem on
>   purpose (set time to 0 while the top state wants 1; leave no state active at time 0) and get exactly one clear
>   warning each.
> - Rally: parity scenarios compared with `Parity/baseline/` if runtime files Rally uses changed.
> - Unity 7 Lab: full module tests and the sample's PlayMode flow test.
> - `git status` of my uncommitted files matches before and after.
> - Handoff in `Sessions/Session6C/HANDOFF.html`, `Wayfinder/map.md` updated, next session's OVERVIEW and PROMPT
>   written, both carrying the long-name requirement and the Input note. Say which of 6A-build, 6B-build and 7A comes
>   next, and ask me if the plan doesn't settle it.
>
> Rules: stay inside ProjectRallyShift, Abstract_Pixel_Lab and your scratchpad, and put that rule in every subagent
> brief. One type per script; no `var`; no strings configure anything. Commit per task by path with the task ID first
> (`S6C-T1: …`). Never stage `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`,
> `Core/Runtime/SuperLogger/DebugDataBase.asset` or `CODING_STANDARDS.md.meta`. The UI prototype in the Lab stays
> untracked; don't delete it.
