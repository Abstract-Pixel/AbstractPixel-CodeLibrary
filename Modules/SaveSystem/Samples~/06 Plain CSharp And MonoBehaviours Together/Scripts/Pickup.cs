using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.Together
{
    // A pickup in the scene. Each one has its own Savable Bridge, so each has its own save ID.
    // Pickups belong to their scene, so they're saved in a Scene save file (SamplePickupsSaveFile).
    public class Pickup : MonoBehaviour, ISavable<bool, SamplePickupsSaveFile>
    {
        [SerializeField] string displayName;

        public string DisplayName => displayName;
        public bool IsCollected { get; private set; }

        void Awake()
        {
            IsCollected = false;
        }

        // Picking something up also completes a quest in the plain C# quest log.
        public void Collect(QuestLog questLog)
        {
            IsCollected = true;
            questLog.Complete($"Found the {displayName}");
        }

        public void PutBack()
        {
            IsCollected = false;
        }

        public bool CaptureData() => IsCollected;

        public void RestoreData(bool _loadedData) => IsCollected = _loadedData;
    }
}
