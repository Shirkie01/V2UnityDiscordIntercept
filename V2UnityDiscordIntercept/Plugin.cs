using BepInEx;
using HarmonyLib;
using UnityEngine;
using V2UnityDiscordIntercept.Patches;

namespace V2UnityDiscordIntercept
{
    [BepInPlugin("c1b6540e-a6ed-4f10-89b3-8e715ee70a78", "V2Unity Discord Intercept", "0.0.5-alpha")]
    public class Plugin : BaseUnityPlugin
    {
        private Rect windowRect = new Rect(100, 100, 300, 180);
        private string ipAddress;

        internal static Network Network = new Network();

        private bool ShowConnectionWindow
        {
            get => Demo_GetLobbies_Patch.ShowConnectionWindow;
            set => Demo_GetLobbies_Patch.ShowConnectionWindow = value;
        }

        private void Awake()
        {
            var harmony = new Harmony("c1b6540e-a6ed-4f10-89b3-8e715ee70a78");
            harmony.PatchAll();
        }

        private void OnGUI()
        {
            if (!ShowConnectionWindow)
                return;

            windowRect = GUILayout.Window(1, windowRect, ConnectionWindow, "Network Connection");
        }

        private void ConnectionWindow(int windowId)
        {
            GUILayout.Label("Username");
            Network.Username = GUILayout.TextField(Network.Username);

            GUILayout.Label("IP Address");
            ipAddress = GUILayout.TextField(ipAddress);

            GUILayout.Label("Port");
            if (int.TryParse(GUILayout.TextField(Network.Port.ToString()), out int newPort))
            {
                Network.Port = newPort;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Connect"))
                {
                    Network.ConnectToLobby(ipAddress, Network.Port);
                    ShowConnectionWindow = false;
                }
            }

            GUI.DragWindow();
        }
    }
}