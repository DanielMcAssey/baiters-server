using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Plugins;
using System.Reflection;

namespace GLOKON.Baiters.Plugins.ChatCommand
{
    public class ChatCommandPlugin(GameManager gm) : BaitersPlugin(
        gm,
        "Chat Command",
        "A plugin to handle players chat commands",
        "Daniel McAssey <dan@glokon.me>",
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0")
    {
        public override void OnInit()
        {
            base.OnInit();

            GM.Chat.ListenFor("users.list", "Show all the users in the server", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                GM.Server.SendSystemMessage(string.Format("-- Players ({0}/{1}) --", GM.Server.PlayerCount, GM.Server.MaxPlayerCount), MessageColour.Information, sender);

                foreach (var player in GM.Server.Players)
                {
                    GM.Server.SendSystemMessage(string.Format("- [{0}] {1}", player.Key, player.Value.FisherName), MessageColour.Information, sender);
                }
            });

            GM.Chat.ListenFor("spawn", "Spawn a new element on the server", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendSystemMessage("Invalid spawn command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                string spawnType = commandParams[0].ToLower();
                if (GM.Spawner.Spawn(spawnType))
                {
                    GM.Server.SendSystemMessage(string.Format("Spawned a {0}", spawnType), MessageColour.Information, sender);
                }
                else
                {
                    GM.Server.SendSystemMessage(string.Format("Cannot spawn a {0}", spawnType), MessageColour.Information, sender);
                }
            });

            GM.Chat.ListenFor("spawn.list", "Spawn a new element on the server", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                GM.Server.SendSystemMessage(string.Format("Spawnable: {0}", string.Join(", ", ActorSpawner.Spawnable)), MessageColour.Information, sender);
            });

            GM.Chat.ListenFor("kick", "Kick a player by providing their SteamID", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendSystemMessage("Invalid kick command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendSystemMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                GM.Server.KickPlayer(steamId);
            });

            GM.Chat.ListenFor("say", "Send a player a message by providing their SteamID", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 2)
                {
                    GM.Server.SendSystemMessage("Invalid say command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendSystemMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                GM.Server.SendMessage(string.Join(" ", commandParams.Skip(1).ToArray()), MessageColour.Warning, steamId);
            });

            GM.Chat.ListenFor("say.all", "Send all players a message", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendSystemMessage("Invalid say.all command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                GM.Server.SendMessage(string.Join(" ", commandParams), MessageColour.Warning);
            });

            GM.Chat.ListenFor("ban", "Ban a player by passing their SteamID with an optional reason", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendSystemMessage("Invalid ban command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendSystemMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                string? reason = null;
                if (commandParams.Length > 1)
                {
                    reason = string.Join(' ', commandParams.Skip(1).ToArray());
                }

                GM.Server.BanPlayer(steamId, reason);
            });

            GM.Chat.ListenFor("ban.list", "List all players that are currently banned", (sender, commandParams) =>
            {
                GM.Server.SendSystemMessage("-- Ban List --", MessageColour.Information, sender);

                foreach (var playerBan in GM.Server.PlayerBans)
                {
                    GM.Server.SendSystemMessage(string.Format("[{0}] {1}: {2}", playerBan.Key, playerBan.Value.PlayerName, playerBan.Value.Reason ?? "(No Reason Given)"), MessageColour.Information, sender);
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
                    GM.Server.SendSystemMessage("Invalid unban command, not enough parameters", MessageColour.Error, sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendSystemMessage("Invalid SteamID, could not be parsed", MessageColour.Error, sender);
                    return;
                }

                GM.Server.UnbanPlayer(steamId);
            });

            GM.Chat.ListenFor("plugins", "Show all the plugins loaded", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                GM.Server.SendSystemMessage("-- Plugins --", MessageColour.Information, sender);

                foreach (var plugin in PluginLoader.Plugins)
                {
                    GM.Server.SendSystemMessage(string.Format("- {0}:{1} by {2}", plugin.Name, plugin.Version, plugin.Author), MessageColour.Information, sender);
                }
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            GM.Chat.StopListening("help");
            GM.Chat.StopListening("users.list");
            GM.Chat.StopListening("spawn");
            GM.Chat.StopListening("spawn.list");
            GM.Chat.StopListening("kick");
            GM.Chat.StopListening("say");
            GM.Chat.StopListening("say.all");
            GM.Chat.StopListening("ban");
            GM.Chat.StopListening("ban.list");
            GM.Chat.StopListening("unban");
            GM.Chat.StopListening("plugins");
        }
    }
}
