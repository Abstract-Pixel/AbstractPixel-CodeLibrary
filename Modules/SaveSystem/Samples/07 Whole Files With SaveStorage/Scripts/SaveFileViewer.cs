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
            GUILayout.BeginArea(new Rect(20, 20, 700, 700));
            GUILayout.Label($"Visits (a [Save] field in this file): {VisitCounter.Visits}");
            GUILayout.Label("File: " + (IsGameProfileOpen ? SaveStorage.GetFilePath<SaveFiles.GameProfile>() : "none, no game profile is open"));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Count a visit"))
            {
                VisitCounter.CountAVisit();
            }
            if (GUILayout.Button("Save"))
            {
                SaveActions.SaveAll();
                ReadFile();
            }
            if (GUILayout.Button("Read the file again"))
            {
                ReadFile();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy the file's text"))
            {
                GUIUtility.systemCopyBuffer = FileText;
            }
            if (GUILayout.Button("Paste text into the file"))
            {
                WriteFile(GUIUtility.systemCopyBuffer);
            }
            if (GUILayout.Button("Delete the file"))
            {
                DeleteFile();
            }
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(520));
            GUILayout.TextArea(FileText);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
