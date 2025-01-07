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
            Log.Debug("New connection request from {0}", data.Identity.SteamId);

            _connections.TryRemove(data.Identity.SteamId, out _);
            if (CanSteamIdJoin(data.Identity.SteamId))
            {
                Log.Debug("Connection {0} can join, accepting session", data.Identity.SteamId);
                if (connection.Accept() == Result.OK && _connections.TryAdd(data.Identity.SteamId, connection))
                {
                    Log.Debug("Connection {0} is accepted", data.Identity.SteamId);
                    SendWebLobbyPacket(data.Identity.SteamId);
                }
                else
                {
                    Log.Error("Failed to accept connection from {0}", data.Identity.SteamId);
                }
            }
            else
            {
                Log.Debug("Connection {0} is blocked from joining", data.Identity.SteamId);
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
                    Log.Error("Failed to send packet to {0}", steamId);
                    LeavePlayer(steamId);
                }
            }
        }
    }
}
