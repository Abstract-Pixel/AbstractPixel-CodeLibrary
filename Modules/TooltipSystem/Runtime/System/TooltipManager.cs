using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.Tooltip
{
    public static class TooltipManager
    {
        // Maps a Config to its SINGLE instantiated UI Shell.
        private static Dictionary<TooltipConfig, TooltipView> spawnedTooltipViewsDict = new Dictionary<TooltipConfig, TooltipView>();

        // Optional: A clean parent to keep your hierarchy organized
        private static Transform uiContainer;
        private static TooltipFactory tooltipFactory = new TooltipFactory();

        public static void ShowTooltip(TooltipData _tooltipData)
        {
            TooltipConfig config = _tooltipData.Config;

            if(spawnedTooltipViewsDict.TryGetValue(config, out TooltipView _tooltipView))
            {
                _tooltipView.Show();
            }
            else
            {
                TooltipView newTooltipView = tooltipFactory.Create(_tooltipData, _parentTransform: uiContainer);
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
            if (uiContainer == null)
            {
                GameObject containerGo = new GameObject("[Tooltip_Container]");
                Object.DontDestroyOnLoad(containerGo);
                uiContainer = containerGo.transform;
            }
            tooltipFactory = new TooltipFactory();

            SceneManager.sceneLoaded -= ResetDataOnSceneLoad;
            SceneManager.sceneLoaded += ResetDataOnSceneLoad;
        }

        private static void ResetDataOnSceneLoad(Scene _scene, LoadSceneMode _mode)
        {
            if(spawnedTooltipViewsDict.Count > 0)
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

            if (uiContainer == null)
            {
                GameObject containerGo = new GameObject("[Tooltip_Container]");
                Object.DontDestroyOnLoad(containerGo);
                uiContainer = containerGo.transform;
            }
        }
       
    }
}