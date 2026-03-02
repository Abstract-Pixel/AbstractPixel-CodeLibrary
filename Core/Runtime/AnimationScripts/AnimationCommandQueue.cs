using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace AbstractPixel.Core
{
    [Serializable]
    public class AnimationCommandQueue
    {
        [ReadOnly][SerializeField] private MonoBehaviour coroutineRunner;
        [SerializeField] private Queue<IAnimationCommand> commandQueue = new Queue<IAnimationCommand>();
        [field: SerializeField] public bool IsBusy { get; private set; }
        bool isActive;

        public void Initialize(MonoBehaviour _coroutineRunner)
        {
            coroutineRunner = _coroutineRunner;
            isActive = true;
        }

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;
            if (!isActive)
            {
                IsBusy = false;
            }
            else
            {
                ContinueExecutionOfAnimationQueue();
            }
        }

        void ContinueExecutionOfAnimationQueue()
        {
            if (!isActive) return;
            if (commandQueue.Count > 0)
                coroutineRunner.StartCoroutine(ProcessAnimationQueue());
        }

        public void AddComandToQueue(IAnimationCommand animationCommand)
        {
            if (!isActive) return;
            if (animationCommand == null)
            {
                Debug.LogError("Attempted to add a null animation command to the queue.");
                return;
            }
            commandQueue.Enqueue(animationCommand);
            if (!IsBusy)
            {
                coroutineRunner.StartCoroutine(ProcessAnimationQueue());
            }
        }

        private IEnumerator ProcessAnimationQueue()
        {
            if (!isActive) yield break;
            IsBusy = true;
            // This ensures that the animation started event is raised only once and it knows which command is starting.
            AnimationCommandEventBroadcaster.RaiseOnAnimationStartedEvent(commandQueue.Peek());
            IAnimationCommand AnimationCommand = null;
            while (commandQueue.Count > 0)
            {
                AnimationCommand = commandQueue.First();
                AnimationCommandEventBroadcaster.RaiseOnAnimationInProgressEvent(AnimationCommand);
                yield return coroutineRunner.StartCoroutine(AnimationCommand.ExecuteAnimation());
                commandQueue.Dequeue();
            }
            IsBusy = false;
        }

    }
}