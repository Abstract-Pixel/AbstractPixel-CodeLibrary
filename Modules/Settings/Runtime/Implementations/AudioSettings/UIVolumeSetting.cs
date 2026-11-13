using Ami.BroAudio;
using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class UIVolumeSetting : AudioVolumeSetting 
    {
        public UIVolumeSetting()
        {
            targetBroAudioType = BroAudioType.UI;
        }
    }
}