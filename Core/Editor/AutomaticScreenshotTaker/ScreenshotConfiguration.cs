namespace ScreenshotTool.Editor
{
    public readonly struct ScreenshotConfiguration
    {
        public string SaveDirectoryPath { get; }
        public float IntervalInSeconds { get; }
        public ScreenshotAspectRatio AspectRatio { get; }
        public ScreenshotResolutionTier ResolutionTier { get; }
        public ScreenshotFileFormat FileFormat { get; }
        public int CustomWidth { get; }
        public int CustomHeight { get; }

        public ScreenshotConfiguration(
            string _saveDirectoryPath,
            float _intervalInSeconds,
            ScreenshotAspectRatio _aspectRatio,
            ScreenshotResolutionTier _resolutionTier,
            ScreenshotFileFormat _fileFormat,
            int _customWidth,
            int _customHeight)
        {
            SaveDirectoryPath = _saveDirectoryPath;
            IntervalInSeconds = _intervalInSeconds;
            AspectRatio = _aspectRatio;
            ResolutionTier = _resolutionTier;
            FileFormat = _fileFormat;
            CustomWidth = _customWidth;
            CustomHeight = _customHeight;
        }
    }
}