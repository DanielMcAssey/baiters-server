using GLOKON.Baiters.Core.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;
using Steamworks.Data;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace GLOKON.Baiters.Core
{
    public sealed class SocketBaitersServer(IOptions<WebFishingOptions> options) : BaitersServer(options)
    {
        private readonly ConcurrentDictionary<ulong, Connection> _connections = new();

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            _connections.TryRemove(data.Identity.SteamId, out _);
            if (CanSteamIdJoin(data.Identity.SteamId))
            {
                if (connection.Accept() == Result.OK && _connections.TryAdd(data.Identity.SteamId, connection))
                {
                    SendWebLobbyPacket(data.Identity.SteamId);
                }
                else
                {
                    Log.Error("Failed to accept connection from {0}", data.Identity.SteamId);
                }
            }
            else
            {
                connection.Close();
            }
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            LeavePlayer(data.Identity.SteamId);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] messageData = new byte[size];
            Marshal.Copy(data, messageData, 0, size);

            HandleNetworkPacket(identity.SteamId, messageData);
        }

        internal override void LeavePlayer(ulong steamId)
        {
            base.LeavePlayer(steamId);

            _connections.TryRemove(steamId, out _);
        }

        protected override void ReceivePackets()
        {
            _socketManager?.Receive();
        }

        protected override void SendPacketTo(ulong steamId, byte[] data)
        {
            if (_connections.TryGetValue(steamId, out var connection))
            {
                if (connection.SendMessage(data, laneIndex: 2) != Result.OK)
                {
                    LeavePlayer(steamId);
                }
            }
        }
    }
}
