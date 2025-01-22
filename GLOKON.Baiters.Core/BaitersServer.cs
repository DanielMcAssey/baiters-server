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
using GLOKON.Baiters.Core.Models.Chat;
using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Core.Converters.Json;
using System.Text.Json;
using GLOKON.Baiters.Core.Exceptions;

namespace GLOKON.Baiters.Core
{
    public abstract class BaitersServer
    {
        protected readonly int dataChannelCount = Enum.GetNames(typeof(DataChannel)).Length;
        private readonly string _bansFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bans.json");
        private readonly string _chalkCanvasesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chalk_canvases.json");
        private readonly WebFishingOptions options;
        private readonly Random random = new();
        private readonly ConcurrentBag<ChatLog> _chatLogs = [];
        private readonly ConcurrentDictionary<ulong, Player> _players = new();
        private readonly ConcurrentDictionary<ulong, PlayerBan> _playerBans = new();
        private readonly ConcurrentDictionary<long, Actor> _actors = new();
        private readonly ConcurrentDictionary<long, ChalkCanvas> _chalkCanvases = new();

        private Lobby? _lobby;
        private ulong? _serverSteamId;

        public IEnumerable<KeyValuePair<long, Actor>> Actors => _actors;
        public IEnumerable<KeyValuePair<long, ChalkCanvas>> ChalkCanvases => _chalkCanvases;
        public IEnumerable<KeyValuePair<ulong, Player>> Players => _players;
        public IEnumerable<KeyValuePair<ulong, PlayerBan>> PlayerBans => _playerBans;
        public IReadOnlyCollection<ChatLog> ChatLogs => _chatLogs;
        public int PlayerCount => _players.Count + 1;
        public int NpcActorCount => _actors.Where((kv) => kv.Value.Type != ActorType.Player).Count();

        public string LobbyCode { get; private set; } = GenerateLobbyCode();

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
        /// Called when a new lobby message is received
        /// </summary>
        public event Action<ulong, string>? OnLobbyChatMessage;

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

        public BaitersServer(IOptions<WebFishingOptions> _options)
        {
            options = _options.Value;

            if (options.SaveChalkCanvases && File.Exists(_chalkCanvasesFilePath))
            {
                try
                {
                    _chalkCanvases = JsonSerializer.Deserialize<ConcurrentDictionary<long, ChalkCanvas>>(File.ReadAllText(_chalkCanvasesFilePath), JsonOptions.Default) ?? _chalkCanvases;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load chalk canvases file");
                }
            }

            if (File.Exists(_bansFilePath))
            {
                try
                {
                    _playerBans = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, PlayerBan>>(File.ReadAllText(_bansFilePath), JsonOptions.Default) ?? _playerBans;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load bans file");
                }
            }
        }

        public virtual void Setup()
        {
            Log.Information("Setting up server...");

            try
            {
                SteamClient.Init(options.AppId);
                SteamNetworkingUtils.InitRelayNetworkAccess();
            }
            catch (Exception ex)
            {
                throw new ServerSetupFailed("Failed to initialize SteamClient", ex);
            }

            Log.Information("Server setup");
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            LobbyCode = options.CustomLobbyCode ?? GenerateLobbyCode();
            _lobby = await SetupLobbyAsync(LobbyCode);
            var ticksPerSecond = 1000 / options.Modifiers.TicksPerSecond;
            IList<long> actorsToRemove = [];

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_lobby.HasValue)
                {
                    throw new LobbyGoneException("The lobby no longer exists, or the server is shutting down, you should never see this error");
                }

