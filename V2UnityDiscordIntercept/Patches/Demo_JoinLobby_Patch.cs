using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(Demo), nameof(Demo.JoinLobby))]
    internal class Demo_JoinLobby_Patch
    {
        [HarmonyPrefix]
        public static bool JoinLobby(Demo __instance, long lobbyId, string secret)
        {
            Logger.Log("Joining lobby");

            if (!DiscordController.IsOwner())
            {
                __instance.componentPanel.gameObject.SetActive(true);
                __instance.controlPanel.gameObject.SetActive(true);
                __instance.modeLabel.gameObject.SetActive(false);
                __instance.stageLabel.gameObject.SetActive(false);
                __instance.damageLabel.gameObject.SetActive(false);
                __instance.difficultyLabel.gameObject.SetActive(false);
                __instance.onlineDmgLabel.gameObject.SetActive(false);
                __instance.livesLabel.gameObject.SetActive(false);
                __instance.readyLabel.gameObject.SetActive(true);
                __instance.notReadyLabel.gameObject.SetActive(false);
                __instance.settingsText.gameObject.SetActive(true);
                __instance.mapText.gameObject.SetActive(true);
                __instance.damageText.gameObject.SetActive(true);
                __instance.livesText.gameObject.SetActive(true);
                __instance.lobbyScrollView.gameObject.SetActive(false);
                __instance.DeleteLobbies();
                __instance.SetupPlaceholders();
            }
            Plugin.Client.JoinLobby(lobbyId, secret);
            return false;
        }
    }
}
