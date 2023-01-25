using HarmonyLib;
using Lidgren.Network;
using Discord;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.CreateLobby))]
    internal class DiscordController_CreateLobby_Patch
    {
        [HarmonyPrefix]
        public static bool CreateLobby(string lobbyName)
        {
            Plugin.Network.Username = lobbyName;
            Logger.Log("Creating lobby");
            Plugin.Network.CreateLobby(lobbyName);
            return false;
        }
    }
}
