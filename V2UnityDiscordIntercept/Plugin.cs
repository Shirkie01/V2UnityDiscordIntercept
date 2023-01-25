using BepInEx;
using HarmonyLib;
using UnityEngine;
using V2UnityDiscordIntercept.Patches;

namespace V2UnityDiscordIntercept
{
    [BepInPlugin("c1b6540e-a6ed-4f10-89b3-8e715ee70a78", "V2Unity Discord Intercept", "0.0.5-alpha")]
    public class Plugin : BaseUnityPlugin
    {
        public const string AppIdentifier = "V2Unity-Discord-Patch";
        private Rect windowRect = new Rect(100, 100, 300, 180);
        private string ipAddress = "localhost";
        public static int Port { get; set; } = 14697;
        public static string Username = "Player";
        public static VigServer Server { get; set; }
        public static VigClient Client { get; set; }

        public static bool ShowConnectionWindow { get; set; }

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
            Username = GUILayout.TextField(Username);

            GUILayout.Label("IP Address");
            ipAddress = GUILayout.TextField(ipAddress);

            GUILayout.Label("Port");
            if (int.TryParse(GUILayout.TextField(Port.ToString()), out int newPort))
            {
                Port = newPort;
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Close"))
                {
                    ShowConnectionWindow = false;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Connect"))
                {
                    Client = new VigClient();
                    Client.ConnectToLobby(ipAddress, Port);
                    ShowConnectionWindow = false;
                }
            }

            GUI.DragWindow();
        }

        public void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }

            if (Server != null)
            {
                Server.Update();
            }
        }

        public void LateUpdate()
        {
            if (Client != null)
            {
                Client.LateUpdate();
            }

            if (Server != null)
            {
                Server.LateUpdate();
            }
        }
    }
}