using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Networking;
using GLOKON.Baiters.Core.Packets.Handlers;
using Microsoft.Extensions.Options;
using Serilog;

namespace GLOKON.Baiters.Core.Packets
{
    public sealed class PacketManager
    {
        private readonly WebFishingOptions options;
        private readonly Dictionary<string, IPacketHandler> handlers = [];
        private readonly BaitersServer server;

        public PacketManager(IOptions<WebFishingOptions> options, BaitersServer server)
        {
            this.options = options.Value;
            this.server = server;
            server.OnPacket += Server_OnPacket;
        }

        public void Setup()
        {
            handlers.Add(PacketType.Handshake, new HandshakeHandler(server));
            handlers.Add(PacketType.NewPlayerJoin, new NewPlayerJoinHandler(server, options.CommandPrefix, options.JoinMessage));
            handlers.Add(PacketType.InstanceActor, new InstanceActorHandler(server));
            handlers.Add(PacketType.ActorUpdate, new ActorUpdateHandler(server));
            handlers.Add(PacketType.ActorAnimationUpdate, new ActorAnimationUpdateHandler());
            handlers.Add(PacketType.ActorAction, new ActorActionHandler(server));
            handlers.Add(PacketType.RequestActors, new RequestActorsHandler(server));
            handlers.Add(PacketType.ChalkPacket, new ChalkPacketHandler(server));
            handlers.Add(PacketType.Message, new MessageHandler(server));
            handlers.Add(PacketType.LetterReceived, new LetterReceivedHandler(server));
            handlers.Add(PacketType.LetterWasAccepted, new LetterWasAcceptedHandler());
            handlers.Add(PacketType.LetterWasDenied, new LetterWasDeniedHandler());
            handlers.Add(PacketType.PlayerPunch, new PlayerPunchHandler());
            handlers.Add(PacketType.UserJoinedWebLobby, new UserJoinedWebLobbyHandler());
            handlers.Add(PacketType.UserLeftWebLobby, new UserLeftWebLobbyHandler());
            handlers.Add(PacketType.ReceiveWebLobby, new ReceiveWebLobbyHandler());
        }

        private void Server_OnPacket(ulong sender, Packet packet)
        {
            Log.Verbose("Handling packet {0} for {1}", packet.Type, sender);

            if (handlers.TryGetValue(packet.Type, out var handler) && handler != null)
            {
                try
                {
                    handler.Handle(sender, packet);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to handle packet {0} from {1}", packet.Type, sender);
                }
            }
            else
            {
                Log.Warning("Received unknown packet {0} from {1}", packet.Type, sender);
            }
        }
    }
}
