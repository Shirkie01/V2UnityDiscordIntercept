using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(Demo), nameof(Demo.GetLobbies))]
    internal class Demo_GetLobbies_Patch
    {
        [HarmonyPrefix]
        public static void GetLobbies()
        {
            Plugin.ShowConnectionWindow = true;
        }
    }
}