                if (!_lobby.Value.IsOwnedBy(ServerId))
                {
                    throw new LostLobbyHost(string.Format("You are no longer the host, stopping the server, host changed to: {0}", _lobby.Value.Owner.Id));
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
            Log.Information("Stopping server...");

            try
            {
                SendPacket(new("server_close"), DataChannel.GameState);
                SteamMatchmaking.OnChatMessage -= SteamMatchmaking_OnChatMessage;
                SteamMatchmaking.OnLobbyMemberDisconnected -= SteamMatchmaking_OnLobbyMemberDisconnected;
                SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
                if (_lobby.HasValue)
                {
                    _lobby.Value.Leave();
                    _lobby = null;
                }
                SteamClient.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to stop server gracefully");
            }

            // TODO: For Canvases and bans should we do periodic saves, just incase the server crashes?
            if (options.SaveChalkCanvases)
            {
                try
                {
                    Log.Information("Saving chalk canvases...");
                    File.WriteAllText(_chalkCanvasesFilePath, JsonSerializer.Serialize(_chalkCanvases, JsonOptions.Default));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to save chalk canvases");
                }
            }

            try
            {
                Log.Information("Saving bans...");
                File.WriteAllText(_bansFilePath, JsonSerializer.Serialize(_playerBans, JsonOptions.Default));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save bans");
            }

            Log.Information("Server stopped");
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
            if (steamId == ServerId)
            {
                return; // Lets not kick ourselves
            }

            SendPacket(new("peer_was_kicked")
            {
                ["user_id"] = (long)steamId,
            }, DataChannel.GameState);
            SendPacket(new("client_was_kicked"), DataChannel.GameState, steamId);

            if (_players.TryGetValue(steamId, out var player) && player != null)
            {
                SendMessage($"Kicked {player.FisherName}", MessageColour.Error, steamId);
            }

            LeavePlayer(steamId, DisconnectReason.Kicked);
        }

        public void BanPlayer(ulong steamId, string? reason = null)
        {
            if (steamId == ServerId)
            {
                return; // Lets not ban ourselves
            }

            SendPacket(new("peer_was_banned")
            {
                ["user_id"] = (long)steamId,
            }, DataChannel.GameState);
            SendPacket(new("client_was_banned"), DataChannel.GameState, steamId);

            string playerName = "Unknown";
            if (TryGetPlayer(steamId, out var player) && player != null)
            {
                playerName = player.FisherName;
            }

            _playerBans.TryAdd(steamId, new()
            {
                CreatedAt = DateTime.Now,
                PlayerName = playerName,
                Reason = reason,
            });

            LeavePlayer(steamId, DisconnectReason.Banned);
        }

        public void UnbanPlayer(ulong steamId)
        {
            _playerBans.TryRemove(steamId, out _);
        }

        public bool TryGetChalkCanvas(long canvasId, out ChalkCanvas? chalkCanvas)
        {
            return _chalkCanvases.TryGetValue(canvasId, out chalkCanvas);
        }

        public void AddChalkCanvas(long canvasId, ChalkCanvas chalkCanvas)
        {
            _chalkCanvases.TryAdd(canvasId, chalkCanvas);
        }

