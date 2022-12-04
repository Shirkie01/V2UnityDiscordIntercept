using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SendNetworkMessageToUser))]
    internal class DiscordController_SendNetworkMessageToUser_Patch
    {
        [HarmonyPrefix]
        public static bool SendNetworkMessageToUser(long userId, byte channelId, byte[] data)
        {
            Plugin.Network.SendNetworkMessageToUser(userId, channelId, data);
            return false;
        }
    }
}
