using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Enums.Configuration;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Models.Networking;
using GLOKON.Baiters.Core.Plugins;
using System.Numerics;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;
using Steamworks.Data;
using System.Collections.Concurrent;
using GLOKON.Baiters.Core.Enums.Networking;

namespace GLOKON.Baiters.Core
{
    public abstract class BaitersServer(IOptions<WebFishingOptions> _options)
    {
        protected readonly int dataChannelCount = Enum.GetNames(typeof(DataChannel)).Length;
        private readonly WebFishingOptions options = _options.Value;
        private readonly Random random = new();
        private readonly ConcurrentDictionary<ulong, Player> _players = new();
        private readonly ConcurrentDictionary<long, Actor> _actors = new();

        protected static bool CanSteamIdJoin(ulong steamId)
        {
            bool canJoin = true;

            foreach (var plugin in PluginLoader.Plugins)
            {
                if (!plugin.CanPlayerJoin(steamId))
                {
                    canJoin = false;
                    break;
                }
            }

            return canJoin;
        }

        private Lobby _lobby;
        private ulong? _serverSteamId;

        public IEnumerable<KeyValuePair<long, Actor>> Actors => _actors;
        public IEnumerable<KeyValuePair<ulong, Player>> Players => _players;
        public int NpcActorCount => _actors.Where((kv) => kv.Value.Type != ActorType.Player).Count();

        /// <summary>
        /// Called every tick of the server
        /// </summary>
        public event Action? OnTick;

        /// <summary>
        /// Called when a chat message is received
        /// </summary>
        public event Action<ulong, string>? OnChatMessage;

        /// <summary>
        /// Called when a player has joined
        /// </summary>
        public event Action<ulong>? OnPlayerJoined;

        /// <summary>
        /// Called when a player leaves
        /// </summary>
        public event Action<ulong>? OnPlayerLeft;

        /// <summary>
        /// Called when a new packet is received
        /// </summary>
        public event Action<ulong, Packet>? OnPacket;

        /// <summary>
        /// Only available after the server Setup has been called
        /// </summary>
        public ulong ServerId
        {
            get
            {
                if (!_serverSteamId.HasValue)
                {
                    // Cache this, as call can be slow
                    _serverSteamId = SteamClient.SteamId.Value;
                }

                return _serverSteamId.Value;
            }
        }

        public virtual void Setup()
        {
            try
            {
                SteamClient.Init(options.AppId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize SteamClient");
                throw;
            }
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            _lobby = await SetupLobbyAsync(new(Enumerable.Range(0, 5).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[new Random().Next(36)]).ToArray()));
            var ticksPerSecond = 1000 / options.Modifiers.TicksPerSecond;
            IList<long> actorsToRemove = [];

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_lobby.IsOwnedBy(ServerId))
                {
                    Log.Error("You are no longer the host, stopping the server, host changed to: {0}", _lobby.Owner.Id);
                    Stop();
                    break;
                }

                ReceivePackets();

                foreach (var actor in _actors)
                {
                    actor.Value.OnUpdate();

                    if (actor.Value.IsDespawned)
                    {
                        actorsToRemove.Add(actor.Key);
                    }
                    else if (actor.Value.IsSyncRequired)
                    {
                        SendActorUpdate(actor.Key, actor.Value);
                    }
                }

                foreach (var actorIdToRemove in actorsToRemove)
                {
                    RemoveActor(actorIdToRemove);
                }

                actorsToRemove.Clear();

                OnTick?.Invoke();

