using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class VerticalCameraInversionSetting : BaseCameraInversionSetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.VerticalY; 
    }
}