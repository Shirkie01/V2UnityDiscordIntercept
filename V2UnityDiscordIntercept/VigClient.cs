using Discord;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace V2UnityDiscordIntercept
{
    public class VigClient : VigPeer
    {
        protected override NetPeer Peer => client;

        private NetClient client;
        public int MemberId { get; set; }

        private Dictionary<string, string> lobbyMetadata = new Dictionary<string, string>();        

        public void Disconnect(string byeMessage)
        {
            client.ServerConnection.Disconnect(byeMessage);
        }

        private void HandleMemberDisconnected(Packet _packet, long userId)
        {
            if (GameManager.instance.inDebug)
            {
                Demo.instance.MemberLeft(userId);
                return;
            }
            GameObject.Destroy(GameManager.instance.networkMembers[userId].unit);
            GameManager.instance.FUN_309A0(GameManager.instance.networkMembers[userId]);
            GameManager.instance.networkMembers.Remove(userId);
            GameManager.instance.networkIds.Remove(userId);
        }

        public void ConnectToLobby(string ipAddress, int port)
        {
            Logger.Log($"Connecting to server at {ipAddress}:{port}");
            var config = new NetPeerConfiguration(Plugin.AppIdentifier);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            client = new NetClient(config);
            client.Start();
            var netConnection = client.Connect(ipAddress, port);
            Logger.Log($"Created connection with id: {client.UniqueIdentifier}");
        }

        public void JoinLobby(long lobbyId, string secret)
        {
            Logger.Log($"Joined Lobby");
            InitializePacketHandlers();
            ClientSend.Joined();
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
                {43, new PacketHandler(HandleLobbyMetadata) },
                {44, new PacketHandler(HandleMemberDisconnected) }
            };
        }

        private void StatusChanged(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus)msg.ReadByte();
            Logger.Log($"StatusChanged: {newStatus} - {msg.ReadString()}");

            switch (newStatus)
            {
                case NetConnectionStatus.Connected:
                    Plugin.ShowConnectionWindow = false;
                    MemberId = msg.SenderConnection.RemoteHailMessage.ReadInt32();
                    Logger.Log($"MemberId: {MemberId}");
                    Demo.instance.JoinLobby(0L, null);
                    break;
                case NetConnectionStatus.Disconnected:
                    Plugin.ShowConnectionWindow = false;
                    GameManager.instance.LoadDebug();
                    break;
            }
        }

        private void Data(NetIncomingMessage msg)
        {
            // Gets the actual originator user id
            long fromUserId = GetUserIdFromNetworkMessage(msg);

            var data = new byte[msg.Data.Length - 8];
            Array.Copy(msg.Data, 8, data, 0, data.Length);
            var channelId = msg.SequenceChannel;

            if (channelId == 0)
            {
                HandleTCPData(data, fromUserId);
            }
            if (channelId == 1 && data.Length >= 4)
            {
                HandleUDPData(data, fromUserId);
            }
        }

        private long GetUserIdFromNetworkMessage(NetIncomingMessage msg)
        {
            byte[] userIdBytes = new byte[8];
            Array.Copy(msg.Data, 0, userIdBytes, 0, 8);
            var fromUserId = BitConverter.ToInt64(userIdBytes);
            return fromUserId;
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
                int length = packet.ReadInt(true);
                _data = packet.ReadBytes(length, true);
            }
            using (Packet packet2 = new Packet(_data))
            {
                int key = packet2.ReadInt(true);
                if (!packetHandlers.TryGetValue(key, out PacketHandler handler))
                {
                    Logger.Log("Could not find handler for key " + key);
                    return;
                }

                handler(packet2, userId);
            }
        }

        public override void ReadMessages()
        {
            NetIncomingMessage msg;
            while ((msg = Peer.ReadMessage()) != null)
            {
                try
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            Logger.Log($"Found server at {msg.SenderEndPoint} with name {msg.ReadString()}");
                            break;

                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Logger.Log(msg.ReadString());
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            StatusChanged(msg);
                            break;

                        case NetIncomingMessageType.Data:
                            Data(msg);
                            break;

                        default:
                            Logger.Log("Unhandled type: " + msg.MessageType + "\n" + msg.ToString());
                            break;
                    }
                    Peer.Recycle(msg);
                }
                catch (Exception e)
                {
                    Logger.Log($"Bad message: {string.Join(",", msg.Data)}.\r\n {e}");
                }
            }
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

        public void SendNetworkMessage(byte channelId, byte[] data)
        {
            // This method just sends the data to the server. We include the originator id and
            // a zero as the target user id so the server knows to relay it to everybody.

            var msg = client.CreateMessage();
            // from
            msg.Write(client.UniqueIdentifier);
            // to
            msg.Write(0L);
            // data
            msg.Write(data);
            client.SendMessage(msg, client.ServerConnection, GetDeliveryMethod(channelId), channelId);
        }

        public void SendNetworkMessageToUser(long userId, byte channelId, byte[] data)
        {
            // This method just sends the data to the server. We include the originator id and
            // the target user id so the server knows who to forward it to.
            var msg = client.CreateMessage();
            // from
            msg.Write(client.UniqueIdentifier);
            // to
            msg.Write(userId);
            // data
            msg.Write(data);
            client.SendMessage(msg, client.ServerConnection, GetDeliveryMethod(channelId), channelId);
        }
    }
}
