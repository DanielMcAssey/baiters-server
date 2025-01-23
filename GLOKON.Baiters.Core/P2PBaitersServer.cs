using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Enums.Networking;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class P2PBaitersServer(IOptions<WebFishingOptions> options) : BaitersServer(options)
    {
        public override void Setup()
        {
            base.Setup();

            SteamNetworking.AllowP2PPacketRelay(true);
            SteamNetworking.OnP2PSessionRequest = (steamId) =>
            {
                Log.Debug("New P2P session request from {0}", steamId);

                if (CanSteamIdJoin(steamId))
                {
                    Log.Debug("P2P session {0} can join, accepting session", steamId);
                    if (SteamNetworking.AcceptP2PSessionWithUser(steamId))
                    {
                        Log.Debug("P2P session {0} is accepted", steamId);
                        SendWebLobbyPacket(steamId);
                    }
                    else
                    {
                        Log.Error("Failed to accept P2P session request from {0}", steamId);
                    }
                }
                else
                {
                    Log.Debug("P2P session {0} is blocked from joining", steamId);
                    SteamNetworking.CloseP2PSessionWithUser(steamId);
                }
            };
            SteamNetworking.OnP2PConnectionFailed = (steamId, error) =>
            {
                Log.Error("Failed to create P2P connection for {steamId}, caused by {error}", steamId, error);
                LeavePlayer(steamId);
            };
        }

        internal override void LeavePlayer(ulong steamId, DisconnectReason reason = DisconnectReason.NormalLeave)
        {
            base.LeavePlayer(steamId, reason);
            SteamNetworking.CloseP2PSessionWithUser(steamId);
        }

        protected override void ReceivePackets()
        {
            for (int channel = 0; channel < dataChannelCount; channel++)
            {
                while (SteamNetworking.IsP2PPacketAvailable(channel))
                {
                    var packet = SteamNetworking.ReadP2PPacket(channel);
                    if (packet.HasValue)
                    {
                        HandleNetworkPacket(packet.Value.SteamId, packet.Value.Data, (DataChannel)channel);
                    }
                }
            }
        }


        protected override void SendPacketTo(byte[] data, DataChannel channel, bool reliable)
        {
            foreach (var player in Players)
            {
                InternalSendPacket(player.Key, data, channel, reliable);
            }
        }

        protected override void SendPacketTo(byte[] data, DataChannel channel, ulong steamId, bool reliable)
        {
            InternalSendPacket(steamId, data, channel, reliable);
        }

        private void InternalSendPacket(ulong steamId, byte[] data, DataChannel channel, bool reliable)
        {
            if (!SteamNetworking.SendP2PPacket(steamId, data, data.Length, (int)channel, reliable ? P2PSend.Reliable : P2PSend.Unreliable))
            {
                Log.Error("Failed to send P2P packet to {0}", steamId);
                LeavePlayer(steamId);
            }
        }
    }
}
