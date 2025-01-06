using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Packets;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core
{
    public sealed class P2PBaitersServer(
        IOptions<WebFishingOptions> options,
        PacketManager packetManager) : BaitersServer(options, packetManager)
    {
        private const int p2pChannelCount = 6; // The amount of P2P channels used

        public override void Setup()
        {
            base.Setup();

            SteamNetworking.OnP2PSessionRequest = (steamId) =>
            {
                if (CanSteamIdJoin(steamId))
                {
                    SteamNetworking.AcceptP2PSessionWithUser(steamId);
                    SendWebLobbyPacket(steamId);
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
            SteamNetworking.SendP2PPacket(steamId, data, nChannel: 2);
        }
    }
}
