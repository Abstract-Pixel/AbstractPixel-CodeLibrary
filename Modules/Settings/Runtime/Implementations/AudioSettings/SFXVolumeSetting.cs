using Ami.BroAudio;
using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class SFXVolumeSetting : AudioVolumeSetting 
    {
        public SFXVolumeSetting()
        {
            targetBroAudioType = BroAudioType.SFX;
        }
    }
}