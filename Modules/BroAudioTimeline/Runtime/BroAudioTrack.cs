using UnityEngine;
using UnityEngine.Timeline;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// A Timeline track that plays BroAudio sounds by SoundID, so a sequence's sounds are swapped in the BroAudio
    /// library, not in the Timeline. Bind a Transform to make the track's sounds follow it in 3D (the entity's own
    /// spatial settings decide how 3D it sounds); leave it empty for plain 2D sound.
    /// </summary>
    [TrackColor(0.85f, 0.45f, 0.15f)]
    [TrackClipType(typeof(BroAudioClipAsset))]
    [TrackBindingType(typeof(Transform))]
    public sealed class BroAudioTrack : TrackAsset
    {
    }
}
