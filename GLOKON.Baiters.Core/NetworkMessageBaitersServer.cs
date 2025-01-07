using GLOKON.Baiters.Core.Configuration;
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
        private const int p2pChannelCount = 6; // The amount of P2P channels used

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

        internal override void LeavePlayer(ulong steamId)
        {
            if (_connections.TryRemove(steamId, out var connection))
            {
                SteamNetworkingMessages.CloseSessionWithUser(ref connection);
            }

            base.LeavePlayer(steamId);
        }

        protected override void ReceivePackets()
        {
            for (int channel = 0; channel < p2pChannelCount; channel++)
            {
                SteamNetworkingMessages.Receive(channel);
            }
        }

        protected override void SendPacketTo(ulong steamId, byte[] data)
        {
            if (_connections.TryGetValue(steamId, out var connection))
            {
                if (SteamNetworkingMessages.SendMessageToUser(ref connection, data, nRemoteChannel: 2) != Result.OK)
                {
                    Log.Error("Failed to send packet to {0}", steamId);
                    LeavePlayer(steamId);
                }
            }
        }

        private void SteamNetworkingMessages_OnMessage(NetIdentity identity, IntPtr data, int size, int channel)
        {
            byte[] messageData = new byte[size];
            Marshal.Copy(data, messageData, 0, size);

            HandleNetworkPacket(identity.SteamId, messageData);
        }

        private void SteamNetworkingMessages_OnSessionFailed(ConnectionInfo connection)
        {
            var steamId = connection.Identity.SteamId;
            Log.Error($"Failed connection for {steamId}");
            LeavePlayer(steamId);
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
