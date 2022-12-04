﻿using HarmonyLib;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(ClientSend), nameof(ClientSend.Joined))]
    internal class ClientSend_Joined_Patch
    {
        [HarmonyPrefix]
        public static bool Joined()
        {
            using (Packet packet = new Packet((int)ClientPackets.joined))
            {
                packet.Write(Plugin.Network.Username);
                Plugin.Network.SendTCPData(packet, 0L);
            }
            return false;
        }
    }
}