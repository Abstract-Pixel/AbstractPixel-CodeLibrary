# Scene sets fill scene groups, and a group's overrides persist

A scene set is a default, not a live link that owns the data. Each scene group still stores its own manager scenes, shared scenes and Reset shared scenes choice, which the loader reads as before. The group also names its scene set and records, for each of those three parts, whether it is an override. A part that is not an override is copied from the set whenever the set or the group's choice of set changes. A part that is an override is never touched by the set, by saving or by reimporting, until the designer reverts it. This is the prefab override model Unity designers already know.

## Considered options

- The group reads its scenes from the set at runtime and stores only extras. Rejected: every loader and the save copy would change, a deleted set would empty every group, and a designer could not change one level without making a new set.
- Copy the set once when it is picked, with no link afterwards. Rejected: changing the "Level" set would mean editing every level again, which is the mistake scene sets exist to remove.

## Consequences

The runtime and `SceneGroupData` do not change. An editor hook keeps the non-override parts of every group in step with its set. Groups with no set behave as before, with every part their own.
