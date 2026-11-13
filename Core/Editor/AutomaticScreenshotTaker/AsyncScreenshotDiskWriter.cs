using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ScreenshotTool.Editor
{
    public static class AsyncScreenshotDiskWriter
    {
        private const int BYTES_PER_PIXEL_RGBA32 = 4;
        private const int FILE_STREAM_BUFFER_SIZE = 65536;
        private const int MAXIMUM_JPEG_QUALITY = 98;

        public static async Task<string> SaveImageAsync(
            NativeArray<byte> _pixelBuffer,
            int _width,
            int _height,
            ScreenshotConfiguration _configuration,
            CancellationToken _cancellationToken)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            ProcessAndFlipBuffer(_pixelBuffer, _width, _height);

            NativeArray<byte> encodedData = _configuration.FileFormat == ScreenshotFileFormat.PNG
                ? ImageConversion.EncodeNativeArrayToPNG(_pixelBuffer, GraphicsFormat.R8G8B8A8_SRGB, (uint)_width, (uint)_height, 0)
                : ImageConversion.EncodeNativeArrayToJPG(_pixelBuffer, GraphicsFormat.R8G8B8A8_SRGB, (uint)_width, (uint)_height, 0, MAXIMUM_JPEG_QUALITY);

            string destinationPath = string.Empty;

            try
            {
                if (!Directory.Exists(_configuration.SaveDirectoryPath))
                {
                    Directory.CreateDirectory(_configuration.SaveDirectoryPath);
                }

                string fileExtension = _configuration.FileFormat == ScreenshotFileFormat.PNG ? "png" : "jpg";
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string fileName = $"Shot_{timestamp}_{_width}x{_height}.{fileExtension}";
                destinationPath = Path.Combine(_configuration.SaveDirectoryPath, fileName);

                byte[] managedOutputArray = encodedData.ToArray();

                using FileStream outputStream = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    FILE_STREAM_BUFFER_SIZE,
                    useAsync: true);

                await outputStream.WriteAsync(managedOutputArray, 0, managedOutputArray.Length, _cancellationToken);
            }
            finally
            {
                if (encodedData.IsCreated)
                {
                    encodedData.Dispose();
                }
            }

            return destinationPath;
        }

        private static void ProcessAndFlipBuffer(NativeArray<byte> _buffer, int _width, int _height)
        {
            int rowByteCount = _width * BYTES_PER_PIXEL_RGBA32;
            byte[] rentedRowBuffer = ArrayPool<byte>.Shared.Rent(rowByteCount);

            try
            {
                int halfHeight = _height / 2;
                for (int rowIndex = 0; rowIndex < halfHeight; rowIndex++)
                {
                    int topRowOffset = rowIndex * rowByteCount;
                    int bottomRowOffset = (_height - 1 - rowIndex) * rowByteCount;

                    NativeArray<byte>.Copy(_buffer, topRowOffset, rentedRowBuffer, 0, rowByteCount);
                    NativeArray<byte>.Copy(_buffer, bottomRowOffset, _buffer, topRowOffset, rowByteCount);
                    NativeArray<byte>.Copy(rentedRowBuffer, 0, _buffer, bottomRowOffset, rowByteCount);
                }

                int totalByteCount = _buffer.Length;
                for (int byteIndex = 3; byteIndex < totalByteCount; byteIndex += BYTES_PER_PIXEL_RGBA32)
                {
                    _buffer[byteIndex] = 255;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedRowBuffer);
            }
        }
    }
}