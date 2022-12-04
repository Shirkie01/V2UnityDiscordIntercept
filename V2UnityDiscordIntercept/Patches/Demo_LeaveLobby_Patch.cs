using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace V2UnityDiscordIntercept.Patches
{
    [HarmonyPatch(typeof(Demo), nameof(Demo.LeaveLobby))]
    internal class Demo_LeaveLobby_Patch
    {
        public static bool LeaveLobby(long lobbyId)
        {
            Console.WriteLine($"Demo.LeaveLobby: {lobbyId}");
            return true;
        }
    }
}
