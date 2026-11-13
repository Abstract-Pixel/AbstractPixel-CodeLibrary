using System;
using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Owns every static subscription the rumble system needs (active device, plug/unplug, focus, quit)
    /// and re-raises them as simple events for RumbleService. Subscribes at most once.
    /// </summary>
    public class RumbleDeviceWatcher
    {
        /// <summary>Debug: how many watchers hold static subscriptions. Should be 0 or 1.</summary>
        public static int ActiveSubscriptionCount { get; private set; }

        public event Action<InputDevice> ActiveDeviceChanged;
        public event Action<InputDevice> DeviceRemoved;
        public event Action DeviceListChanged;
        public event Action<bool> FocusChanged;
        public event Action Quitting;

        private bool isSubscribed;

        public void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            isSubscribed = true;
            ActiveSubscriptionCount++;
            InputDeviceTracker.OnCurrentInputDeviceChanged += HandleActiveDeviceChanged;
            InputSystem.onDeviceChange += HandleDeviceChange;
            Application.focusChanged += HandleFocusChanged;
            Application.quitting += HandleQuitting;
        }

        public void Unsubscribe()
        {
            if (isSubscribed == false)
            {
                return;
            }

            isSubscribed = false;
            ActiveSubscriptionCount--;
            InputDeviceTracker.OnCurrentInputDeviceChanged -= HandleActiveDeviceChanged;
            InputSystem.onDeviceChange -= HandleDeviceChange;
            Application.focusChanged -= HandleFocusChanged;
            Application.quitting -= HandleQuitting;
        }

        private void HandleActiveDeviceChanged(InputDevice _device)
        {
            ActiveDeviceChanged?.Invoke(_device);
        }

        private void HandleDeviceChange(InputDevice _device, InputDeviceChange _change)
        {
            bool isRemoval = _change == InputDeviceChange.Removed || _change == InputDeviceChange.Disconnected;
            if (isRemoval)
            {
                DeviceRemoved?.Invoke(_device);
                return;
            }

            bool isAddition = _change == InputDeviceChange.Added || _change == InputDeviceChange.Reconnected;
            if (isAddition)
            {
                DeviceListChanged?.Invoke();
            }
        }

        private void HandleFocusChanged(bool _hasFocus)
        {
            FocusChanged?.Invoke(_hasFocus);
        }

        private void HandleQuitting()
        {
            Quitting?.Invoke();
        }
    }
}
