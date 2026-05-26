using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    public class GhostPlayer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The visual representation of the ghost.")]
        [SerializeField] private Transform ghostVisuals;

        private GhostProfile profileToPlay;
        private float playbackTime;
        private int currentFrameIndex;
        private bool isPlaying;

        public void Play(GhostProfile _profile)
        {
            if (_profile == null || _profile.Frames.Count < 2)
            {
                gameObject.SetActive(false);
                return;
            }

            profileToPlay = _profile;
            playbackTime = 0f;
            currentFrameIndex = 0;
            isPlaying = true;
            
            // Snap to first frame immediately
            ghostVisuals.position = profileToPlay.Frames[0].Position;
            ghostVisuals.rotation = profileToPlay.Frames[0].Rotation;
        }

        private void Update()
        {
            if (!isPlaying || profileToPlay == null) return;

            playbackTime += Time.deltaTime;
            if (currentFrameIndex >= profileToPlay.Frames.Count - 1)
            {
                isPlaying = false;
                return;
            }

            // Advance the index if playback time has passed the NEXT frame's timestamp.
            while (currentFrameIndex < profileToPlay.Frames.Count - 2 && 
                   playbackTime > profileToPlay.Frames[currentFrameIndex + 1].Timestamp)
            {
                currentFrameIndex++;
            }

            GhostFrame frameA = profileToPlay.Frames[currentFrameIndex];
            GhostFrame frameB = profileToPlay.Frames[currentFrameIndex + 1];

            // Calculate interpolation factor (0 to 1) between the two frames
            float timeWindow = frameB.Timestamp - frameA.Timestamp;
            float lerpFactor = (playbackTime - frameA.Timestamp) / timeWindow;

            ghostVisuals.position = Vector3.Lerp(frameA.Position, frameB.Position, lerpFactor);
            ghostVisuals.rotation = Quaternion.Slerp(frameA.Rotation, frameB.Rotation, lerpFactor);
        }
    }
}