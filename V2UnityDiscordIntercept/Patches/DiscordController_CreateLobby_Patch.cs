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
            Plugin.Username = lobbyName;
            var server = new VigServer(Plugin.Port);
            server.CreateLobby();
            Plugin.Server = server;


            Plugin.Client = new VigClient();
            Plugin.Client.ConnectToLobby("localhost", Plugin.Port);
            DiscordController.instance.SetLobbyOwner(Plugin.Client.Identifier);            
            return false;
        }
    }
}
