# Scene sets fill scene groups, and a group's overrides persist

<!-- layout new -->

<!-- decision -->

A scene set is a default, not a live link that owns the data. This is the prefab override model Unity designers already know.

## The decision

- **The group keeps its lists:** each scene group still stores its own manager scenes, shared scenes and Reset shared scenes choice, which the loader reads as before.
- **It names its set:** and records, for each of those three parts, whether it is an override.
- **Not an override:** the part is copied from the set whenever the set, or the group's choice of set, changes.
- **An override:** never touched by the set, by saving or by reimporting, until the designer reverts it.

## Considered options

| Option | Why not |
|---|---|
| The group reads its scenes from the set at runtime and stores only extras | Every loader and the save copy would change, a deleted set would empty every group, and a designer could not change one level without making a new set |
| Copy the set once when it is picked, with no link afterwards | Changing the "Level" set would mean editing every level again, the mistake scene sets exist to remove |

## Consequences

- **Runtime unchanged:** the runtime and `SceneGroupData` do not change.
- **Kept in step:** an editor hook keeps the non-override parts of every group in step with its set.
- **No set:** groups with no set behave as before, with every part their own.
