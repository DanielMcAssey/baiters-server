using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Game;
using System.Numerics;

namespace GLOKON.Baiters.Core
{
    public sealed class ActorActioner(BaitersServer server)
    {
        public void SendTalk(char letter, float pitch = 1.5f, ulong? steamId = null)
        {
            server.SendPacket(new("actor_action")
            {
                ["action"] = "_talk",
                ["actor_id"] = (long)server.ServerId,
                ["params"] = new object[]
                {
                    letter.ToString(),
                    pitch,
                },
            }, DataChannel.Speech, steamId);
        }

        public void PlayGuitarStrum(string guitarString, int guitarFret, float volume, ulong? steamId = null)
        {
            server.SendPacket(new("actor_action")
            {
                ["action"] = "_sync_strum",
                ["actor_id"] = (long)server.ServerId,
                ["params"] = new object[]
                {
                    guitarString,
                    guitarFret,
                    volume,
                },
            }, DataChannel.Guitar, steamId);
        }

        public void PlayGuitarHammer(string guitarString, int guitarFret, ulong? steamId = null)
        {
            server.SendPacket(new("actor_action")
            {
                ["action"] = "_sync_hammer",
                ["actor_id"] = (long)server.ServerId,
                ["params"] = new object[]
                {
                    guitarString,
                    guitarFret,
                },
            }, DataChannel.Guitar, steamId);
        }

        public void PlayParticle(string id, Vector3 offset, bool global, ulong? steamId = null)
        {
            server.SendPacket(new("actor_action")
            {
                ["action"] = "_play_particle",
                ["actor_id"] = (long)server.ServerId,
                ["params"] = new object[]
                {
                    id,
                    offset,
                    global,
                },
            }, DataChannel.Speech, steamId);
        }

        public void PlaySFX(string id, Vector3? position = null, float pitch = 1.0f, ulong? steamId = null)
        {
            server.SendPacket(new("actor_action")
            {
                ["action"] = "_play_sfx",
                ["actor_id"] = (long)server.ServerId,
                ["params"] = new object[]
                {
                    id,
                    position.HasValue ? position.Value : null,
                    pitch,
                },
            }, DataChannel.Speech, steamId);
        }

        public void MovePlayer(ulong steamId, Vector3 position, Vector3? rotation = null)
        {
            if (server.TryGetPlayer(steamId, out var player) && player != null && player.ActorId.HasValue)
            {
                player.Position = position;

                if (rotation.HasValue)
                {
                    player.Rotation = rotation.Value;
                }

                server.SendActorUpdate(player.ActorId.Value, player);
            }
        }

        public void SendLetter(ulong toSteamId, string header, string body, string closing, string[]? items = null)
        {
            // NOTE: Typo in "received" is in WebFishing not here
            server.SendPacket(new("letter_recieved")
            {
                ["to"] = toSteamId.ToString(),
                ["data"] = new Dictionary<string, object>()
                {
                    ["letter_id"] = new Random().Next(),
                    ["to"] = toSteamId.ToString(),
                    ["from"] = server.ServerId.ToString(),
                    ["header"] = header,
                    ["body"] = body,
                    ["closing"] = closing,
                    ["items"] = items ?? [],
                },
            }, DataChannel.GameState, toSteamId);
        }

        public void SetPlayerCosmetics(ulong steamId, Cosmetics cosmetics)
        {
            if (server.TryGetPlayer(steamId, out var player) && player != null && player.ActorId.HasValue)
            {
                player.Cosmetics = cosmetics;
                var cosmeticsPkt = new Dictionary<string, object>()
                {
                    ["title"] = cosmetics.Title,
                    ["eye"] = cosmetics.Eye,
                    ["nose"] = cosmetics.Nose,
                    ["mouth"] = cosmetics.Mouth,
                    ["undershirt"] = cosmetics.Undershirt,
                    ["overshirt"] = cosmetics.Overshirt,
                    ["legs"] = cosmetics.Legs,
                    ["hat"] = cosmetics.Hat,
                    ["species"] = cosmetics.Species,
                    ["accessory"] = cosmetics.Accessory,
                    ["pattern"] = cosmetics.Pattern,
                    ["primary_color"] = cosmetics.PrimaryColor,
                    ["secondary_color"] = cosmetics.SecondaryColor,
                    ["tail"] = cosmetics.Tail,
                };

                if (!string.IsNullOrWhiteSpace(cosmetics.Bobber))
                {
                    cosmeticsPkt.Add("bobber", cosmetics.Bobber);
                }

                server.SendPacket(new("actor_action")
                {
                    ["action"] = "_update_cosmetics",
                    ["actor_id"] = player.ActorId.Value,
                    ["params"] = new object[] { cosmeticsPkt, },
                }, DataChannel.ActorAction);
            }
        }

        public void SetPlayerHeldItem(ulong steamId, HeldItem? item = null)
        {
            if (server.TryGetPlayer(steamId, out var player) && player != null && player.ActorId.HasValue)
            {
                player.HeldItem = item;
                var heldItemPkt = new Dictionary<string, object>();
                if (item != null)
                {
                    heldItemPkt.Add("id", item.Id);
                    heldItemPkt.Add("size", item.Size);
                    heldItemPkt.Add("quality", (int)item.Quality);
                }

                server.SendPacket(new("actor_action")
                {
                    ["action"] = "_update_held_item",
                    ["actor_id"] = player.ActorId.Value,
                    ["params"] = new object[] { heldItemPkt, },
                }, DataChannel.ActorAction);
            }
        }
    }
}
