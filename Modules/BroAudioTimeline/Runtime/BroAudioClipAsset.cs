using Ami.BroAudio;
using Ami.BroAudio.Data;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// One BroAudio sound on a BroAudioTrack. The sound starts when the playhead enters the clip and stops when it
    /// leaves or the Timeline stops. The curves run across the clip's length (0 = clip start, 1 = clip end) and
    /// multiply the entity's own volume and pitch.
    /// </summary>
    public sealed class BroAudioClipAsset : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("The BroAudio sound (entity) to play. Change the sound itself in the BroAudio library.")]
        [SerializeField] private AudioEntity sound;

        [Tooltip("Volume across the clip, 0 to 1, multiplied by the entity's volume and the player's saved volumes.")]
        [SerializeField] private AnimationCurve volumeCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("Pitch across the clip, multiplied by the entity's pitch. A flat 1 leaves the entity's pitch untouched.")]
        [SerializeField] private AnimationCurve pitchCurve = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("Optional echo on this clip's sound only.")]
        [SerializeField] private BroAudioEchoSettings echo = new BroAudioEchoSettings();

        /// <summary>The clip's echo settings (off unless enabled).</summary>
        public BroAudioEchoSettings Echo => echo;

        /// <summary>The sound asset, readable in edit mode (BroAudio keeps SoundID's entity internal).</summary>
        public AudioEntity SoundEntity => sound;

        public SoundID Sound => new SoundID(sound);

        public ClipCaps clipCaps => ClipCaps.None;

        /// <summary>New clips are as long as the sound's first clip.</summary>
        public override double duration
        {
            get
            {
                AudioClip firstClip = FindFirstAudioClip();
                if (firstClip == null)
                {
                    return base.duration;
                }

                return firstClip.length;
            }
        }

        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner)
        {
            ScriptPlayable<BroAudioClipBehaviour> playable = ScriptPlayable<BroAudioClipBehaviour>.Create(_graph);
            BroAudioClipBehaviour behaviour = playable.GetBehaviour();
            behaviour.Configure(this, volumeCurve, pitchCurve);
            return playable;
        }

        private AudioClip FindFirstAudioClip()
        {
            AudioEntity entity = sound;
            if (entity == null || entity.Clips == null || entity.Clips.Length == 0)
            {
                return null;
            }

            IBroAudioClip firstClip = entity.Clips[0];
            return firstClip.GetAudioClip();
        }
    }
}
