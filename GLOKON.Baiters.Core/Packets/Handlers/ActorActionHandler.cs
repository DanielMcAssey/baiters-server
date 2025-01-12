using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Game;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorActionHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            string action = (string)data["action"];
            long actorId = (long)data["actor_id"];
            Array actionParams = (Array)data["params"];

            switch (action)
            {
                case "_update_cosmetics":
                    return; // TODO: Needs testing
                    var cosmeticsPkt = (Dictionary<string, object>)(actionParams.GetValue(0) ?? new Dictionary<string, object>());

                    if (server.TryGetPlayer(sender, out var playerCosmetic) && playerCosmetic != null)
                    {
                        playerCosmetic.Cosmetics = new()
                        {
                            Title = (string)cosmeticsPkt["title"],
                            Eye = (string)cosmeticsPkt["eye"],
                            Nose = (string)cosmeticsPkt["nose"],
                            Mouth = (string)cosmeticsPkt["mouth"],
                            Undershirt = (string)cosmeticsPkt["undershirt"],
                            Overshirt = (string)cosmeticsPkt["overshirt"],
                            Legs = (string)cosmeticsPkt["legs"],
                            Hat = (string)cosmeticsPkt["hat"],
                            Species = (string)cosmeticsPkt["species"],
                            Accessory = ((Array)cosmeticsPkt["accessory"]).OfType<string>().ToArray(),
                            Pattern = (string)cosmeticsPkt["pattern"],
                            PrimaryColor = (string)cosmeticsPkt["primary_color"],
                            SecondaryColor = (string)cosmeticsPkt["secondary_color"],
                            Tail = (string)cosmeticsPkt["tail"],
                            Bobber = (string?)cosmeticsPkt.GetValueOrDefault("bobber"),
                        };
                    }
                    break;
                case "_update_held_item":
                    return; // TODO: Needs testing
                    var heldItemPkt = (Dictionary<string, object>)(actionParams.GetValue(0) ?? new Dictionary<string, object>());

                    if (server.TryGetPlayer(sender, out var playerHeldItem) && playerHeldItem != null)
                    {
                        playerHeldItem.HeldItem = new() {
                            Id = (string)heldItemPkt["id"],
                            Size = (float)heldItemPkt["size"],
                            Quality = (ItemQuality)heldItemPkt["quality"]
                        };
                    }
                    break;
                case "_sync_sound":
                case "_talk":
                case "_face_emote":
                case "_play_particle":
                case "_play_sfx":
                case "_sync_strum":
                case "_sync_hammer":
                case "_sync_punch":
                case "queue_free":
                case "_change_id":
                case "_set_state":
                case "_flush":
                    // TODO: Shall we do something with this?
                    break;
                case "_set_zone":
                    // TODO: Set actor zone
                    break;
                case "_sync_create_bubble":
                    string chatMssage = (string)(actionParams.GetValue(0) ?? -1);
                    server.OnPlayerChat(sender, chatMssage);
                    break;
                case "_wipe_actor":
                    long wipeActorId = (long)(actionParams.GetValue(0) ?? -1);
                    if (server.TryGetActor(wipeActorId, out var actor) && actor != null)
                    {
                        if (ActorType.ServerOnly.Contains(actor.Type))
                        {
                            return;
                        }

                        Log.Debug("Player asked to remove {0} actor", actor.Type);
                        server.RemoveActor(wipeActorId);
                    }
                    break;


                default:
                    Log.Verbose("Unknown actor action {0}", action);
                    break;
            }
        }
    }
}
