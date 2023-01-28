using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.DisconnectLobby))]
    public class DiscordController_DisconnectLobby_Patch
    {
        [HarmonyPrefix]
        public static void DisconnectLobby()
        {
            Logger.Log("DisconnectLobby");
        }
    }
}
