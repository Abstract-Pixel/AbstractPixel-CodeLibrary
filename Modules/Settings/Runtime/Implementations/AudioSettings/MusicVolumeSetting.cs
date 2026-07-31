using Ami.BroAudio;
using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class MusicVolumeSetting : AudioVolumeSetting 
    {
        public MusicVolumeSetting()
        {
            targetBroAudioType = BroAudioType.Music;
        }
    }
}