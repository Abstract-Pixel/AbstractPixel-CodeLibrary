using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace AbstractPixel.Core
{
    public static class PlayerLoopUtils
    {
        public static bool TryInsertSystem<T>(ref PlayerLoopSystem loop, PlayerLoopSystem newSystemToInsert, int index)
        {
            if (loop.type != typeof(T))
            {
                return FindSystemInSubSystemLoop<T>(ref loop, newSystemToInsert, index);
            }

            List<PlayerLoopSystem> playerLoopCopyList = new List<PlayerLoopSystem>();
            if (loop.subSystemList != null && loop.subSystemList.Length > 0)
            {
                playerLoopCopyList.AddRange(loop.subSystemList);
            }
            playerLoopCopyList.Insert(index, newSystemToInsert);
            loop.subSystemList = playerLoopCopyList.ToArray();
            return true;
        }

        static bool FindSystemInSubSystemLoop<T>(ref PlayerLoopSystem parentLoop, PlayerLoopSystem systemToInsert, int index)
        {
            if (parentLoop.subSystemList == null || parentLoop.subSystemList.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < parentLoop.subSystemList.Length; i++)
            {

                if (!TryInsertSystem<T>(ref parentLoop.subSystemList[i], systemToInsert, index))
                {
                    continue;
                }
                else
                {
                    return true;
                }

            }
            return false;


        }

        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null) return;

            List<PlayerLoopSystem> playerLoopCopyList = new List<PlayerLoopSystem>(loop.subSystemList);
            for (int i = 0; i < playerLoopCopyList.Count; i++)
            {
                if (playerLoopCopyList[i].type == systemToRemove.type && playerLoopCopyList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    playerLoopCopyList.RemoveAt(i);
                    loop.subSystemList = playerLoopCopyList.ToArray();
                }
            }
            HandleSystemRemovalInSubSystemLoop<T>(ref loop, systemToRemove);
        }

        static void HandleSystemRemovalInSubSystemLoop<T>(ref PlayerLoopSystem parentLoop, in PlayerLoopSystem systemToRemove)
        {
            if (parentLoop.subSystemList == null || parentLoop.subSystemList.Length == 0)
            {
                return;
            }
            for (int i = 0; i < parentLoop.subSystemList.Length; i++)
            {
                RemoveSystem<T>(ref parentLoop.subSystemList[i], systemToRemove);
            }
        }

        public static void PrintPlayerLoop(PlayerLoopSystem loop)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Unity Player Loop:");
            foreach (PlayerLoopSystem system in loop.subSystemList)
            {
                PrintSubsystem(system, sb, 1);
            }
            Debug.Log(sb.ToString());
        }

        static void PrintSubsystem(PlayerLoopSystem system, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2).AppendLine(system.type.Name);
            if (system.subSystemList == null || system.subSystemList.Length == 0)
            {
                return;
            }
            foreach (PlayerLoopSystem subSystem in system.subSystemList)
            {
                PrintSubsystem(subSystem, sb, level + 1);
            }
        }
    }
}
