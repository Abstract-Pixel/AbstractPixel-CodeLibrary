using UnityEngine;
using System.Threading.Tasks;

namespace AbstractPixel.Core
{
    public static class TaskExtensions
    {
        public static Task AsTask(this AsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<bool>();
            if(asyncOperation.isDone)
            {
                tcs.SetResult(true);
            }
            else
            {
                asyncOperation.completed += _ => tcs.SetResult(true);
            }      
            return tcs.Task;
        }
    }
}
