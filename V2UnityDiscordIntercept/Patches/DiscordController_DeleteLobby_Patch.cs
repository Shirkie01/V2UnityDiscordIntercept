using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.DeleteLobby))]
    internal class DiscordController_DeleteLobby_Patch
    {
        [HarmonyPrefix]
        public static bool DeleteLobby()
        {
            Plugin.Network.DeleteLobby();
            return false;
        }
    }
}
