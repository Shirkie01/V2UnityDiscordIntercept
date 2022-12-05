using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.GetMemberId))]
    internal class DiscordController_GetMemberId_Patch
    {
        [HarmonyPrefix]
        public static bool GetMemberId(ref int __result)
        {
            __result = Plugin.Network.GetMemberId();
            return false;
        }
    }
}
