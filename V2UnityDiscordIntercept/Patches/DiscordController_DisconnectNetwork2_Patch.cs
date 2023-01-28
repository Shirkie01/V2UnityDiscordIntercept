using HarmonyLib;
using Lidgren.Network;
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
            Logger.Log("Disconnecting network");

            Demo.instance.LeaveLobby(0L);

            Plugin.Client.Disconnect(Plugin.Username + " has disconnected.");

            if (DiscordController.IsOwner())
                Plugin.Server.DeleteLobby();

            return false;
        }
    }
}
