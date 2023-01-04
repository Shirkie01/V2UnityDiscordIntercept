using Discord;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(Demo), nameof(Demo.LeaveLobby))]
    internal class Demo_LeaveLobby_Patch
    {
        [HarmonyPrefix]
        public static bool LeaveLobby(Demo __instance, long lobbyId)
        {            
            __instance.playerText.Clear();
            __instance.playerNames.Clear();
            __instance.playerReady.Clear();
            __instance.playerVehicles.Clear();
            GameManager.instance.networkMembers.Clear();
            GameManager.instance.networkIds.Clear();
            return false;
        }
    }
}
