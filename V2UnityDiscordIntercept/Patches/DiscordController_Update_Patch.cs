using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(Update))]
    internal class DiscordController_Update_Patch
    {
        [HarmonyPrefix]
        public static bool Update()
        {            
            return false;
        }
    }
}
