# When a scene change fails <!-- level 1 -->

<!-- layout new -->

When a scene of the group does not load, the change fails, `OnSceneChangeFailed` fires once and the scene system goes to the [fallback scene group](term:fallback-scene-group).

## Pick the fallback scene group <!-- level 0 -->

Set "If a scene change fails, go to" in Project Settings > Abstract Pixel > Scene System > Start-up. Empty means the [start scene group](term:start-scene-group).

<!-- action open-settings "Start-up" "Open Start-up" -->

## What happens <!-- level 1 -->

The screen stays covered until the scene system has gone somewhere safe.

1. The change records what is really open and sends `OnSceneChangeFailed`.
2. The fallback scene group opens once, with the screen still covered.
3. If the fallback fails too, the screen shows whatever is open and an error is logged.

> [!NOTE]
> A failed load is never tried again. A scene that fails to close is logged and the change carries on.

<details><summary>Why it works</summary>

- **A known place:** going to a known group keeps the player out of a half-open place ([design decision 11](../../../../docs/adr/0011-a-failed-scene-change-goes-to-the-fallback-scene-group.md)).

</details>

<details><summary>If it goes wrong</summary>

- **Failed with a scene that is Not in build:** fix its [scene status](term:scene-status).
- **Failed for an Addressable scene:** check its address for `[` or `]`.

</details>
