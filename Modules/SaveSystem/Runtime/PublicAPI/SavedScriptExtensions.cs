using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Lets any saved script on a spawned copy's GameObject give it its own save ID.</summary>
    public static class SavedScriptExtensions
    {
        /// <summary>
        /// Gives a spawned copy its own save ID, so it doesn't share its prefab's. Call it right after Instantiate,
        /// before the copy's Start.
        /// Example: <c>Goblin goblin = Instantiate(goblinPrefab); goblin.SetSaveId($"goblin-{spawnPoint.name}");</c>
        /// </summary>
        public static void SetSaveId(this MonoBehaviour savedScript, string saveId)
        {
            SavableBridge bridge = savedScript.GetComponent<SavableBridge>();
            if (bridge == null)
            {
                SaveLog.Error($"[SaveSystem] SetSaveId on '{savedScript.gameObject.name}' needs a Savable Bridge on the same GameObject.");
                return;
            }
            bridge.SetSaveId(saveId);
        }
    }
}
