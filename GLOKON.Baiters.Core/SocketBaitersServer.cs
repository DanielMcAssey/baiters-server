using GLOKON.Baiters.Core.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;
using Steamworks.Data;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace GLOKON.Baiters.Core
{
    public sealed class SocketBaitersServer(IOptions<WebFishingOptions> options) : BaitersServer(options), ISocketManager
    {
        private readonly ConcurrentDictionary<ulong, Connection> _connections = new();

        private SocketManager? _socketManager;

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);
            return base.RunAsync(cancellationToken);
        }

        public override void Stop()
        {
            if (_socketManager != null)
            {
                try
                {
                    _socketManager.Close();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to cleanup Steam socket manager");
                }
                finally
                {
                    _socketManager = null;
                }
            }

            base.Stop();
        }

        public void OnConnecting(Connection connection, ConnectionInfo data)
        {
            Log.Debug("{0} is connecting", data.Identity);

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

        public void OnConnected(Connection connection, ConnectionInfo data)
        {
            Log.Information("{0} has joined the game", data.Identity);
        }

        public void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            Log.Information("{0} is out of here", data.Identity);
            LeavePlayer(data.Identity.SteamId);
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            Log.Debug("We got a message from {0}!", identity);

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
