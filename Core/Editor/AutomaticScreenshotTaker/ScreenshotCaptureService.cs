using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScreenshotTool.Editor
{
    public sealed class ScreenshotCaptureService
    {
        private const int MAXIMUM_CONCURRENT_SAVING_TASKS = 2;

        private ScreenshotConfiguration activeConfiguration;
        private RenderTexture pooledRenderTexture;
        private CancellationTokenSource serviceCancellationTokenSource;
        private SemaphoreSlim backgroundTaskLimiter;
        private double lastTimestamp;
        private float captureIntervalTimer;
        private int previousGameViewSizeIndex;
        private bool isCapturing;
        private bool isGpuReadbackPending;
        private bool shouldCaptureNextFrame;

        public bool IsCapturing => isCapturing;
        public int SavedScreenshotCount { get; private set; }

        public void StartCapture(ScreenshotConfiguration _configuration)
        {
            if (isCapturing)
            {
                StopCapture();
            }

            activeConfiguration = _configuration;
            SavedScreenshotCount = 0;
            captureIntervalTimer = 0f;
            lastTimestamp = EditorApplication.timeSinceStartup;
            isGpuReadbackPending = false;
            shouldCaptureNextFrame = false;

            serviceCancellationTokenSource = new CancellationTokenSource();
            backgroundTaskLimiter = new SemaphoreSlim(MAXIMUM_CONCURRENT_SAVING_TASKS, MAXIMUM_CONCURRENT_SAVING_TASKS);

            (int targetWidth, int targetHeight) = ScreenshotResolutionUtility.CalculateDimensions(activeConfiguration);

            GameViewResolutionBridge.TrySetGameViewResolution(targetWidth, targetHeight, out previousGameViewSizeIndex);

            pooledRenderTexture = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                name = "ScreenshotCapture_PooledRenderTexture",
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point
            };
            pooledRenderTexture.Create();

            isCapturing = true;

            EditorApplication.update += HandleEditorUpdate;
            RenderPipelineManager.endCameraRendering += HandleEndCameraRendering;
        }

        public void StopCapture()
        {
            if (!isCapturing)
            {
                return;
            }

            isCapturing = false;

            EditorApplication.update -= HandleEditorUpdate;
            RenderPipelineManager.endCameraRendering -= HandleEndCameraRendering;

            GameViewResolutionBridge.RestoreGameViewResolution(previousGameViewSizeIndex);
            previousGameViewSizeIndex = -1;

            if (serviceCancellationTokenSource != null)
            {
                serviceCancellationTokenSource.Cancel();
                serviceCancellationTokenSource.Dispose();
                serviceCancellationTokenSource = null;
            }

            if (backgroundTaskLimiter != null)
            {
                backgroundTaskLimiter.Dispose();
                backgroundTaskLimiter = null;
            }

            if (pooledRenderTexture != null)
            {
                if (pooledRenderTexture.IsCreated())
                {
                    pooledRenderTexture.Release();
                }
                UnityEngine.Object.DestroyImmediate(pooledRenderTexture);
                pooledRenderTexture = null;
            }
        }

        private void HandleEditorUpdate()
        {
            if (!isCapturing || !EditorApplication.isPlaying)
            {
                return;
            }

            double currentTimestamp = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTimestamp - lastTimestamp);
            lastTimestamp = currentTimestamp;

            captureIntervalTimer += deltaTime;

            if (captureIntervalTimer >= activeConfiguration.IntervalInSeconds)
            {
                captureIntervalTimer = 0f;

                if (!isGpuReadbackPending)
                {
                    shouldCaptureNextFrame = true;
                }
            }
        }

        private void HandleEndCameraRendering(ScriptableRenderContext _context, Camera _camera)
        {
            if (!isCapturing || !shouldCaptureNextFrame || isGpuReadbackPending)
            {
                return;
            }

            if (_camera == null || _camera.cameraType != CameraType.Game || _camera.targetTexture != null)
            {
                return;
            }

            if (pooledRenderTexture == null || !pooledRenderTexture.IsCreated())
            {
                return;
            }

            int currentScreenWidth = Screen.width;
            int currentScreenHeight = Screen.height;

            if (currentScreenWidth <= 0 || currentScreenHeight <= 0)
            {
                return;
            }

            (int targetWidth, int targetHeight) = ScreenshotResolutionUtility.CalculateDimensions(activeConfiguration);

            shouldCaptureNextFrame = false;
            isGpuReadbackPending = true;

            if (currentScreenWidth == targetWidth && currentScreenHeight == targetHeight)
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(pooledRenderTexture);
            }
            else
            {
                RenderTexture temporaryScreenBuffer = RenderTexture.GetTemporary(
                    currentScreenWidth,
                    currentScreenHeight,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB
                );
                temporaryScreenBuffer.filterMode = FilterMode.Bilinear;

                ScreenCapture.CaptureScreenshotIntoRenderTexture(temporaryScreenBuffer);

                float sourceAspect = (float)currentScreenWidth / currentScreenHeight;
                float targetAspect = (float)targetWidth / targetHeight;

                Vector2 uvScale = Vector2.one;
                Vector2 uvOffset = Vector2.zero;

                if (sourceAspect > targetAspect)
                {
                    uvScale.x = targetAspect / sourceAspect;
                    uvOffset.x = (1.0f - uvScale.x) * 0.5f;
                }
                else if (sourceAspect < targetAspect)
                {
                    uvScale.y = sourceAspect / targetAspect;
                    uvOffset.y = (1.0f - uvScale.y) * 0.5f;
                }

                Graphics.Blit(temporaryScreenBuffer, pooledRenderTexture, uvScale, uvOffset);
                RenderTexture.ReleaseTemporary(temporaryScreenBuffer);
            }

            AsyncGPUReadback.Request(pooledRenderTexture, 0, TextureFormat.RGBA32, OnAsyncGpuReadbackCompleted);
        }

        private void OnAsyncGpuReadbackCompleted(AsyncGPUReadbackRequest _request)
        {
            isGpuReadbackPending = false;

            if (_request.hasError || serviceCancellationTokenSource == null || serviceCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            int imageWidth = _request.width;
            int imageHeight = _request.height;

            NativeArray<byte> readbackData = _request.GetData<byte>();
            NativeArray<byte> persistentBuffer = new NativeArray<byte>(readbackData.Length, Allocator.Persistent);
            persistentBuffer.CopyFrom(readbackData);

            CancellationToken token = serviceCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                await backgroundTaskLimiter.WaitAsync(token);

                try
                {
                    await AsyncScreenshotDiskWriter.SaveImageAsync(
                        persistentBuffer,
                        imageWidth,
                        imageHeight,
                        activeConfiguration,
                        token);

                    SavedScreenshotCount++;
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (persistentBuffer.IsCreated)
                    {
                        persistentBuffer.Dispose();
                    }

                    backgroundTaskLimiter.Release();
                }
            }, token);
        }
    }
}