using HarmonyLib;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(ClientSend), nameof(ClientSend.Welcome))]
    internal class ClientSend_Welcome_Patch
    {
        [HarmonyPrefix]
        public static bool Welcome(long userId)
        {
            using (Packet packet = new Packet((int)ClientPackets.welcome))
            {
                packet.Write(Plugin.Network.Username);
                packet.Write(GameManager.instance.ready);
                packet.Write(GameManager.instance.vehicles[0]);
                Plugin.Network.SendTCPData(packet, userId);
            }
            return false;
        }
    }
}