using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Core.Enums.Configuration;
using GLOKON.Baiters.Core.Models.Actor;
using GLOKON.Baiters.Core.Packets;
using GLOKON.Baiters.Core.Plugins;
using GLOKON.Baiters.GodotInterop.Models;
using Microsoft.Extensions.Options;
using Serilog;
using Steamworks;
using Steamworks.Data;
using System.Collections.Concurrent;
using System.Text;

namespace GLOKON.Baiters.Core
{
    public abstract class BaitersServer(PacketManager packetManager, IOptions<WebFishingOptions> options): ISocketManager
    {
        private readonly ConcurrentDictionary<SteamId, Player> _players = new();
        private readonly ConcurrentDictionary<long, Actor> _actors = new();

        protected static bool CanSteamIdJoin(SteamId steamId)
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

        protected SocketManager? _socketManager;
        private Lobby _lobby;

        public WebFishingOptions Options { get; } = options.Value;

        public IEnumerable<KeyValuePair<long, Actor>> Actors { get { return _actors; } }

        public int NpcActorCount { get { return _actors.Where((kv) => kv.Value.Type != "player").Count(); } }

        public virtual void Setup()
        {
            packetManager.Setup(this);

            try
            {
                Dispatch.OnDebugCallback = (type, str, server) =>
                {
                    Log.Debug($"[Callback {type} {(server ? "server" : "client")}]");
                    Log.Debug(str);
                    Log.Debug($"");
                };
                Dispatch.OnException = (e) =>
                {
                    Log.Error(e.Message);

                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        Log.Error(e.StackTrace);
                    }
                };
                SteamClient.Init(Options.AppId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize SteamClient");
                throw;
            }
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Options.PluginsEnabled)
            {
                PluginLoader.LoadPlugins(this);
            }

            _socketManager = SteamNetworkingSockets.CreateRelaySocket(0, this);
            _lobby = await SetupLobbyAsync(new(Enumerable.Range(0, 5).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[new Random().Next(36)]).ToArray()));
            await _lobby.Join();

            IList<long> actorsToRemove = [];

