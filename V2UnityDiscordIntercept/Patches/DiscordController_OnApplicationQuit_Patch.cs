using HarmonyLib;
using System;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(OnApplicationQuit))]
    internal class DiscordController_OnApplicationQuit_Patch
    {
        [HarmonyPrefix]
        public static bool OnApplicationQuit()
        {
            Console.WriteLine("DiscordController.OnApplicationQuit");
            return false;
        }
    }
}