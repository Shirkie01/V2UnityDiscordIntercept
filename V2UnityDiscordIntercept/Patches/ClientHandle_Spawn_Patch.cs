using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(ClientHandle), nameof(ClientHandle.Spawn))]
    internal class ClientHandle_Spawn_Patch
    {
        [HarmonyPrefix]
        public static void Spawn(Packet _packet, long userId)
        {
            Logger.Log($"Spawn {userId}");
        }
    }
}
