﻿using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Game;
using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class ActorActionHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet data)
        {
            var action = (string)data["action"];
            var actorId = (long)data["actor_id"];
            var actionParams = (Array)data["params"];

            switch (action)
            {
                case "_update_cosmetics":
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
                    var heldItemPkt = (Dictionary<string, object>)(actionParams.GetValue(0) ?? new Dictionary<string, object>());

                    if (server.TryGetPlayer(sender, out var playerHeldItem) && playerHeldItem != null)
                    {
                        playerHeldItem.HeldItem = new()
                        {
                            Id = (string)heldItemPkt["id"],
                            Size = (double)heldItemPkt["size"],
                            Quality = (ItemQuality)Enum.ToObject(typeof(ItemQuality), heldItemPkt["quality"]),
                        };
                    }
                    break;
                case "_sync_level_bubble":
                    if (server.TryGetPlayer(sender, out var playerLevelUp) && playerLevelUp != null)
                    {
                        Log.Information("{0} has levelled up", playerLevelUp.FisherName);
                    }
                    break;
                case "_face_emote":
                    if (server.TryGetPlayer(sender, out var playerFaceEmote) && playerFaceEmote != null)
                    {
                        string emote = (string)(actionParams.GetValue(0) ?? "unknown");
                        playerFaceEmote.LastEmote = emote;
                        Log.Information("{0} emoted and is {1}", playerFaceEmote.FisherName, emote);
                    }
                    break;
                case "_change_id":
                    // TODO: Change Actor ID
                    break;
                case "_set_zone":
                    // TODO: Set actor zone
                    break;
                case "_sync_create_bubble":
                    // Uses Message handler instead as we get more data
                    break;
                case "_wipe_actor":
                    long wipeActorId = (long)(actionParams.GetValue(0) ?? -1);
                    if (server.TryGetActor(wipeActorId, out var actor) && actor != null && actor.OwnerId == server.ServerId)
                    {
                        if (ActorType.ServerOnly.Contains(actor.Type))
                        {
                            return;
                        }

                        Log.Debug("Player asked to remove {0} actor", actor.Type);
                        server.RemoveActor(wipeActorId);
                    }
                    break;
                case "_talk": // Play player speech audio (single character per packet)
                case "_play_particle": // Play particle
                case "_play_sfx": // Play audio sfx
                case "_sync_strum": // Play guitar strum
                case "_sync_hammer": // Play guitar hammer
                case "_sync_punch": // Show punch effects for local player
                case "_flush": // Flush toilet
                case "_set_state": // Set boombox state
                case "queue_free":
                    // Not required for the server
                    break;
                case "_sync_sound":
                    // Unused
                    break;
                default:
                    Log.Verbose("Unknown actor action {0}", action);
                    break;
            }
        }
    }
}
