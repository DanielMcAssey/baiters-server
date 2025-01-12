using GLOKON.Baiters.Core.Configuration;
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
            handlers.Add("handshake", new HandshakeHandler(server));
            handlers.Add("new_player_join", new NewPlayerJoinHandler(server, options.CommandPrefix, options.JoinMessage));
            handlers.Add("instance_actor", new InstanceActorHandler(server));
            handlers.Add("actor_update", new ActorUpdateHandler(server));
            handlers.Add("actor_animation_update", new ActorAnimationUpdateHandler(server));
            handlers.Add("request_ping", new RequestPingHandler(server));
            handlers.Add("actor_action", new ActorActionHandler(server));
            handlers.Add("request_actors", new RequestActorsHandler(server));
            handlers.Add("chalk_packet", new ChalkPacketHandler(server));
            handlers.Add("message", new MessageHandler(server));
        }

        private void Server_OnPacket(ulong sender, Packet packet)
        {
            Log.Verbose("Handling packet {0} for {1}", packet.Type, sender);

            if (handlers.TryGetValue(packet.Type, out var handler) && handler != null)
            {
                handler.Handle(sender, packet);
            }
            else
            {
                Log.Warning("Received unknown packet {0} from {1}", packet.Type, sender);
            }
        }
    }
}
