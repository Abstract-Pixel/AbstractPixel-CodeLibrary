using UnityEngine;
using System.Threading.Tasks;

namespace AbstractPixel.Core
{
    public static class TaskExtensions
    {
        public static Task AsTask(this AsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (asyncOperation.isDone)
            {
                tcs.SetResult(true);
            }
            else
            {
                asyncOperation.completed += _ => tcs.SetResult(true);
            }
            return tcs.Task;
        }

        public static async void ForgetTask(this Task task)
        {
            try
            {
                await task;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
