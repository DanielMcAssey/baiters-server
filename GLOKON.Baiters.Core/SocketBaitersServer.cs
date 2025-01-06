using GLOKON.Baiters.Core.Chat;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Packets;
using Microsoft.Extensions.Options;
using Steamworks.Data;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace GLOKON.Baiters.Core
{
    public sealed class SocketBaitersServer(
        IOptions<WebFishingOptions> options,
        PacketManager packetManager,
        ChatManager chatManager
        ) : BaitersServer(options, packetManager, chatManager)
    {
        private readonly ConcurrentDictionary<ulong, Connection> _connections = new();

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            _connections.TryRemove(data.Identity.SteamId, out _);
            if (CanSteamIdJoin(data.Identity.SteamId) && _connections.TryAdd(data.Identity.SteamId, connection))
            {
                connection.Accept();
                SendWebLobbyPacket(data.Identity.SteamId);
            }
            else
            {
                connection.Close();
            }
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            LeavePlayer(data.Identity.SteamId);
            _connections.TryRemove(data.Identity.SteamId, out _);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] messageData = new byte[size];
            Marshal.Copy(data, messageData, 0, size);

            HandleNetworkPacket(identity.SteamId, messageData);
        }

        protected override void ReceivePackets()
        {
            _socketManager?.Receive();
        }

        protected override void SendPacketTo(ulong steamId, byte[] data)
        {
            if (_connections.TryGetValue(steamId, out var connection))
            {
                connection.SendMessage(data, laneIndex: 2);
            }
        }
    }
}
