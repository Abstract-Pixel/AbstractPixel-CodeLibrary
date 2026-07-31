using Ami.BroAudio;
using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class AmbienceVolumeSetting : AudioVolumeSetting 
    {
        public AmbienceVolumeSetting()
        {
            targetBroAudioType = BroAudioType.Ambience;
        }
    }
}