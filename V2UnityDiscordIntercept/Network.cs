using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace V2UnityDiscordIntercept
{
    internal class Network
    {
        private const string appIdentifier = "V2Unity-Patch";
        public string Username { get; set; } = "Player";
        public int Port { get; set; } = 14697;

        private NetServer server;
        private NetClient client;

        private bool IsServer => server != null;
        public NetPeer Peer => server ?? client as NetPeer;

        private string _lobbyName;
        private IDictionary<string, string> lobbyMetadata = new Dictionary<string, string>();

        private delegate void PacketHandler(Packet _packet, long userId);

        private IDictionary<int, PacketHandler> packetHandlers = new Dictionary<int, PacketHandler>();

        public void CreateLobby(string lobbyName)
        {
            if (server == null)
            {
                _lobbyName = lobbyName;
                Username = lobbyName;
                var config = new NetPeerConfiguration(appIdentifier)
                {
                    Port = Port
                };
                config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
                config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
                config.EnableMessageType(NetIncomingMessageType.StatusChanged);

                server = new NetServer(config);
                server.Start();
                Console.WriteLine($"Started server with name {_lobbyName}.");

                DiscordController.instance.SetLobbyOwner(Peer.UniqueIdentifier);

                InitializePacketHandlers();
            }
        }

        public void ConnectToLobby(string ipAddress, int port)
        {
            Port = port;
            var config = new NetPeerConfiguration(appIdentifier);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            client = new NetClient(config);
            client.Start();
            var netConnection = client.Connect(ipAddress, port);
        }

        public void JoinLobby(long lobbyId, string secret)
        {
            Console.WriteLine($"Network.JoinLobby({lobbyId}, {secret})");
            InitializePacketHandlers();
            ClientSend.Joined();
        }

        public void Update()
        {
            if (Peer == null)
                return;

            ReadMessages();
        }

        public void LateUpdate()
        {
            if (Peer == null)
                return;

            Peer.FlushSendQueue();
        }

        public void ReadMessages()
        {
            NetIncomingMessage msg;
            while ((msg = Peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        CreateDiscoveryResponse(msg);
                        break;

                    case NetIncomingMessageType.DiscoveryResponse:
                        Console.WriteLine($"Found server at {msg.SenderEndPoint} with name {msg.ReadString()}");
                        break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        StatusChanged(msg);
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        ConnectionApproval(msg);
                        break;

                    case NetIncomingMessageType.Data:
                        Data(msg);
                        break;

                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType + "\n" + msg.ToString());
                        break;
                }
                Peer.Recycle(msg);
            }
        }

        private void CreateDiscoveryResponse(NetIncomingMessage msg)
        {
            // Create a response and write some example data to it
            NetOutgoingMessage response = server.CreateMessage();
            response.Write(_lobbyName);

            // Send the response to the sender of the request
            server.SendDiscoveryResponse(response, msg.SenderEndPoint);
        }

        public void DeleteLobby()
        {
            server.Shutdown("The server is shutting down.");
        }

        public void SendNetworkMessage(byte channelId, byte[] data)
        {
            var msg = Peer.CreateMessage();
            msg.Write(data);
            foreach (var connection in Peer.Connections)
            {
                Peer.SendMessage(msg, connection, channelId == 0 ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced, channelId);
            }
        }

        public void SendNetworkMessageToUser(long userId, byte channelId, byte[] data)
        {
            var msg = Peer.CreateMessage();
            msg.Write(data);

            var connection = Peer.Connections.FirstOrDefault(c => c.RemoteUniqueIdentifier == userId);
            if (connection != null)
            {
                Peer.SendMessage(msg, connection, channelId == 0 ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced, channelId);
            }
        }

        private static void StatusChanged(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus)msg.ReadByte();
            Console.WriteLine($"StatusChanged: {newStatus} - {msg.ReadString()}");

            switch (newStatus)
            {
                case NetConnectionStatus.Connected:
                    Demo.instance.JoinLobby(msg.SenderConnection.RemoteUniqueIdentifier, null);
                    break;
            }
        }

        private static void ConnectionApproval(NetIncomingMessage msg)
        {
            msg.SenderConnection.Approve();
        }

        private void Data(NetIncomingMessage msg)
        {
            var channelId = msg.SequenceChannel;

            if (channelId == 0)
            {
                HandleTCPData(msg.Data, Peer.UniqueIdentifier);
            }
            if (channelId == 1 && msg.Data.Length >= 4)
            {
                HandleUDPData(msg.Data, Peer.UniqueIdentifier);
            }
        }

        private bool HandleTCPData(byte[] _data, long userId)
        {
            int num = 0;
            Packet receivedData = new Packet(_data);
            receivedData.SetBytes(_data);
            if (receivedData.UnreadLength() >= 4)
            {
                num = receivedData.ReadInt(true);
                if (num <= 0)
                {
                    return true;
                }
            }
            while (num > 0 && num <= receivedData.UnreadLength())
            {
                using (Packet packet = new Packet(receivedData.ReadBytes(num, true)))
                {
                    int key = packet.ReadInt(true);
                    packetHandlers[key](packet, userId);
                }
                num = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    num = receivedData.ReadInt(true);
                    if (num <= 0)
                    {
                        return true;
                    }
                }
            }
            return num <= 1;
        }

        private void HandleUDPData(byte[] _data, long userId)
        {
            using (Packet packet = new Packet(_data))
            {
                try
                {
                    int length = packet.ReadInt(true);
                    _data = packet.ReadBytes(length, true);
                }
                catch
                {
                    Console.WriteLine("Failed to parse packet " + string.Join("-", packet.ToArray()));
                }
            }
            using (Packet packet2 = new Packet(_data))
            {
                int key = packet2.ReadInt(true);
                if (!packetHandlers.TryGetValue(key, out PacketHandler handler))
                {
                    Console.WriteLine("Could not find handler for key " + key);
                    return;
                }

                handler(packet2, userId);
            }
        }

        public void SendTCPData(Packet _packet, long userId)
        {
            _packet.WriteLength();

            if (userId == 0L)
            {
                DiscordController.instance.SendNetworkMessage(0, _packet.ToArray());
                return;
            }
            DiscordController.instance.SendNetworkMessageToUser(userId, 0, _packet.ToArray());
        }

        public void SendUDPData(Packet _packet, long userId)
        {
            _packet.WriteLength();
            if (userId == 0L)
            {
                DiscordController.instance.SendNetworkMessage(1, _packet.ToArray());
                return;
            }
            DiscordController.instance.SendNetworkMessageToUser(userId, 1, _packet.ToArray());
        }

        private void InitializePacketHandlers()
        {
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { 1, new PacketHandler(ClientHandle.Joined)},
                { 2, new PacketHandler(ClientHandle.Welcome)},
                { 3, new PacketHandler(ClientHandle.Ready)},
                { 4, new PacketHandler(ClientHandle.NotReady)},
                { 5, new PacketHandler(ClientHandle.Load)},
                { 6, new PacketHandler(ClientHandle.Mode)},
                { 7, new PacketHandler(ClientHandle.Map)},
                { 8, new PacketHandler(ClientHandle.Damage)},
                { 9, new PacketHandler(ClientHandle.Difficulty)},
                {10, new PacketHandler(ClientHandle.Lives)},
                {11, new PacketHandler(ClientHandle.Spawn)},
                {12, new PacketHandler(ClientHandle.Transform)},
                {13, new PacketHandler(ClientHandle.Physics)},
                {14, new PacketHandler(ClientHandle.Control)},
                {15, new PacketHandler(ClientHandle.Status)},
                {16, new PacketHandler(ClientHandle.Pickup)},
                {17, new PacketHandler(ClientHandle.Weapon)},
                {18, new PacketHandler(ClientHandle.Combo)},
                {19, new PacketHandler(ClientHandle.TrailerTransform)},
                {20, new PacketHandler(ClientHandle.TrailerDetach)},
                {21, new PacketHandler(ClientHandle.Destroyed)},
                {22, new PacketHandler(ClientHandle.Wrecked)},
                {23, new PacketHandler(ClientHandle.Totaled)},
                {24, new PacketHandler(ClientHandle.DropWeapon)},
                {25, new PacketHandler(ClientHandle.SpawnPickup)},
                {26, new PacketHandler(ClientHandle.ObjectDestroyed)},
                {27, new PacketHandler(ClientHandle.RandomNumber)},
                {28, new PacketHandler(ClientHandle.SpawnAI)},
                {29, new PacketHandler(ClientHandle.TransformAI)},
                {30, new PacketHandler(ClientHandle.PhysicsAI)},
                {31, new PacketHandler(ClientHandle.ControlAI)},
                {32, new PacketHandler(ClientHandle.StatusAI)},
                {33, new PacketHandler(ClientHandle.PickupAI)},
                {34, new PacketHandler(ClientHandle.WeaponAI)},
                {35, new PacketHandler(ClientHandle.ComboAI)},
                {36, new PacketHandler(ClientHandle.TrailerTransformAI)},
                {37, new PacketHandler(ClientHandle.TrailerDetachAI)},
                {38, new PacketHandler(ClientHandle.DestroyedAI)},
                {39, new PacketHandler(ClientHandle.WreckedAI)},
                {40, new PacketHandler(ClientHandle.TotaledAI)},
                {41, new PacketHandler(ClientHandle.DropWeaponAI)},
                {42, new PacketHandler(ClientHandle.Pause)},
                {43, new PacketHandler(HandleLobbyMetadata) }
            };
        }

        public void SetLobbyMetadata(string key, string value)
        {
            if (!lobbyMetadata.ContainsKey(key))
            {
                lobbyMetadata.Add(key, value);
            }
            else
            {
                lobbyMetadata[key] = value;
            }

            var packet = new Packet(43);
            packet.Write(key);
            packet.Write(value);

            SendTCPData(packet, Peer.UniqueIdentifier);
        }

        private void HandleLobbyMetadata(Packet _packet, long userId)
        {
            var key = _packet.ReadString();
            var value = _packet.ReadString();

            if (!lobbyMetadata.ContainsKey(key))
            {
                lobbyMetadata.Add(key, value);
            }
            else
            {
                lobbyMetadata[key] = value;
            }
        }
    }
}