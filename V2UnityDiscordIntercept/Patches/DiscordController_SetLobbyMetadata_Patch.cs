using HarmonyLib;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SetLobbyMetadata))]
    internal class DiscordController_SetLobbyMetadata_Patch
    {
        [HarmonyPrefix]
        public static bool SetLobbyMetadata(string key, string value)
        {
            Plugin.Client.SetLobbyMetadata(key, value);
            return false;
        }
    }
}