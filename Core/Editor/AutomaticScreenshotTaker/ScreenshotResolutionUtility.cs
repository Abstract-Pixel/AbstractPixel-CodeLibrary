using UnityEngine;

namespace ScreenshotTool.Editor
{
    public static class ScreenshotResolutionUtility
    {
        private const int MINIMUM_DIMENSION_LIMIT = 128;
        private const int MAXIMUM_DIMENSION_LIMIT = 7680;

        public static (int width, int height) CalculateDimensions(ScreenshotConfiguration _configuration)
        {
            if (_configuration.ResolutionTier == ScreenshotResolutionTier.Custom)
            {
                int clampedWidth = Mathf.Clamp(_configuration.CustomWidth, MINIMUM_DIMENSION_LIMIT, MAXIMUM_DIMENSION_LIMIT);
                int clampedHeight = Mathf.Clamp(_configuration.CustomHeight, MINIMUM_DIMENSION_LIMIT, MAXIMUM_DIMENSION_LIMIT);
                return (clampedWidth, clampedHeight);
            }

            return _configuration.AspectRatio switch
            {
                ScreenshotAspectRatio.Landscape16x9 => _configuration.ResolutionTier switch
                {
                    ScreenshotResolutionTier.StandardHD720p => (1280, 720),
                    ScreenshotResolutionTier.FullHD1080p => (1920, 1080),
                    ScreenshotResolutionTier.QuadHD1440p => (2560, 1440),
                    ScreenshotResolutionTier.UltraHD4K => (3840, 2160),
                    _ => (1920, 1080)
                },
                ScreenshotAspectRatio.Portrait9x16 => _configuration.ResolutionTier switch
                {
                    ScreenshotResolutionTier.StandardHD720p => (720, 1280),
                    ScreenshotResolutionTier.FullHD1080p => (1080, 1920),
                    ScreenshotResolutionTier.QuadHD1440p => (1440, 2560),
                    ScreenshotResolutionTier.UltraHD4K => (2160, 3840),
                    _ => (1080, 1920)
                },
                ScreenshotAspectRatio.Handheld16x10 => _configuration.ResolutionTier switch
                {
                    ScreenshotResolutionTier.StandardHD720p => (1280, 800),
                    ScreenshotResolutionTier.FullHD1080p => (1920, 1200),
                    ScreenshotResolutionTier.QuadHD1440p => (2560, 1600),
                    ScreenshotResolutionTier.UltraHD4K => (3840, 2400),
                    _ => (1920, 1200)
                },
                ScreenshotAspectRatio.Ultrawide21x9 => _configuration.ResolutionTier switch
                {
                    ScreenshotResolutionTier.StandardHD720p => (1680, 720),
                    ScreenshotResolutionTier.FullHD1080p => (2560, 1080),
                    ScreenshotResolutionTier.QuadHD1440p => (3440, 1440),
                    ScreenshotResolutionTier.UltraHD4K => (5120, 2160),
                    _ => (2560, 1080)
                },
                ScreenshotAspectRatio.Square1x1 => _configuration.ResolutionTier switch
                {
                    ScreenshotResolutionTier.StandardHD720p => (720, 720),
                    ScreenshotResolutionTier.FullHD1080p => (1080, 1080),
                    ScreenshotResolutionTier.QuadHD1440p => (1440, 1440),
                    ScreenshotResolutionTier.UltraHD4K => (2160, 2160),
                    _ => (1080, 1080)
                },
                _ => (1920, 1080)
            };
        }
    }
}