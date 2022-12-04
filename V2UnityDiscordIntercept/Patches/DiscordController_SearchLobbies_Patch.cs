using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SearchLobbies))]
    internal class DiscordController_SearchLobbies_Patch
    {
        [HarmonyPrefix]
        public static bool SearchLobbies()
        {
            Console.WriteLine("DiscordController.SearchLobbies");
            // This prevents the original SearchLobbies code from running.
            return false;
        }
    }
}
