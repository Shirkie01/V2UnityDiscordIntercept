using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V2UnityDiscordIntercept
{
    public class VigServer : Network
    {
        public override NetPeer Peer => server;
        public int Port { get; }
        private NetServer server;        

        public VigServer(int port)
        {
            Port = port;
        }

        public void CreateLobby()
        {
            // Close the window if we got it open somehow.
            Plugin.ShowConnectionWindow = false;

            var config = new NetPeerConfiguration(Plugin.AppIdentifier)
            {
                Port = Port
            };
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);

            server = new NetServer(config);
            server.Start();
            Logger.Log($"Created server with id: {server.UniqueIdentifier}");
        }

        private void CreateDiscoveryResponse(NetIncomingMessage msg)
        {
            Logger.Log("Creating discovery response.");

            // Create a response and write some example data to it
            NetOutgoingMessage response = server.CreateMessage();
            response.Write($"Found server running on port:{server.Port}");

            // Send the response to the sender of the request
            server.SendDiscoveryResponse(response, msg.SenderEndPoint);
        }

        public void DeleteLobby()
        {
            Logger.Log("Deleting the lobby");
            server.Shutdown("The server is shutting down.");
            server = null;
        }

        private void ConnectionApproval(NetIncomingMessage msg)
        {
            Logger.Log($"Connection approval requested from {msg.SenderConnection.RemoteUniqueIdentifier}");

            // Only allow new connections if we are still in the debug menu.
            if (GameManager.instance.inDebug)
            {
                // Provide the new client with a member id.
                var hail = server.CreateMessage();
                hail.Write(server.ConnectionsCount);
                msg.SenderConnection.Approve(hail);
            }
            else
            {
                msg.SenderConnection.Deny("Currently in game!");
            }
        }

        public override void ReadMessages()
        {
            NetIncomingMessage msg;
            while ((msg = Peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        CreateDiscoveryResponse(msg);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Logger.Log(msg.ReadString());
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        ConnectionApproval(msg);
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
        }

        private void Data(NetIncomingMessage msg)
        {
            long fromUserId = 0L;
            long toUserId = 0L;

            try
            {
                // We got some data. It needs to either be relayed to a specific user,
                // or relayed to everybody. We recreate the message excluding the target user id,
                // as the target user id is specifically for the server to know where to send the data.
                fromUserId = GetUserIdFromNetworkMessage(msg, 0);
                toUserId = GetUserIdFromNetworkMessage(msg, 8);

                // We recreate the message excluding the target user id.
                var relayMsg = server.CreateMessage();

                relayMsg.Write(fromUserId);

                // Since we are manually writing the fromUserId, we take away both the originator and target user ids
                // from the data, and then write the new data without those, as the fromUserId is already included
                // and we don't care about sending the targetUserId to the clients.
                var relayData = new byte[msg.Data.Length - 16];
                Array.Copy(msg.Data, 16, relayData, 0, relayData.Length);
                relayMsg.Write(relayData);

                var channelId = msg.SequenceChannel;

                // Relay the message to all excluding sender
                if (toUserId == 0L && server.Connections.Any(c => c.RemoteUniqueIdentifier != fromUserId))
                {
                    SendDataMessageToAll(relayMsg, fromUserId, channelId);
                }
                // Send to target user
                else if (toUserId != 0L)
                {
                    SendDataMessageToUser(relayMsg, toUserId, channelId);
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.ToString() + $"{fromUserId} :" + string.Join(",", msg.Data));
            }
        }

        private void SendDataMessageToAll(NetOutgoingMessage relayMsg, long fromUserId, int channelId)
        {
            server.SendMessage(relayMsg, server.Connections.Where(c => c.RemoteUniqueIdentifier != fromUserId).ToList(), GetDeliveryMethod(channelId), channelId);
        }

        private void SendDataMessageToUser(NetOutgoingMessage relayMsg, long toUserId, int channelId)
        {
            var targetConnection = server.Connections.FirstOrDefault(c => c.RemoteUniqueIdentifier == toUserId);
            if (targetConnection == null)
            {
                Logger.Log($"Unable to find connection with id: {toUserId}");
                return;
            }
            server.SendMessage(relayMsg, targetConnection, GetDeliveryMethod(channelId), channelId);
        }

        private long GetUserIdFromNetworkMessage(NetIncomingMessage msg, int srcOffset)
        {
            byte[] userIdBytes = new byte[8];
            Array.Copy(msg.Data, srcOffset, userIdBytes, 0, 8);
            var fromUserId = BitConverter.ToInt64(userIdBytes);
            return fromUserId;
        }

        private void StatusChanged(NetIncomingMessage msg)
        {
            var newStatus = (NetConnectionStatus)msg.ReadByte();

            switch (newStatus)
            {
                case NetConnectionStatus.Disconnected:
                    OnMemberDisconnected(msg.SenderConnection.RemoteUniqueIdentifier);
                    break;
                default:
                    Logger.Log($"StatusChanged: {newStatus}:{msg.SenderConnection.RemoteUniqueIdentifier} - {msg.ReadString()}");
                    break;
            }

        }

        private void OnMemberDisconnected(long userId)
        {
            var msg = server.CreateMessage();
            msg.Write(userId);

            using (var _packet = new Packet(44))
            {
                _packet.WriteLength();
                msg.Write(_packet.ToArray());
            }

            SendDataMessageToAll(msg, userId, 0);
        }
    }
}
