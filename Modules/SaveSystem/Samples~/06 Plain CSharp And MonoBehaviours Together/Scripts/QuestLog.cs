using System;
using System.Collections.Generic;
using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.Together
{
    [Serializable]
    public class QuestLogData
    {
        public List<string> CompletedQuests = new List<string>();
    }

    // A plain C# quest log, saved in the game profile under its own save ID.
    [Savable(SaveCategory.Game)]
    public class QuestLog : ISavable<QuestLogData>
    {
        public List<string> CompletedQuests { get; private set; } = new List<string>();

        public QuestLog()
        {
            // Register last: the saved quests may be restored inside this call.
            SaveActions.Register(this, "quest-log");
        }

        public void Complete(string quest)
        {
            if (!CompletedQuests.Contains(quest))
            {
                CompletedQuests.Add(quest);
            }
        }

        public void Clear()
        {
            CompletedQuests.Clear();
        }

        public QuestLogData CaptureData() => new QuestLogData { CompletedQuests = new List<string>(CompletedQuests) };

        public void RestoreData(QuestLogData _loadedData) => CompletedQuests = new List<string>(_loadedData.CompletedQuests);
    }
}
