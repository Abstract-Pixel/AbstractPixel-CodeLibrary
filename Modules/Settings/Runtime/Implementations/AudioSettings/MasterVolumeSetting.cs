using Ami.BroAudio;
using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class MasterVolumeSetting : AudioVolumeSetting 
    {
        public MasterVolumeSetting()
        {
            targetBroAudioType = BroAudioType.All;
        }
    }
}