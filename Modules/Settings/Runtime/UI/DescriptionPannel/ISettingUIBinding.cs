namespace AbstractPixel.Settings
{
    /// <summary>
    /// Assembly-internal contract that allows non-generic trigger components 
    /// to access bound setting metadata without exposing API outside the settings assembly.
    /// </summary>
    internal interface ISettingUIBinding
    {
        ISettingBackend BoundSetting { get; }
    }
}