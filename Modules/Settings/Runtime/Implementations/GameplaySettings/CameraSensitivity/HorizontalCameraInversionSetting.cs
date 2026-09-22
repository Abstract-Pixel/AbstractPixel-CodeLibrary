using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class HorizontalCameraInversionSetting : BaseCameraInversionSetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.HorizontalX; 
    }
}