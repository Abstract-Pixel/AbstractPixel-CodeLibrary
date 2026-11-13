using System;
using System.IO;
using UnityEngine;

namespace ScreenshotTool.Editor
{
    public sealed class ScreenshotSessionItem
    {
        public string OriginalFilePath { get; }
        public string CurrentFilePath { get; set; }
        public string DesiredFileNameWithoutExtension { get; set; }
        public string FileExtension { get; }
        public bool IsMarkedToKeep { get; set; }
        public Texture2D ThumbnailTexture { get; private set; }

        public ScreenshotSessionItem(string _filePath)
        {
            OriginalFilePath = _filePath;
            CurrentFilePath = _filePath;
            FileExtension = Path.GetExtension(_filePath);
            DesiredFileNameWithoutExtension = Path.GetFileNameWithoutExtension(_filePath);
            IsMarkedToKeep = false;
        }

        public Texture2D GetOrCreateThumbnail()
        {
            if (ThumbnailTexture != null)
            {
                return ThumbnailTexture;
            }

            if (!File.Exists(CurrentFilePath))
            {
                return null;
            }

            try
            {
                byte[] rawFileBytes = File.ReadAllBytes(CurrentFilePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                if (ImageConversion.LoadImage(texture, rawFileBytes))
                {
                    ThumbnailTexture = texture;
                    return ThumbnailTexture;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DisposeThumbnail()
        {
            if (ThumbnailTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(ThumbnailTexture);
                ThumbnailTexture = null;
            }
        }
    }
}