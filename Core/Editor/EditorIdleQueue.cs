using System;
using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Runs work once the Editor is idle (not compiling, not importing), from EditorApplication.update. Use it instead
    /// of EditorApplication.delayCall for work an asset postprocessor or a save hook must do after the import:
    /// delayCall can wait until the Editor window is focused again, update keeps ticking.
    /// </summary>
    public static class EditorIdleQueue
    {
        private static readonly List<Action> pending = new List<Action>();
        private static bool isSubscribed;

        public static void Run(Action _work)
        {
            if (_work == null)
            {
                return;
            }
            pending.Add(_work);
            if (isSubscribed)
            {
                return;
            }
            EditorApplication.update += RunPending;
            isSubscribed = true;
        }

        private static void RunPending()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }
            EditorApplication.update -= RunPending;
            isSubscribed = false;
            List<Action> work = new List<Action>(pending);
            pending.Clear();
            foreach (Action action in work)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
        }
    }
}
