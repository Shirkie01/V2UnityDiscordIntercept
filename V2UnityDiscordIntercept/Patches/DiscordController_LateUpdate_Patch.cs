using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(LateUpdate))]
    internal class DiscordController_LateUpdate_Patch
    {
        [HarmonyPrefix]
        public static bool LateUpdate()
        {            
            return false;
        }
    }
}
