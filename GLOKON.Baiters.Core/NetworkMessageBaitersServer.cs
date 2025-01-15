using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Enums.Networking;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;
using Steamworks.Data;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace GLOKON.Baiters.Core
{
    public sealed class NetworkMessageBaitersServer(IOptions<WebFishingOptions> options) : BaitersServer(options)
    {
        private readonly ConcurrentDictionary<ulong, NetIdentity> _connections = new();

        public override void Setup()
        {
            base.Setup();

            SteamNetworkingMessages.OnSessionRequest += SteamNetworkingMessages_OnSessionRequest;
            SteamNetworkingMessages.OnSessionFailed += SteamNetworkingMessages_OnSessionFailed;
            SteamNetworkingMessages.OnMessage += SteamNetworkingMessages_OnMessage;
        }

        public override void Stop()
        {
            base.Stop();

            SteamNetworkingMessages.OnSessionRequest -= SteamNetworkingMessages_OnSessionRequest;
            SteamNetworkingMessages.OnSessionFailed -= SteamNetworkingMessages_OnSessionFailed;
            SteamNetworkingMessages.OnMessage -= SteamNetworkingMessages_OnMessage;
        }

        internal override void LeavePlayer(ulong steamId, DisconnectReason reason = DisconnectReason.NormalLeave)
        {
            if (_connections.TryRemove(steamId, out var connection))
            {
                SteamNetworkingMessages.CloseSessionWithUser(ref connection);
            }

            base.LeavePlayer(steamId, reason);
        }

        protected override void ReceivePackets()
        {
            for (int channel = 0; channel < dataChannelCount; channel++)
            {
                SteamNetworkingMessages.Receive(channel);
            }
        }

        protected override void SendPacketTo(byte[] data, DataChannel channel)
        {
            foreach (var connection in _connections)
            {
                InternalSendPacket(connection.Key, connection.Value, data, channel);
            }
        }

        protected override void SendPacketTo(byte[] data, DataChannel channel, ulong steamId)
        {
            if (_connections.TryGetValue(steamId, out var netIdentity))
            {
                InternalSendPacket(steamId, netIdentity, data, channel);
            }
        }

        private void InternalSendPacket(ulong steamId, NetIdentity netIdentity, byte[] data, DataChannel channel)
        {
            if (SteamNetworkingMessages.SendMessageToUser(ref netIdentity, data, data.Length, (int)channel) != Result.OK)
            {
                Log.Error("Failed to send network message packet to {0}", steamId);
                LeavePlayer(steamId);
            }
        }

        private void SteamNetworkingMessages_OnMessage(NetIdentity identity, IntPtr data, int size, int channel)
        {
            byte[] messageData = new byte[size];
            Marshal.Copy(data, messageData, 0, size);

            HandleNetworkPacket(identity.SteamId, messageData, (DataChannel)channel);
        }

        private void SteamNetworkingMessages_OnSessionFailed(ConnectionInfo connection)
        {
            Log.Error("Failed connection for {0}", connection.Identity.SteamId);
            LeavePlayer(connection.Identity.SteamId);
        }

        private void SteamNetworkingMessages_OnSessionRequest(NetIdentity identity)
        {
            Log.Debug("New session request from {0}", identity.SteamId);

            if (CanSteamIdJoin(identity.SteamId))
            {
                Log.Debug("Session {0} can join, accepting session", identity.SteamId);
                if (SteamNetworkingMessages.AcceptSessionWithUser(ref identity) && _connections.TryAdd(identity.SteamId, identity))
                {
                    Log.Debug("Session {0} is accepted", identity.SteamId);
                    SendWebLobbyPacket(identity.SteamId);
                }
                else
                {
                    Log.Error("Failed to accept session request from {0}", identity.SteamId);
                }
            }
            else
            {
                Log.Debug("Session {0} is blocked from joining", identity.SteamId);
                SteamNetworkingMessages.CloseSessionWithUser(ref identity);
            }
        }
    }
}
