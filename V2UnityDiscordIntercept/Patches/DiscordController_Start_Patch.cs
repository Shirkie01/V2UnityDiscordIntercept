using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(Start))]
    internal class DiscordController_Start_Patch
    {
        [HarmonyPrefix]
        public static bool Start()
        {
            return false;
        }
    }
}
