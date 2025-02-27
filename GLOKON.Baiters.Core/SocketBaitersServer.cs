﻿using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Enums.Networking;
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

        internal override Task RunAsync(CancellationToken cancellationToken)
        {
            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);
            return base.RunAsync(cancellationToken);
        }

        internal override void Stop()
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
                    SendHandshake(data.Identity.SteamId);
                    SendWebLobbyPacket(data.Identity.SteamId);
                }
                else
                {
                    Log.Error("Failed to accept connection from {0}", data.Identity.SteamId);
                    connection.Close();
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

            HandleNetworkPacket(identity.SteamId, messageData, (DataChannel)channel);
        }

        internal override void LeavePlayer(ulong steamId, DisconnectReason reason = DisconnectReason.NormalLeave)
        {
            base.LeavePlayer(steamId, reason);

            _connections.TryRemove(steamId, out _);
        }

        protected override void ReceivePackets()
        {
            _socketManager?.Receive();
        }

        protected override void SendPacketTo(byte[] data, DataChannel channel, bool reliable)
        {
            // Reliable/Unreliable not supported on Sockets
            foreach (var connection in _connections)
            {
                InternalSendPacket(connection.Key, connection.Value, data, channel);
            }
        }

        protected override void SendPacketTo(byte[] data, DataChannel channel, ulong steamId, bool reliable)
        {
            // Reliable/Unreliable not supported on Sockets
            if (_connections.TryGetValue(steamId, out var connection))
            {
                InternalSendPacket(steamId, connection, data, channel);
            }
        }

        private void InternalSendPacket(ulong steamId, Connection connection, byte[] data, DataChannel channel)
        {
            if (connection.SendMessage(data, laneIndex: (ushort)channel) != Result.OK)
            {
                Log.Error("Failed to send packet to {0}", steamId);
                LeavePlayer(steamId);
            }
        }
    }
}
