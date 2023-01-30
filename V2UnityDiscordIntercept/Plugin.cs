using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V2UnityDiscordIntercept
{
    [BepInPlugin("c1b6540e-a6ed-4f10-89b3-8e715ee70a78", "V2Unity Discord Intercept", "0.0.7-alpha")]
    public class Plugin : BaseUnityPlugin
    {
        public const string AppIdentifier = "V2Unity-Discord-Patch";

        public static VigServer Server { get; set; }
        public static VigClient Client { get; set; }

        public static bool ShowConnectionWindow { get; set; }
        private Rect connectionWindowRect = new Rect(100, 100, 300, 180);
        private string ipAddress = "localhost";
        public static int Port { get; set; } = 14697;
        public static string Username = "Player";

        public static bool LogErrors { get; set; } = true;
        public static bool ShowErrorWindow { get; set; }
        private float secondsToShowWindow = -1;
        private Rect errorWindowRect = new Rect(200, 200, 800, 400);
        private Vector2 errorWindowScrollPos = Vector2.zero;

        private readonly IList<Exception> errors = new List<Exception>();

        private void Awake()
        {
            var harmony = new Harmony("c1b6540e-a6ed-4f10-89b3-8e715ee70a78");
            harmony.PatchAll();
        }

        private void OnGUI()
        {
            if (ShowConnectionWindow)
                connectionWindowRect = GUILayout.Window(1, connectionWindowRect, ConnectionWindow, "Network Connection");

            if (ShowErrorWindow)
                errorWindowRect = GUILayout.Window(2, errorWindowRect, ErrorWindow, "Errors");

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
                }
            }

            GUI.DragWindow();
        }

        private void ErrorWindow(int windowId)
        {
            GUILayout.Box("Press F2 to show/hide this window.");

            errorWindowScrollPos = GUILayout.BeginScrollView(errorWindowScrollPos, "box");
            foreach (var error in errors)
            {
                GUILayout.Label(error.ToString(), "box");
            }
            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        public void Update()
        {
            try
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
            catch (Exception e)
            {
                V2UnityDiscordIntercept.Logger.Log(e.ToString());
                errors.Add(e);

#if DEBUG
                secondsToShowWindow = 3;
                ShowErrorWindow = true;
#endif
            }

            if (secondsToShowWindow > 0 && ShowErrorWindow)
            {
                secondsToShowWindow -= Time.deltaTime;
                if (secondsToShowWindow <= 0)
                    ShowErrorWindow = false;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                secondsToShowWindow = -1;
                ShowErrorWindow = !ShowErrorWindow;
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