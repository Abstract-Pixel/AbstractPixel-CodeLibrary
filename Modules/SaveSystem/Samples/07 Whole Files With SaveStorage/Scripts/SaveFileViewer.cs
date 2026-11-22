using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.WholeFiles
{
    // A debug screen for the open game profile's GameProfile save file, read and written whole as JSON text.
    // This is what SaveStorage is for: tools, debug buttons, copying a player's save to reproduce a bug.
    // Game code saves through [Save] fields and SaveActions instead.
    public class SaveFileViewer : MonoBehaviour
    {
        Vector2 scrollPosition;

        public VisitCounter VisitCounter { get; private set; }

        /// <summary>The file's text as last read, or a note saying why there is none.</summary>
        public string FileText { get; private set; } = "";

        void Awake()
        {
            VisitCounter = new VisitCounter();
        }

        void Start()
        {
            ReadFile();
        }

        void OnDestroy()
        {
            SaveActions.Unregister(VisitCounter);
        }

        // A GameProfile file belongs to the open game profile, so with none open there is no file to name or read.
        static bool IsGameProfileOpen => SaveActions.GameProfiles.Current != null;

        public void ReadFile()
        {
            if (!IsGameProfileOpen)
            {
                FileText = "(no game profile is open, so there is no GameProfile file)";
                return;
            }
            bool found = SaveStorage.ReadText<SaveFiles.GameProfile>(out string json);
            FileText = found ? json : "(no GameProfile file yet: press Save first)";
        }

        // In play mode WriteText also restores every registered object in that file from the new text.
        // Text that isn't a whole save file is refused, logged, and changes nothing.
        public bool WriteFile(string json)
        {
            bool written = SaveStorage.WriteText<SaveFiles.GameProfile>(json);
            ReadFile();
            return written;
        }

        // Deletes the file and its backup. The running save system forgets its copy too, so the next save can't
        // write the old data back.
        public void DeleteFile()
        {
            SaveStorage.Delete<SaveFiles.GameProfile>();
            ReadFile();
        }

        void OnGUI()
        {
            SampleGui.BeginPanel(20, 20, 820, "The GameProfile file, as text", IsGameProfileOpen ? SaveStorage.GetFilePath<SaveFiles.GameProfile>() : "No game profile is open, so there is no file.");
            SampleGui.BeginRow();
            SampleGui.Value("Visits, a [Save] field in this file: ", VisitCounter.Visits.ToString());
            GUILayout.FlexibleSpace();
            if (SampleGui.SmallButton("Count a visit", 200))
            {
                VisitCounter.CountAVisit();
            }
            SampleGui.EndRow();

            SampleGui.Section("Whole file with SaveStorage");
            SampleGui.BeginRow();
            if (SampleGui.Button("Read the file again"))
            {
                ReadFile();
            }
            if (SampleGui.Button("Copy the file's text"))
            {
                GUIUtility.systemCopyBuffer = FileText;
            }
            if (SampleGui.Button("Paste text into the file"))
            {
                WriteFile(GUIUtility.systemCopyBuffer);
            }
            if (SampleGui.Button("Delete the file"))
            {
                DeleteFile();
            }
            SampleGui.EndRow();

            // Read-only: the file changes only through the buttons above, so nothing typed here is silently lost.
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            SampleGui.Code(FileText);
            GUILayout.EndScrollView();

            SampleGui.Footer(ReadFile);
            SampleGui.EndPanel();
        }
    }
}
