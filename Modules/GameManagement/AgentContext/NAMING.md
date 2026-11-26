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

## Renames done in Session 4B (2026-11-26)

The owner's three flagged names, plus every other name the audit found failing the read-alone test. No behaviour
change; `[FormerlySerializedAs]`/`[MovedFrom]` were not needed because none of these types are referenced by
field name or `[SerializeReference]` in scenes, prefabs or assets (`m_Script` GUIDs carried the rename). Rally's
`GameManagment.prefab`, its menu scenes and `Samples/01 Minimal Game` had their `m_EditorClassIdentifier` and
UnityEvent `m_TargetAssemblyTypeName` strings updated to match (grepped before and after).

| Today | Was | Why it failed |
|---|---|---|
| `SceneStartRequester` (`Runtime/Requesters/`) | `EnterOnSceneStart` | No module word, and every other `...Request` sibling became `...Requester`. |
| `EventRequester` | `EventRequest` | `...Request` adapters become `...Requester` (role word; a scene component is never the request itself). |
| `PollRequester` | `PollRequest` | Same. |
| `InputToggleRequester` | `InputToggleRequest` | Same. |
| `BackRequester` | `BackRequest` | Same. |
| `StateButtonRequester` | `StateRequester` | Didn't say it was for UnityEvent buttons; also a near-twin of `StateRequestAdapter`/`StateRequestRules`. |
| `NextStateRequester` (`Runtime/Requesters/`) | `WhenLeavesGoTo` | Sentence fragment; no module word; didn't say it makes requests. |
| `NextStateRule` (`Runtime/Requesters/Shared/`) | `FollowUpRule` | "Follow up" didn't say what follows what. |
| `PollBecameTrueCheck` (`Runtime/Requesters/Shared/`) | `RisingEdge` | Electronics term; no tie to polling. (Kept as its own class, not nested, because `Tests/Requesters/RequesterTests.cs` constructs it directly.) |
| `StateRequesterBase` (`Runtime/Requesters/Shared/`) | `StateRequestAdapter` | "Adapter" isn't a glossary role word; renamed for the `...Requester` family it is the base of. |
| `StateRequestRules` (`Runtime/Requesters/Shared/`) | `StateRequests` | Near-twin of `StateRequester`/`StateButtonRequester`. |
| `StateEffectSource` (`Runtime/Machine/Effects/`) | `EffectSource` | No module word at all (unlike `GameEffects`, `StateEffectSettings`). |
| `StateEffectResolver` (`Runtime/Machine/Effects/`) | `EffectResolver` | Same. |

Audited and left unchanged (already pass, or are `internal`-exempt / not worth the churn this session):
`StateRequestKind`, `StateWatch`, `GameHud` (carries "Game"), `NoStateTimeScale`, `CursorModeRule` and
`TimeScaleEase` (both tested directly by name from `Tests/`, so renaming them for internal-only exemption would
have meant touching test assembly visibility, deferred), `IInputSource`, `InputSourceKind`, `ITimeScaleTarget`,
`MenuRevealTiming`. `GameCursorController` stays, per Session 4's own note.

The folder that was over the limit, `Runtime/Adapters/` (12 scripts), is now `Runtime/Requesters/` (7 scripts:
the trigger components) and `Runtime/Requesters/Shared/` (5 scripts: the base class, the enum, the static rules
and the two plain-C# helpers).