                await Task.Delay(ticksPerSecond, CancellationToken.None);
            }
        }

        public virtual void Stop()
        {
            SendPacket(new("server_close"), DataChannel.GameState);
            SteamMatchmaking.OnChatMessage -= SteamMatchmaking_OnChatMessage;
            SteamMatchmaking.OnLobbyMemberDisconnected -= SteamMatchmaking_OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
            _lobby.Leave();
            SteamClient.Shutdown();
        }

        public IEnumerable<KeyValuePair<long, Actor>> GetActorsByType(string type)
        {
            return _actors.Where((kv) => kv.Value.Type == type);
        }

        public bool TryGetPlayer(ulong steamId, out Player? player)
        {
            return _players.TryGetValue(steamId, out player);
        }

        public void KickPlayer(ulong steamId)
        {
            SendPacket(new("peer_was_kicked")
            {
                ["user_id"] = (long)steamId,
            }, DataChannel.GameState);
            SendPacket(new("client_was_kicked"), DataChannel.GameState, steamId);

            if (_players.TryGetValue(steamId, out var player) && player != null)
            {
                SendMessage($"Kicked {player.FisherName}", steamId: steamId);
                SendMessage($"{player.FisherName} was kicked from the lobby!");
            }
        }

        public void BanPlayer(ulong steamId)
        {
            SendPacket(new("peer_was_banned")
            {
                ["user_id"] = (long)steamId,
            }, DataChannel.GameState);
            SendPacket(new("client_was_banned"), DataChannel.GameState, steamId);
        }

        public bool TryGetActor(long actorId, out Actor? actor)
        {
            return _actors.TryGetValue(actorId, out actor);
        }

        public void AddActor(long actorId, Actor actor)
        {
            _actors.TryAdd(actorId, actor);
        }

        public void RemoveActor(long actorId)
        {
            SendPacket(new("actor_action")
            {
                ["actor_id"] = actorId,
                ["action"] = "queue_free",
                ["params"] = Array.Empty<object>(),
            }, DataChannel.GameState);

            _actors.TryRemove(actorId, out _);
        }

        public void SpawnActor(Actor actor)
        {
            long actorId = random.NextInt64();
            AddActor(actorId, actor);
            SendActor(actorId, actor);
        }

        public void SendMessage(string message, string color = "ffffff", ulong? steamId = null)
        {
            SendPacket(new("message")
            {
                ["message"] = message,
                ["color"] = color,
                ["local"] = false,
                ["position"] = Vector3.Zero,
                ["zone"] = "main_zone",
                ["zone_owner"] = 1
            }, DataChannel.GameState, steamId);
        }

        public void SendLetter(ulong toSteamId, string header, string body, string closing, string user)
        {
            SendPacket(new("letter_received")
            {
                ["to"] = toSteamId.ToString(),
                ["data"] = new Dictionary<string, object>()
                {
                    ["to"] = toSteamId.ToString(),
                    ["from"] = ServerId.ToString(),
                    ["header"] = header,
                    ["body"] = body,
                    ["closing"] = closing,
                    ["user"] = user,
                    ["letter_id"] = new Random().Next(),
                    ["items"] = Array.Empty<object>(),
                },
            }, DataChannel.GameState, toSteamId);
        }

        public void SendPacket(Packet packet, DataChannel channel, ulong? steamId = null)
        {
            byte[] data = packet.ToBytes();

            if (steamId.HasValue)
            {
                Log.Verbose("Sending {0} packet on {1} to single player {2}", packet.Type, channel, steamId.Value);
                SendPacketTo(data, channel, steamId.Value);
            }
            else
            {
                Log.Verbose("Sending {0} packet on {1} to all players", packet.Type, channel);
                SendPacketTo(data, channel);
            }
        }

        public bool IsAdmin(ulong steamId)
        {
            return options.Admins.Contains(steamId);
        }

        internal void JoinPlayerLobby(ulong steamId, string playerName)
        {
            if (_players.ContainsKey(steamId))
            {
                SendWebLobbyPacket(steamId);
                return;
            }

            if (!CanSteamIdJoin(steamId))
            {
                _lobby.SendChatString($"$weblobby_request_denied_deny-{steamId}");
                return;
            }

            Log.Debug("Joining new player [{steamId}] {playerName}", steamId, playerName);
            _lobby.SendChatString($"$weblobby_request_accepted-{steamId}");
            _players.TryAdd(steamId, new Player(steamId, playerName, IsAdmin(steamId)));

            SendPacket(new("user_joined_weblobby")
            {
                ["user_id"] = (long)steamId,
            }, DataChannel.GameState);
            SendWebLobbyPacket();
        }

        internal virtual void JoinPlayer(ulong steamId, long actorId, Player player)
        {
            AddActor(actorId, player);
            player.SetActorId(actorId);
            Log.Information("[{0}] {1} joined the server", steamId, player.FisherName);
            SendWebLobbyPacket(steamId);
            UpdatePlayerCount();
            OnPlayerJoined?.Invoke(steamId);
        }

        internal virtual void LeavePlayer(ulong steamId)
        {
            if (_players.Remove(steamId, out var player) && player != null)
            {
                var fisherName = player.FisherName;
                Log.Information("[{steamId}] {fisherName} left the server", steamId, fisherName);

                SendPacket(new("user_left_weblobby")
                {
                    ["user_id"] = (long)steamId,
                    ["reason"] = 0
                }, DataChannel.GameState);

                UpdatePlayerCount();

                if (player.ActorId.HasValue)
                {
                    RemoveActor(player.ActorId.Value);
                }

                OnPlayerLeft?.Invoke(steamId);
            }
        }

        internal void SendActor(long actorId, Actor actor, ulong? steamId = null)
        {
            Dictionary<string, object> instanceActorParams = [];
            instanceActorParams["actor_type"] = actor.Type;

            if (actor is MovableActor movableActor)
            {
                instanceActorParams["at"] = movableActor.Position;
                instanceActorParams["rot"] = movableActor.Rotation;
            }
            else
            {
                instanceActorParams["at"] = Vector3.Zero;
                instanceActorParams["rot"] = Vector3.Zero;
            }

            instanceActorParams["zone"] = "main_zone";
            instanceActorParams["zone_owner"] = -1;
            instanceActorParams["actor_id"] = actorId;
            instanceActorParams["creator_id"] = (long)ServerId;

            SendPacket(new("instance_actor")
            {
                ["params"] = instanceActorParams,
            }, DataChannel.GameState, steamId);
        }

        internal void SendActorUpdate(long actorId, Actor actor)
        {
            var position = Vector3.Zero;
            var rotation = Vector3.Zero;
            if (actor is MovableActor movableActor)
            {
                position = movableActor.Position;
                rotation = movableActor.Rotation;
            }

            SendPacket(new("actor_update")
            {
                ["actor_id"] = actorId,
                ["pos"] = position,
                ["rot"] = rotation,
            }, channel: DataChannel.ActorUpdate);
        }

        internal void OnPlayerChat(ulong sender, string message)
        {
            string fisherName = "UNKNOWN";

            if (_players.TryGetValue(sender, out var player) && player != null)
            {
                fisherName = player.FisherName;
                OnChatMessage?.Invoke(sender, message);
            }

            Log.Information("[{sender}] {fisherName}: {message}", sender, fisherName, message);
        }

        protected abstract void ReceivePackets();

        protected abstract void SendPacketTo(byte[] data, DataChannel channel);

        protected abstract void SendPacketTo(byte[] data, DataChannel channel, ulong steamId);

        protected void HandleNetworkPacket(ulong sender, byte[] data, DataChannel channel)
        {
            var parsedPacket = Packet.Parse(data);
            Log.Debug("Received packet {0} on channel {1} from {2}", parsedPacket.Type, channel, sender);
            OnPacket?.Invoke(sender, parsedPacket);
        }

        protected void SendWebLobbyPacket(ulong? steamId = null)
        {
            List<long> usersInServer = [(long)ServerId];

            foreach (var player in _players)
            {
                usersInServer.Add((long)player.Key);
            }

            SendPacket(new("receive_weblobby")
            {
                ["weblobby"] = usersInServer.ToArray(),
            }, DataChannel.GameState, steamId);
        }

        private async Task<Lobby> SetupLobbyAsync(string lobbyCode)
        {
            Log.Information("Setting up game lobby...");

            var maybeNewLobby = await SteamMatchmaking.CreateLobbyAsync(options.MaxPlayers);
            if (!maybeNewLobby.HasValue)
            {
                throw new Exception("Failed to create Steam Lobby");
            }

            SteamMatchmaking.OnChatMessage += SteamMatchmaking_OnChatMessage;
            SteamMatchmaking.OnLobbyMemberDisconnected += SteamMatchmaking_OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;

            Lobby newLobby = maybeNewLobby.Value;
            newLobby.SetJoinable(true);

            switch (options.JoinType)
            {
                case JoinType.FriendsOnly:
                    newLobby.SetFriendsOnly();
                    break;
                case JoinType.Public:
                    newLobby.SetPublic();
                    break;
                case JoinType.InviteOnly:
                    newLobby.SetInvisible();
                    break;
            }

            newLobby.SetData("ref", "webfishing_gamelobby");
            newLobby.SetData("version", options.AppVersion);
            newLobby.SetData("code", lobbyCode);
            newLobby.SetData("type", "0");
            newLobby.SetData("public", options.JoinType == JoinType.InviteOnly ? "false" : "true");
            newLobby.SetData("request", "false"); // TODO: Add support for join request
            newLobby.SetData("cap", options.MaxPlayers.ToString());
            newLobby.SetData("count", "1");
            newLobby.SetData("lobby_name", options.ServerName);
            newLobby.SetData("name", options.ServerName);
            newLobby.SetData("server_browser_value", "0");
            newLobby.SetData("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            string[] validTags = ["talkative", "quiet", "grinding", "chill", "silly", "hardcore", "mature", "modded"];

            foreach (var tag in validTags)
            {
                newLobby.SetData($"tag_{tag}", options.Tags.Contains(tag) ? "1" : "0");
            }

            Log.Information("{0} {1} lobby created, max players {2}, invite code: {3}", options.ServerName, options.JoinType, options.MaxPlayers, lobbyCode);

            return newLobby;
        }

        private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend from, string message)
        {
            if (message.Length <= 0) return;

            Log.Debug("Lobby[{0}]: {1}", from.Id, message);

            if (string.Compare(message, "$weblobby_join_request", true) == 0)
            {
                JoinPlayerLobby(from.Id, from.Name ?? "Unknown");
            }
        }

        private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend from)
        {
            Log.Debug("Lobby[{0}]: Member Left", from.Id);
            LeavePlayer(from.Id);
        }

        private void SteamMatchmaking_OnLobbyMemberDisconnected(Lobby lobby, Friend from)
        {
            Log.Debug("Lobby[{0}]: Member Disconnected", from.Id);
            LeavePlayer(from.Id);
        }

        private void UpdatePlayerCount()
        {
            int totalPlayerCount = (_players.Count + 1);
            _lobby.SetData("count", totalPlayerCount.ToString());
            Console.Title = string.Format("[{0}] {1} - {2}/{3}", options.JoinType, options.ServerName, totalPlayerCount, options.MaxPlayers);
        }
    }
}
