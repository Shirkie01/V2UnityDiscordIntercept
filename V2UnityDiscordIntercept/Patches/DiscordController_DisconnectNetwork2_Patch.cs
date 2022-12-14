using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.DisconnectNetwork2))]
    internal class DiscordController_DisconnectNetwork2_Patch
    {
        [HarmonyPrefix]
        public static bool DisconnectNetwork2()
        {
            Demo.instance.LeaveLobby(0L);

            if (Plugin.Network.IsServer)
                Plugin.Network.DeleteLobby();

            return false;
        }
    }
}
