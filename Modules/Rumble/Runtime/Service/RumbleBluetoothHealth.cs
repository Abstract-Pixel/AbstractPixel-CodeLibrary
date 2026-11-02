using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Watches the bound pad's Bluetooth link for the debug panel and the log:
    /// - drops: the pad was bound over Bluetooth and Windows removed it (link lost). Logged once per drop.
    /// - silence: seconds since the pad last sent any input report (it may just be resting: the panel asks the
    ///   tester to press a button before calling it stuck).
    /// Events only on bind / removal; the per-frame reads allocate nothing.
    /// </summary>
    public class RumbleBluetoothHealth
    {
        private const float DropWindowSeconds = 60f;

        // The last few drop times, for "dropped N times in the last minute". A small fixed ring: no allocation.
        private readonly float[] dropTimes = new float[4];
        private int nextDropSlot;

        private InputDevice boundDevice;
        private RumblePadIdentity boundIdentity = RumblePadIdentity.None;
        private float lastDropTime = float.NegativeInfinity;
        private double boundInputTime;

        public int TotalDrops { get; private set; }
        public string LastDroppedModel { get; private set; } = "-";

        public bool IsBoundOnBluetooth => boundDevice != null && boundIdentity.Connection == RumbleConnection.Bluetooth;

        public float SecondsSinceLastDrop => Time.unscaledTime - lastDropTime;

        /// <summary>Seconds since the bound pad last sent an input report (0 when no pad is bound).</summary>
        public float SecondsWithoutInput
        {
            get
            {
                if (boundDevice == null || boundDevice.added == false)
                {
                    return 0f;
                }

                // Counted from the bind when the pad hasn't sent anything since (lastUpdateTime is 0 before its first report).
                double lastHeard = System.Math.Max(boundDevice.lastUpdateTime, boundInputTime);
                double silence = InputState.currentTime - lastHeard;
                return (float)silence;
            }
        }

        public int DropsInLastMinute
        {
            get
            {
                int count = 0;
                for (int slot = 0; slot < dropTimes.Length; slot++)
                {
                    bool isRecorded = dropTimes[slot] > 0f;
                    bool isRecent = Time.unscaledTime - dropTimes[slot] <= DropWindowSeconds;
                    if (isRecorded && isRecent)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void HandleBound(InputDevice _device, RumblePadIdentity _identity)
        {
            boundDevice = _device;
            boundIdentity = _identity;
            boundInputTime = InputState.currentTime;
        }

        /// <summary>Called when any device is removed. Counts it as a Bluetooth drop only for the bound Bluetooth pad.</summary>
        public void HandleRemoved(InputDevice _device)
        {
            bool isBoundPad = _device != null && _device == boundDevice;
            if (isBoundPad == false || IsBoundOnBluetooth == false)
            {
                return;
            }

            TotalDrops++;
            lastDropTime = Time.unscaledTime;
            LastDroppedModel = boundIdentity.Model;
            dropTimes[nextDropSlot] = Mathf.Max(lastDropTime, 0.0001f);
            nextDropSlot = (nextDropSlot + 1) % dropTimes.Length;
            Debug.LogWarning("[Rumble] Bluetooth: " + LastDroppedModel + " disconnected (" + DropsInLastMinute
                + " drop(s) in the last minute). If its light is still on, Windows lost the link.");
            boundDevice = null;
            boundIdentity = RumblePadIdentity.None;
        }
    }
}
