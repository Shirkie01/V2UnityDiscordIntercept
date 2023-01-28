using Discord;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(Demo), nameof(Demo.LeaveLobby))]
    internal class Demo_LeaveLobby_Patch
    {
        [HarmonyPrefix]
        public static bool LeaveLobby(Demo __instance, long lobbyId)
        {
            // Don't do any network stuff in here, this is just
            // for UI clean up.

            Logger.Log("Leaving lobby");

            foreach (var player in __instance.playerText.Keys)
            {
                if (GameManager.instance.inDebug)
                {
                    GameObject.Destroy(__instance.playerText[player].gameObject);
                }
            }

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
