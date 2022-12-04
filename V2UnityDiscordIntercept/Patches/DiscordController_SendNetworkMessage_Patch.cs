using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SendNetworkMessage))]
    internal class DiscordController_SendNetworkMessage_Patch
    {
        [HarmonyPrefix]
        public static bool SendNetworkMessage(byte channelId, byte[] data)
        {            
            Plugin.Network.SendNetworkMessage(channelId, data);
            return false;
        }
    }
}
