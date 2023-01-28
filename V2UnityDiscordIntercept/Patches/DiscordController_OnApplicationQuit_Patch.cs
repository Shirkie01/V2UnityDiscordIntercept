using HarmonyLib;
using System;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(OnApplicationQuit))]
    internal class DiscordController_OnApplicationQuit_Patch
    {
        [HarmonyPrefix]
        public static bool OnApplicationQuit(DiscordController __instance)
        {
            Logger.Log("DiscordController.OnApplicationQuit");
            __instance.DisconnectNetwork2();
            return false;
        }
    }
}