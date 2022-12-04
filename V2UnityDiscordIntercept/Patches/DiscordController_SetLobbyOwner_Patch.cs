﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SetLobbyOwner))]
    internal class DiscordController_SetLobbyOwner_Patch
    {
        [HarmonyPrefix]
        public static bool SetLobbyOwner(DiscordController __instance, long userId)
        {
            var field = __instance.GetType().GetField("lobbyOwner", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(__instance, Plugin.Network.Peer.UniqueIdentifier == userId);
            return false;
        }
    }
}
