# 05 Spawned Copies

Goblins spawned from one prefab, each saving its own health. A defeated goblin stays defeated.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Reload scene" button work, add the **Spawned Copies** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Spawned Copies** scene and press Play. Three goblins spawn.
2. **Hit** one goblin once and another twice. **Defeat** the third. Press **Save**.
3. Press **Reload scene**, or stop and press Play again.

Each goblin has the health it had, and the defeated one doesn't come back. **Bring every goblin back** spawns it again with full health.

## The lines that matter

```csharp
[Save] int health = MaxHealth;                         // Goblin, on the prefab with a Savable Bridge
Goblin goblin = Instantiate(goblinPrefab, ...);
goblin.SetSaveId($"goblin-{spawnPoint.name}");         // right after Instantiate, before its Start
SaveActions.Forget(goblin.gameObject); Destroy(...);  // a defeated copy's data goes away
[Save] List<string> defeatedSpawnPoints;               // GoblinSpawner, in the scene: who not to spawn
```

Every copy of a prefab has the same Savable Bridge, so each one needs its own save ID. Pick one that is the same every time the game runs, such as the spawn point's name, never a random number. A copy spawned with no `SetSaveId` isn't saved and logs one warning naming it. Two live copies with the same ID log one error, and only the first is saved.

`Forget` on a spawned copy removes its data. It doesn't stop the copy being spawned again, so the spawner remembers that itself.
