using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Converters.Json;
using GLOKON.Baiters.Core.Plugins;
using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace GLOKON.Baiters.Plugins.BanManager
{
    public class BanManagerPlugin(GameManager gm) : BaitersPlugin(
        gm,
        "Ban Manager",
        "A plugin to handle players bans",
        "Daniel McAssey <dan@glokon.me>",
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0")
    {
        private readonly string _bansFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bans.json");
        private ConcurrentDictionary<ulong, PlayerBan> _playerBans = new();

        public override void OnInit()
        {
            base.OnInit();

            if (File.Exists(_bansFilePath))
            {
                try
                {
                    _playerBans = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, PlayerBan>>(File.ReadAllText(_bansFilePath), JsonOptions.Default) ?? _playerBans;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load ban file");
                }
            }

            GM.Chat.ListenFor("ban", "Ban a player by passing their SteamID with an optional reason", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendMessage("Invalid ban command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                string? reason = null;
                if (commandParams.Length > 1)
                {
                    reason = string.Join(' ', commandParams.Skip(1).ToArray());
                }

                string playerName = "Unknown";
                if (GM.Server.TryGetPlayer(steamId, out var player) && player != null)
                {
                    playerName = player.FisherName;
                }

                GM.Server.BanPlayer(steamId);
                _playerBans.TryAdd(steamId, new() { FisherName = playerName, Reason = reason });
            });
            GM.Chat.ListenFor("ban.list", "List all players that are currently banned", (sender, commandParams) =>
            {
                GM.Server.SendMessage("-- Ban List --", MessageColour.Information, sender);

                foreach (var playerBan in _playerBans)
                {
                    GM.Server.SendMessage(string.Format("[{0}] {1}: {2}", playerBan.Key, playerBan.Value.FisherName, playerBan.Value.Reason ?? "(No Reason Given)"), MessageColour.Information, sender);
                }
            });
            GM.Chat.ListenFor("unban", "Un-Ban a player by passing their SteamID", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendMessage("Invalid unban command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                _playerBans.TryRemove(steamId, out _);
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            GM.Chat.StopListening("ban");
            GM.Chat.StopListening("ban.list");
            GM.Chat.StopListening("unban");
            File.WriteAllText(_bansFilePath, JsonSerializer.Serialize(_playerBans, JsonOptions.Default));
        }

        public override bool CanPlayerJoin(ulong steamId)
        {
            return !_playerBans.ContainsKey(steamId);
        }
    }
}
