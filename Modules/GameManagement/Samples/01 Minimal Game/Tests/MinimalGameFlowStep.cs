using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.GameManagement.Samples.Tests
{
    /// <summary>What the player sees after one step of the sample's flow test.</summary>
    [Serializable]
    public class MinimalGameFlowStep
    {
        private static readonly string[] MenuNames = { "Main Menu", "Settings", "HUD", "Pause", "Game Over", "Game Win" };

        public string Step;
        public string TopState;
        public float TimeScale;
        public string CursorLock;
        public bool CursorVisible;
        public List<string> MenusShown = new List<string>();

        public static MinimalGameFlowStep Read(string _stepName)
        {
            MinimalGameFlowStep step = new MinimalGameFlowStep();
            step.Step = _stepName;
            step.TopState = GameStates.Top != null ? GameStates.Top.name : "none";
            step.TimeScale = (float)Math.Round(Time.timeScale, 2);
            step.CursorLock = Cursor.lockState.ToString();
            step.CursorVisible = Cursor.visible;
            foreach (string menuName in MenuNames)
            {
                GameObject menu = GameObject.Find(menuName);
                if (menu != null && menu.activeInHierarchy)
                {
                    step.MenusShown.Add(menuName);
                }
            }
            return step;
        }

        /// <summary>A line naming what differs from the expected step, or null when they match.</summary>
        public string DescribeDifference(MinimalGameFlowStep _expected)
        {
            bool isSame = Step == _expected.Step && TopState == _expected.TopState
                && Mathf.Abs(TimeScale - _expected.TimeScale) < 0.02f
                && CursorLock == _expected.CursorLock && CursorVisible == _expected.CursorVisible
                && MenusShown.SequenceEqual(_expected.MenusShown);
            if (isSame)
            {
                return null;
            }
            return $"'{Step}': got {JsonUtility.ToJson(this)}, expected {JsonUtility.ToJson(_expected)}";
        }
    }
}
