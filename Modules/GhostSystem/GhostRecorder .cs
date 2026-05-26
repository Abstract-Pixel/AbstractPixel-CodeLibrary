using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    public class GhostRecorder : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The transform to track (usually the Player Car).")]
        [SerializeField] private Transform targetTransform;

        [Header("Settings")]
        [Tooltip("How often to record a frame. 0.1 = 10 frames per second. Higher = smaller save files.")]
        [SerializeField] private float recordInterval = 0.1f;
        [SerializeField] private bool autoStart = true;

        private GhostProfile currentProfile;
        private float timeSinceLastRecord;
        private float elapsedRunTime;
        private bool isRecording;

        private void OnEnable()
        {
            GhostActions.RequestFinalGhostProfile += RetrieveFinalProfile;
        }

        private void OnDisable()
        {
            GhostActions.RequestFinalGhostProfile -= RetrieveFinalProfile;
        }

        private void Start()
        {
            if (autoStart)
            {
                StartRecording();
            }
        }

        private void Update()
        {
            if (!isRecording || targetTransform == null) return;

            elapsedRunTime += Time.deltaTime;
            timeSinceLastRecord += Time.deltaTime;

            if (timeSinceLastRecord >= recordInterval)
            {
                RecordFrame();
                timeSinceLastRecord = 0f;
            }
        }

        public void StartRecording()
        {
            currentProfile = new GhostProfile();
            elapsedRunTime = 0f;
            timeSinceLastRecord = 0f;
            isRecording = true;
            
            // Always record the very first frame
            RecordFrame();
        }

        public void StopRecording()
        {
            isRecording = false;
            if (currentProfile != null)
            {
                currentProfile.TotalRunTime = elapsedRunTime;
            }
        }

        private void RecordFrame()
        {
            GhostFrame newFrame = new GhostFrame(elapsedRunTime, targetTransform.position, targetTransform.rotation);
            currentProfile.Frames.Add(newFrame);
        }

        private GhostProfile RetrieveFinalProfile()
        {
            StopRecording();
            return currentProfile;
        }
    }
}