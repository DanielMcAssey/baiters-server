using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Models.Networking;
using GLOKON.Baiters.Core.Packets.Handlers;
using Microsoft.Extensions.Options;
using Serilog;

namespace GLOKON.Baiters.Core.Packets
{
    public sealed class PacketManager(IOptions<WebFishingOptions> options) : IPacketHandler
    {
        private readonly Dictionary<string, IPacketHandler> handlers = [];

        public void Setup(BaitersServer server)
        {
            handlers.Add("handshake_request", new HandshakeRequestHandler(server));
            handlers.Add("new_player_join", new NewPlayerJoinHandler(server, options.Value.JoinMessage));
            handlers.Add("instance_actor", new InstanceActorHandler(server));
            handlers.Add("actor_update", new ActorUpdateHandler(server));
            handlers.Add("request_ping", new RequestPingHandler(server));
            handlers.Add("actor_action", new ActorActionHandler(server));
            handlers.Add("request_actors", new RequestActorsHandler(server));
            handlers.Add("chalk_packet", new ChalkPacketHandler(server));
        }

        public void Handle(ulong sender, Packet packet)
        {
            string type = packet.Type;
            Log.Debug("Handling packet {type} for {sender}", type, sender);

            if (handlers.TryGetValue(type, out var handler) && handler != null)
            {
                handler.Handle(sender, packet);
            }
        }
    }
}
