# BroAudio Timeline

A Timeline track that plays BroAudio sounds by **SoundID**. A sequence's sounds are swapped in the BroAudio library
(change the entity's clips), never in the Timeline.

The module compiles only when both BroAudio (`com.ami.broaudio`) and Timeline 1.8+ are installed.

## Use

1. In a Timeline, add **Abstract Pixel.BroAudioTimeline > BroAudio Track**.
2. Optional: bind a **Transform**. The track's sounds then follow it, and the entity's spatial settings decide how
   3D they sound. Leave it empty for 2D.
3. Add a **BroAudio Clip Asset** and pick its **Sound**. A new clip is as long as the sound's first clip.
4. Shape the clip with:
   - **Volume Curve:** 0–1 across the clip. It multiplies the entity's volume and the player's saved volumes.
   - **Pitch Curve:** multiplies the entity's pitch. A flat 1 keeps the entity's own random pitch.
   - **Echo (optional):** an `AudioEchoFilter` added to this clip's player only (BroAudio's `AddEchoEffect`). It is
     removed when the player is recycled, so no mixer group or other sound is touched.

## Behaviour

- **Play mode:** plays through `BroAudio.Play`, so the audio type's volume (the saved settings), the mixer routing and
  the entity's settings apply as they do anywhere else in the game.
- **Edit mode (Timeline window playing):** plays the entity's clip on a hidden 2D AudioSource, with the entity's volume
  and pitch. Saved settings and mixer routing are not part of the preview.
- **Scrubbing:** silent.
- **Stopping:** the sound stops when the playhead leaves the clip, when the Timeline stops or pauses, and when the
  director is destroyed. It uses the entity's own fade-out.
- **Entering a clip late (after a load hitch):** in Play mode the sound starts from its beginning, because BroAudio has
  no public seek. The edit-mode preview seeks.
