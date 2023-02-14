using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.GetUserId))]
    internal class DiscordController_GetUserId_Patch
    {
        [HarmonyPrefix]
        public static bool GetUserId(ref long __result)
        {
            __result = Plugin.Client.Identifier;
            return false;
        }
    }
}
