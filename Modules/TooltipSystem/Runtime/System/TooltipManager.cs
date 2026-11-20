using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AbstractPixel.Tooltip
{
    public static class TooltipManager
    {
        private static Dictionary<TooltipConfig, TooltipView> spawnedTooltipViewsDict = new Dictionary<TooltipConfig, TooltipView>();

        private static Transform screenSpaceCanvas;
        private static Transform worldSpaceCanvas;
        private static TooltipFactory tooltipFactory = new TooltipFactory();

        public static void ShowTooltip(TooltipData _tooltipData)
        {
            TooltipConfig config = _tooltipData.Config;

            if (spawnedTooltipViewsDict.TryGetValue(config, out TooltipView _tooltipView))
            {
                _tooltipView.Initialize(_tooltipData);
                _tooltipView.Show();
            }
            else
            {
                Transform parentTransform = config.isWorldSpace ? worldSpaceCanvas : screenSpaceCanvas;
                TooltipView newTooltipView = tooltipFactory.Create(config.TooltipPrefab, _tooltipData, _parentTransform: parentTransform);
                newTooltipView.Show();
                spawnedTooltipViewsDict[_tooltipData.Config] = newTooltipView;
            }
        }

        public static void HideTooltip(TooltipConfig _tooltipConfig)
        {
            if (spawnedTooltipViewsDict.TryGetValue(_tooltipConfig, out TooltipView viewShell))
            {
                if (viewShell != null)
                {
                    viewShell.Hide();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetData()
        {
            spawnedTooltipViewsDict.Clear();
            AutoGenerateCanvasIfNull();
            tooltipFactory = new TooltipFactory();
            SceneManager.sceneLoaded -= ResetDataOnSceneLoad;
            SceneManager.sceneLoaded += ResetDataOnSceneLoad;
        }

        private static void ResetDataOnSceneLoad(Scene _scene, LoadSceneMode _mode)
        {
            if (spawnedTooltipViewsDict.Count > 0)
            {
                foreach (TooltipView tooltipView in spawnedTooltipViewsDict.Values)
                {
                    if (tooltipView != null)
                    {
                        Object.Destroy(tooltipView.gameObject);
                    }
                }
            }
            spawnedTooltipViewsDict.Clear();
            tooltipFactory = new TooltipFactory();

            AutoGenerateCanvasIfNull();
        }

        private static void AutoGenerateCanvasIfNull()
        {
            if (screenSpaceCanvas == null)
            {
                GameObject newCanvas = new GameObject("[Tooltip_Canvas]");
                Object.DontDestroyOnLoad(newCanvas);

                Canvas canvas = newCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 30000;

                CanvasScaler scaler = newCanvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                screenSpaceCanvas = newCanvas.transform;
            }

            if (worldSpaceCanvas == null)
            {
                GameObject newWorldCanvas = new GameObject("[Tooltip_World_Canvas]");
                Object.DontDestroyOnLoad(newWorldCanvas);

                Canvas canvas = newWorldCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 30000;

                // Architectural Fix: Removed the CanvasScaler entirely (it does nothing in World Space).
                // Shrunk the canvas scale drastically so UI isn't thousands of meters wide in the 3D scene.
                newWorldCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                worldSpaceCanvas = newWorldCanvas.transform;
            }
        }
    }
}