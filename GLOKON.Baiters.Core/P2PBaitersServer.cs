using GLOKON.Baiters.Core.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class P2PBaitersServer(IOptions<WebFishingOptions> options) : BaitersServer(options)
    {
        private const int p2pChannelCount = 6; // The amount of P2P channels used

        public override void Setup()
        {
            base.Setup();

            SteamNetworking.AllowP2PPacketRelay(true);
            SteamNetworking.OnP2PSessionRequest = (steamId) =>
            {
                Log.Debug("New P2P session request from {0}", steamId);

                if (CanSteamIdJoin(steamId))
                {
                    if (SteamNetworking.AcceptP2PSessionWithUser(steamId))
                    {
                        SendWebLobbyPacket(steamId);
                    }
                    else
                    {
                        Log.Error("Failed to accept P2P session request from {0}", steamId);
                    }
                }
                else
                {
                    SteamNetworking.CloseP2PSessionWithUser(steamId);
                }
            };
            SteamNetworking.OnP2PConnectionFailed = (steamId, error) =>
            {
                Log.Error($"Failed to create P2P connection for {steamId}, caused by {error}");
                LeavePlayer(steamId);
            };
        }

        internal override void LeavePlayer(ulong steamId)
        {
            base.LeavePlayer(steamId);

            SteamNetworking.CloseP2PSessionWithUser(steamId);
        }

        protected override void ReceivePackets()
        {
            for (int channel = 0; channel < p2pChannelCount; channel++)
            {
                while (SteamNetworking.IsP2PPacketAvailable(channel))
                {
                    var packet = SteamNetworking.ReadP2PPacket(channel);
                    if (packet.HasValue)
                    {
                        HandleNetworkPacket(packet.Value.SteamId, packet.Value.Data);
                    }
                }
            }
        }

        protected override void SendPacketTo(ulong steamId, byte[] data)
        {
            if (!SteamNetworking.SendP2PPacket(steamId, data, nChannel: 2))
            {
                Log.Error("Failed to send P2P packet to {0}", steamId);
            }
        }
    }
}
