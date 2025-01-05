using GLOKON.Baiters.Core.Packets.Handlers;
using Serilog;
using Steamworks;

namespace GLOKON.Baiters.Core.Packets
{
    public sealed class PacketManager
    {
        private readonly Dictionary<string, IPacketHandler> handlers = [];

        public void Setup(BaitersServer server)
        {
            handlers.Add("handshake_request", new HandshakeRequestHandler(server));
            handlers.Add("new_player_join", new NewPlayerJoinHandler(server));
            handlers.Add("instance_actor", new InstanceActorHandler(server));
            handlers.Add("actor_update", new ActorUpdateHandler(server));
            handlers.Add("request_ping", new RequestPingHandler(server));
            handlers.Add("actor_action", new ActorActionHandler(server));
            handlers.Add("request_actors", new RequestActorsHandler(server));
            handlers.Add("chalk_packet", new ChalkPacketHandler(server));
        }

        public void Handle(string type, SteamId sender, Dictionary<string, object> data)
        {
            Log.Debug($"Handling packet {type} for {sender}");

            if (handlers.TryGetValue(type, out var handler) && handler != null)
            {
                handler.Handle(sender, data);
            }
        }
    }
}
