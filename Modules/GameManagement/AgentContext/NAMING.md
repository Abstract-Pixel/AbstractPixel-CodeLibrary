# Naming and folders

How game management names its scripts and where it puts them. Written 2026-11-26 after the owner found names they
could not read on their own. Every session follows it for new code; Session 4B applies it to the existing code.

## The read-alone test

A name passes when someone who sees **only the file name**, in a search list or a stack trace, can say:

1. **what it is or does**, and
2. **that it belongs to game management.**

Clarity beats length, but long is not the fix. Aim for two to four words.

## Rules

- **Carry a module word.** Every public type has a word that ties it to the module: `State`, `GameState`,
  `Transition` or `GameManagement`. `RisingEdge` fails this; `StateRequester` passes. Internal helpers that never
  appear outside their folder may skip it, but should then be `internal` or nested.
- **Say the role at the end.** The last word says what kind of thing it is, using the words in [../GLOSSARY.md](../GLOSSARY.md):
  `...Requester` (a scene script that makes requests; today's `...Request` adapters are renamed in 4B), `...Reactor` (responds to state changes), `...Rule`, `...Settings`,
  `...Source`, `...Sink`. One role word per kind; do not invent a synonym.
- **No sentence names.** A name is a noun phrase, not a sentence fragment (`WhenLeavesGoTo` is a fragment).
- **No near-twins.** Two names that differ by one letter or one suffix (`StateRequests`, `StateRequester`,
  `StateRequestAdapter`) must differ in a way that tells you which is which.
- **Old words stay out.** Use the glossary's words, never its "Avoid" list (no "register", "priority", "evicted").

## Folders

- **At most 8 scripts per folder.** Past that, split by part, not by kind of type.
- Folders are named for a part of the module a developer looks for (`Requests`, `Reactors`, `Machine/Journal`), in the
  same words as the glossary.
- Moving or renaming: `git mv` with the `.meta` (keeps the GUID), `[MovedFrom]` for types held by `[SerializeReference]`,
  `[FormerlySerializedAs]` for fields. Grep Rally's scenes and prefabs before and after.

## Names flagged by the owner (2026-11-26)

What each does was checked in the code. Candidates are a starting point for Session 4B, not decisions; the owner
approves the final table.

| Today | What it does (from the code) | Why it fails | Candidates |
|---|---|---|---|
| `WhenLeavesGoTo` (component, `Runtime/Adapters/`) | When one chosen state leaves, requests another, e.g. "when Intro leaves, go to Play". Wraps `FollowUpRule`. | Sentence fragment; no module word; does not say it is a component that makes requests. | `NextStateRequester` |
| `FollowUpRule` (plain C#, `Runtime/Adapters/`) | The testable logic behind `WhenLeavesGoTo`: listens to `GameStates.Changed` and requests the next state when the watched one leaves. | "Follow up" does not say what follows what; no module word. | `NextStateRule` (pairs with `NextStateRequester`) |
| `RisingEdge` (plain C#, `Runtime/Adapters/`) | True once when a condition turns from false to true, so `PollRequest` does not send a request every frame. Used only by `PollRequest` and one test. | Electronics term; no module word; nothing says it is about polling. | Nest it inside `PollRequest` (allowed small extra type), or `PollBecameTrueCheck` |

## Other names to audit in Session 4B

Not flagged by the owner; the audit decides. Every `...Request` adapter (`BackRequest`, `EventRequest`,
`PollRequest`, `InputToggleRequest`) becomes a `...Requester`, and `StateRequester` (the UnityEvent button script) needs
a name that says it is for buttons. `StateRequests` / `StateRequester` / `StateRequestAdapter` (near-twins),
`StateWatch`, `GameHud`, `NoStateTimeScale`, `CursorModeRule`, `EffectSource`, `EffectResolver`, `TimeScaleEase`,
`EnterOnSceneStart`. The folder over the limit today is `Runtime/Adapters/` (12 scripts).
