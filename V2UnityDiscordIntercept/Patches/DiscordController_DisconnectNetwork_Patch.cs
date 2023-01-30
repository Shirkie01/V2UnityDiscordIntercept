using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.DisconnectNetwork))]
    public class DiscordController_DisconnectNetwork_Patch
    {
        [HarmonyPrefix]
        public static void DisconnectNetwork()
        {
            Logger.Log("DisconnectNetwork");
        }
    }
}
