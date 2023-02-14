using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace V2UnityDiscordIntercept
{
    public abstract class VigPeer
    {
        protected abstract NetPeer Peer { get; }

        protected delegate void PacketHandler(Packet _packet, long userId);

        protected IDictionary<int, PacketHandler> packetHandlers = new Dictionary<int, PacketHandler>();
        
        public long Identifier => Peer.UniqueIdentifier;

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

        public abstract void ReadMessages();

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
        
        protected static NetDeliveryMethod GetDeliveryMethod(int channelId)
        {
            return channelId == 0 ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;
        }
    }
}