        public void RemoveChalkCanvas(long canvasId)
        {
            if (_chalkCanvases.TryRemove(canvasId, out var chalkCanvas) && chalkCanvas != null)
            {
                // Blank canvas by overriding the colour to -1 (Eraser)
                SendCanvas(canvasId, chalkCanvas.Cells.Values.ToList(), overrideColour: -1);
            }
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

        public void SetActorZone(long actorId, string zone, ulong? steamId = null)
        {
            if (TryGetActor(actorId, out var actor) && actor != null)
            {
                actor.Zone = zone;
                SendPacket(new("actor_action")
                {
                    ["actor_id"] = actorId,
                    ["action"] = "_set_zone",
                    ["params"] = new List<object>()
                    {
                        zone,
                        actor.ZoneOwnerId,
                    }.ToArray(),
                }, DataChannel.ActorAction, steamId);
            }
        }

        public void SetActorReady(long actorId, ulong? steamId = null)
        {
            SendPacket(new("actor_action")
            {
                ["actor_id"] = actorId,
                ["action"] = "_ready",
                ["params"] = Array.Empty<object>(),
            }, DataChannel.ActorAction, steamId);
        }

        public void SendSystemMessage(string message, string color = MessageColour.Default, ulong? steamId = null)
        {
            SendPacket(new("message")
            {
                // Need to format it like this, if not username wont appear and color wont either
                ["message"] = string.Format("%u: {0}", message),
                ["color"] = color,
                ["local"] = false,
                ["position"] = Vector3.Zero,
                ["zone"] = "main_zone",
                ["zone_owner"] = 1,
            }, DataChannel.GameState, steamId);
        }

        public void SendMessage(string message, string color = MessageColour.Default, ulong? steamId = null)
        {
            SendSystemMessage(message, color, steamId);

            if (!steamId.HasValue || steamId.Value == ServerId)
            {
                _chatLogs.Add(new()
                {
                    SentAt = DateTime.Now,
                    SenderId = ServerId,
                    SenderName = "Server",
                    SendToId = steamId,
                    Message = message,
                    Colour = color,
                    IsLocal = false,
                    Position = Vector3.Zero,
                    Zone = "main_zone",
                    ZoneOwner = 1,
                });
            }
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

        public bool SendLobbyChatMessage(string message)
        {
            if (_lobby.HasValue)
            {
                return _lobby.Value.SendChatString(message);
            }

            return false;
        }

        public bool SendLobbyChatMessage(byte[] message)
        {
            if (_lobby.HasValue)
            {
                return _lobby.Value.SendChatBytes(message);
            }

            return false;
        }

        public bool IsAdmin(ulong steamId)
        {
            return options.Admins.Contains(steamId);
        }

        internal void JoinPlayerLobby(ulong steamId, string playerName)
        {
            if (!_lobby.HasValue)
            {
                Log.Warning("Lobby is not ready to accept players, please wait before trying to join a player");
                return;
            }

            if (_players.ContainsKey(steamId))
            {
                SendWebLobbyPacket(steamId);
                return;
            }

            if (!CanSteamIdJoin(steamId))
            {
                _lobby.Value.SendChatString($"$weblobby_request_denied_deny-{steamId}");
                return;
            }

            if (PlayerCount >= options.MaxPlayers)
            {
                _lobby.Value.SendChatString($"$weblobby_request_denied_full-{steamId}");
                return;
            }

            Log.Debug("Joining new player [{steamId}] {playerName}", steamId, playerName);

            if (SendLobbyChatMessage($"$weblobby_request_accepted-{steamId}")) {
                _players.TryAdd(steamId, new Player(steamId, playerName, IsAdmin(steamId)));

                SendPacket(new("user_joined_weblobby")
                {
                    ["user_id"] = (long)steamId,
                }, DataChannel.GameState);
                SendWebLobbyPacket();
            }
            else
            {
                Log.Error("Failed to send lobby message, cannot accept player join request for [{0}] {1}", steamId, playerName);
            }
        }

        internal virtual void JoinPlayer(ulong steamId, long actorId, Player player)
        {
            AddActor(actorId, player);
            player.ActorId = actorId;
            Log.Information("[{0}] {1} joined the server", steamId, player.FisherName);
            SendWebLobbyPacket(steamId);
            UpdatePlayerCount();
            OnPlayerJoined?.Invoke(steamId);
        }

        internal virtual void LeavePlayer(ulong steamId, DisconnectReason reason = DisconnectReason.NormalLeave)
        {
            if (_players.Remove(steamId, out var player) && player != null)
            {
                switch (reason)
                {
                    case DisconnectReason.Kicked:
                        Log.Information("[{0}] {1} was kicked from the server", steamId, player.FisherName);
                        break;
                    case DisconnectReason.Banned:
                        Log.Information("[{0}] {1} was banned from the server", steamId, player.FisherName);
                        break;
                    case DisconnectReason.NormalLeave:
                    default:
                        Log.Information("[{0}] {1} left the server", steamId, player.FisherName);
                        break;
                };

                SendPacket(new("user_left_weblobby")
                {
                    ["user_id"] = (long)steamId,
                    ["reason"] = (int)reason,
                }, DataChannel.GameState);

                IList<long> actorsToRemove = [];
                foreach (var actor in _actors.Where(actor => actor.Value.OwnerId == steamId))
                {
                    actorsToRemove.Add(actor.Key);
                }

                if (player.ActorId.HasValue)
                {
                    actorsToRemove.Add(player.ActorId.Value);
                }

                foreach (var actorId in actorsToRemove)
                {
                    RemoveActor(actorId);
                }

                UpdatePlayerCount();

                OnPlayerLeft?.Invoke(steamId);
            }
        }

        internal void SendActor(long actorId, Actor actor, ulong? steamId = null)
        {
            SendPacket(new("instance_actor")
            {
                ["params"] = new Dictionary<string, object>
                {
                    ["actor_type"] = actor.Type,
                    ["at"] = actor.Position,
                    ["rot"] = actor.Rotation,
                    ["zone"] = actor.Zone,
                    ["zone_owner"] = actor.ZoneOwnerId,
                    ["actor_id"] = actorId,
                    ["creator_id"] = (long)ServerId,
                },
            }, DataChannel.GameState, steamId);
        }

        internal void SendActorUpdate(long actorId, Actor actor)
        {
            SendPacket(new("actor_update")
            {
                ["actor_id"] = actorId,
                ["pos"] = actor.Position,
                ["rot"] = actor.Rotation,
            }, channel: DataChannel.ActorUpdate);
        }

        internal void SendCanvas(long canvasId, IList<ChalkCanvasPoint> points, ulong? steamId = null, int? overrideColour = null)
        {
            SendPacket(new("chalk_packet")
            {
                ["canvas_id"] = canvasId,
                ["data"] = points.Select((point) => new object[] { point.Position, Convert.ToInt64(overrideColour ?? point.Colour) }).ToArray(),
            }, DataChannel.Chalk, steamId);
        }

        internal void OnPlayerChat(ulong sender, ChatLog chatLog)
        {
            OnChatMessage?.Invoke(sender, chatLog.Message);
            Log.ForContext("Scope", "Chat").Information("[{0}] {1}<{2}>: {3}", sender, chatLog.SenderName, chatLog.Zone, chatLog.Message);
            _chatLogs.Add(chatLog);
        }

        protected abstract void ReceivePackets();

        protected abstract void SendPacketTo(byte[] data, DataChannel channel);

        protected abstract void SendPacketTo(byte[] data, DataChannel channel, ulong steamId);

        protected bool CanSteamIdJoin(ulong steamId)
        {
            bool canJoin = !_playerBans.ContainsKey(steamId);

            if (canJoin)
            {
                foreach (var plugin in PluginLoader.Plugins)
                {
                    if (!plugin.CanPlayerJoin(steamId))
                    {
                        canJoin = false;
                        break;
                    }
                }
            }

            return canJoin;
        }

        protected void HandleNetworkPacket(ulong sender, byte[] data, DataChannel channel)
        {
            var parsedPacket = Packet.Parse(data);
            Log.Verbose("Received packet {0} on channel {1} from {2}", parsedPacket.Type, channel, sender);
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

            UpdatePlayerCount();
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

            OnLobbyChatMessage?.Invoke(from.Id, message);
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
            if (_lobby.HasValue)
            {
                _lobby.Value.SetData("count", PlayerCount.ToString());
            }

            Console.Title = string.Format("[{0}] {1} - {2} - {3}/{4}", options.JoinType, options.ServerName, LobbyCode, PlayerCount, options.MaxPlayers);
        }

        private static string GenerateLobbyCode()
        {
            return new(Enumerable.Range(0, 5).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[new Random().Next(36)]).ToArray());
        }
    }
}