            while (!cancellationToken.IsCancellationRequested)
            {
                SteamClient.RunCallbacks();
                ReceivePackets();

                foreach (var actor in _actors)
                {
                    actor.Value.OnUpdate();

                    if (actor.Value.IsDespawned)
                    {
                        actorsToRemove.Add(actor.Key);
                    }
                }

                foreach (var actorIdToRemove in actorsToRemove)
                {
                    _actors.TryRemove(actorIdToRemove, out _);
                }

                actorsToRemove.Clear();

                foreach (var plugin in PluginLoader.Plugins)
                {
                    plugin.OnUpdate();
                }

                await Task.Delay(1000 / Options.Modifiers.TicksPerSecond, CancellationToken.None);
            }
        }

        public virtual void Stop()
        {
            SteamMatchmaking.OnChatMessage -= SteamMatchmaking_OnChatMessage;
            SteamMatchmaking.OnLobbyMemberDisconnected -= SteamMatchmaking_OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
            PluginLoader.UnloadPlugins();
            _lobby.Leave();

            if (_socketManager != null)
            {
                try
                {
                    _socketManager.Close();
                }
                catch
                {
                    // Do nothing
                }
            }

            SteamClient.Shutdown();
        }

        public virtual void OnConnecting(Connection connection, ConnectionInfo data)
        {
            Log.Debug($"{data.Identity} is connecting");
        }

        public virtual void OnConnected(Connection connection, ConnectionInfo data)
        {
            Log.Information($"{data.Identity} has joined the game");
        }

        public virtual void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            Log.Information($"{data.Identity} is out of here");
        }

        public virtual void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            Log.Debug($"We got a message from {identity}!");
        }

        public IEnumerable<KeyValuePair<long, Actor>> GetActorsByType(string type)
        {
            return _actors.Where((kv) => kv.Value.Type == type);
        }

        public bool TryGetPlayer(SteamId steamId, out Player? player)
        {
            return _players.TryGetValue(steamId, out player);
        }

        public void KickPlayer(SteamId steamId)
        {
            Dictionary<string, object> leftPkt = new()
            {
                ["type"] = "peer_was_kicked",
                ["user_id"] = (long)steamId.Value
            };
            SendPacket(leftPkt);

            Dictionary<string, object> kickPacket = new()
            {
                ["type"] = "client_was_kicked"
            };
            SendPacket(kickPacket, steamId);
        }

        public bool TryGetActor(long actorId, out Actor? actor)
        {
            return _actors.TryGetValue(actorId, out actor);
        }

        public void AddActor(long actorId, Actor actor)
        {
            _actors.TryAdd(actorId, actor);
        }

        public virtual void JoinPlayer(long actorId, Player player)
        {
            AddActor(actorId, player);

            Log.Information($"[{player.SteamId}] {player.FisherName} joined the server");

            SendWebLobbyPacket(player.SteamId);
            UpdatePlayerCount();

            foreach (var plugin in PluginLoader.Plugins)
            {
                plugin.OnPlayerJoin(player);
            }
        }

        public virtual void LeavePlayer(SteamId steamId)
        {
            // TODO: Do we need to remove actor here?

            if (_players.Remove(steamId, out var player) && player != null)
            {
                Log.Information($"[{player.SteamId}] {player.FisherName} left the server");
                SendWebLobbyLeftPacket(steamId);
                UpdatePlayerCount();

                foreach (var plugin in PluginLoader.Plugins)
                {
                    plugin.OnPlayerLeft(player);
                }
            }
        }

        public void RemoveActor(long actorId)
        {
            Dictionary<string, object> removePkt = new()
            {
                ["type"] = "actor_action",
                ["actor_id"] = actorId,
                ["action"] = "queue_free"
            };

            Dictionary<int, object> prams = [];
            removePkt["params"] = prams;

            SendPacket(removePkt);

            _actors.TryRemove(actorId, out _);
        }

        public void SendPacket(Dictionary<string, object> packet, SteamId? steamId = null)
        {
            byte[] data = PacketIO.WritePacket(packet);

            if (steamId.HasValue)
            {
                SendPacketTo(steamId.Value, data);
            }
            else
            {
                foreach (var player in _players)
                {
                    SendPacketTo(player.Key, data);
                }
            }
        }

        public void SendMessage(string message, string color = "ffffff", SteamId? steamId = null)
        {
            Dictionary<string, object> messagePkt = new()
            {
                ["type"] = "message",
                ["message"] = message,
                ["color"] = color,
                ["local"] = false,
                ["position"] = Vector3.Zero,
                ["zone"] = "main_zone",
                ["zone_owner"] = 1
            };

            SendPacket(messagePkt, steamId);
        }

        public void SendLetter(SteamId to, SteamId from, string header, string body, string closing, string user)
        {
            Dictionary<string, object> letterPkt = new()
            {
                ["type"] = "letter_received",
                ["to"] = to.ToString(),
                ["data"] = new Dictionary<string, object>()
                {
                    ["to"] = to.ToString(),
                    ["from"] = from.ToString(),
                    ["header"] = header,
                    ["body"] = body,
                    ["closing"] = closing,
                    ["user"] = user,
                    ["letter_id"] = new Random().Next(),
                    ["items"] = new Dictionary<int, object>(),
                },
            };

            SendPacket(letterPkt, to);
        }

        public void OnPlayerChat(SteamId sender, string message)
        {
            if (_players.TryGetValue(sender, out var player) && player != null)
            {
                Log.Information($"[{sender}] {player.FisherName}: {message}");

                foreach (var plugin in PluginLoader.Plugins)
                {
                    plugin.OnChatMessage(player, message);
                }
            }
            else
            {
                Log.Information($"[UNKNOWN] {sender}: {message}");
            }
        }

        public bool IsAdmin(SteamId steamId)
        {
            return Options.Admins.Contains(steamId);
        }

        protected abstract void ReceivePackets();

        protected abstract void SendPacketTo(SteamId steamId, byte[] data);

        protected void HandleNetworkPacket(SteamId from, byte[] data)
        {
            _players.TryGetValue(from, out var player);

            var packet = PacketIO.ReadPacket(data);

            packetManager.Handle((string)packet["type"], from, packet);

            if (player != null)
            {
                foreach (var plugin in PluginLoader.Plugins)
                {
                    plugin.OnPlayerPacket(player, packet);
                }
            }
        }

        protected void SendWebLobbyPacket(SteamId? steamId = null)
        {
            Dictionary<int, object> usersInServer = new()
            {
                [0] = (long)SteamClient.SteamId.Value
            };

            int userIndex = 1; // Start at 1 as server user is 0

            foreach (var player in _players)
            {
                usersInServer[userIndex] = (long)player.Key.Value;
                userIndex++;
            }

            Dictionary<string, object> usersInServerPkt = new()
            {
                ["type"] = "receive_weblobby",
                ["weblobby"] = usersInServer
            };
            SendPacket(usersInServerPkt, steamId);
        }

        protected void SendWebLobbyLeftPacket(SteamId steamId)
        {
            Dictionary<string, object> playerLeftPkt = new()
            {
                ["type"] = "user_left_weblobby",
                ["user_id"] = (long)steamId.Value,
                ["reason"] = 0
            };
            SendPacket(playerLeftPkt);
        }

        private async Task<Lobby> SetupLobbyAsync(string lobbyCode)
        {
            Log.Information("Creating lobby...");

            var maybeNewLobby = await SteamMatchmaking.CreateLobbyAsync(Options.MaxPlayers);
            if (!maybeNewLobby.HasValue)
            {
                throw new Exception("Failed to create Steam Lobby");
            }

            SteamMatchmaking.OnChatMessage += SteamMatchmaking_OnChatMessage;
            SteamMatchmaking.OnLobbyMemberDisconnected += SteamMatchmaking_OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;

            Lobby newLobby = maybeNewLobby.Value;
            newLobby.SetJoinable(true);

            switch (Options.JoinType)
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
            newLobby.SetData("version", Options.AppVersion);
            newLobby.SetData("code", lobbyCode);
            newLobby.SetData("type", "0");
            newLobby.SetData("public", Options.JoinType == JoinType.InviteOnly ? "false" : "true");
            newLobby.SetData("request", "false"); // TODO: Add support for join request
            newLobby.SetData("cap", Options.MaxPlayers.ToString());
            newLobby.SetData("count", "1");
            newLobby.SetData("lobby_name", Options.ServerName);
            newLobby.SetData("name", Options.ServerName);
            newLobby.SetData("server_browser_value", "0");

            string[] validTags = ["talkative", "quiet", "grinding", "chill", "silly", "hardcore", "mature", "modded"];

            foreach (var tag in validTags)
            {
                newLobby.SetData($"tag_{tag}", Options.Tags.Contains(tag) ? "1" : "0");
            }

            ulong epoch = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            newLobby.SetData("timestamp", epoch.ToString());

            Log.Information("Lobby created, code: {0}", lobbyCode);

            return newLobby;
        }

        private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend from, string message)
        {
            if (message.Length <= 0) return;

            Log.Debug($"We got lobby chat message from {from.Id}, it says {message}!");

            if (string.Compare(message, "$weblobby_join_request", true) == 0)
            {
                if (_players.ContainsKey(from.Id))
                {
                    SendWebLobbyPacket(from.Id);
                    return;
                }

                if (!CanSteamIdJoin(from.Id))
                {
                    _lobby.SendChatBytes(Encoding.UTF8.GetBytes($"$weblobby_request_denied_deny-{from.Id}"));
                    return;
                }

                _lobby.SendChatBytes(Encoding.UTF8.GetBytes($"$weblobby_request_accepted-{from.Id}"));
                _players.TryAdd(from.Id, new Player(from.Id, from.Name ?? "Unknown", IsAdmin(from.Id)));

                Dictionary<string, object> joinedPkt = new()
                {
                    ["type"] = "user_joined_weblobby",
                    ["user_id"] = (long)from.Id.Value,
                };
                SendPacket(joinedPkt);
                SendWebLobbyPacket();
            }
        }

        private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend from)
        {
            LeavePlayer(from.Id);
        }

        private void SteamMatchmaking_OnLobbyMemberDisconnected(Lobby lobby, Friend from)
        {
            LeavePlayer(from.Id);
        }

        private void UpdatePlayerCount()
        {
            _lobby.SetData("count", (_players.Count + 1).ToString());
        }
    }
}
