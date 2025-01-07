using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Plugins;

namespace GLOKON.Baiters.Plugins.ChatCommand
{
    public class ChatCommandPlugin(GameManager gm) : BaitersPlugin(
        gm,
        "Chat Command",
        "A plugin to handle players chat commands",
        "Daniel McAssey <dan@glokon.me>",
        "1.0.0")
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

                GM.Server.SendMessage(string.Format("-- Players ({0}/{1}) --", GM.Server.Players.Count(), GM.Options.MaxPlayers), "0f0f0f", sender);

                foreach (var player in GM.Server.Players)
                {
                    GM.Server.SendMessage(string.Format("- [{0}] {1}", player.Key, player.Value.FisherName), "0f0f0f", sender);
                }

                GM.Server.SendMessage("----", "0f0f0f", sender);
            });

            GM.Chat.ListenFor("spawn", "Spawn a new element on the server", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendMessage("Invalid spawn command, not enough parameters", steamId: sender);
                    return;
                }

                string spawnType = commandParams[0].ToLower();
                if (GM.Spawner.Spawn(spawnType))
                {
                    GM.Server.SendMessage(string.Format("Spawned a {0}", spawnType), "0f0f0f", sender);
                }
                else
                {
                    GM.Server.SendMessage(string.Format("Cannot spawn a {0}", spawnType), "0f0f0f", sender);
                }
            });

            GM.Chat.ListenFor("spawn.list", "Spawn a new element on the server", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                GM.Server.SendMessage(string.Format("Spawnable: {0}", string.Join(", ", ActorSpawner.Spawnable)), "0f0f0f", sender);
            });

            GM.Chat.ListenFor("kick", "Kick a player by providing their SteamID", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendMessage("Invalid kick command, not enough parameters", steamId: sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendMessage("Invalid SteamID, could not be parsed", steamId: sender);
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
                    GM.Server.SendMessage("Invalid say command, not enough parameters", steamId: sender);
                    return;
                }

                bool didParseSteamId = ulong.TryParse(commandParams[0], out var steamId);
                if (!didParseSteamId)
                {
                    GM.Server.SendMessage("Invalid SteamID, could not be parsed", steamId: sender);
                    return;
                }

                GM.Server.SendMessage(string.Format("ADMIN: {0}", string.Join(" ", commandParams.Skip(1).ToArray())), "ff0000", steamId);
            });

            GM.Chat.ListenFor("say.all", "Send all players a message", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                if (commandParams.Length < 1)
                {
                    GM.Server.SendMessage("Invalid say.all command, not enough parameters", steamId: sender);
                    return;
                }

                GM.Server.SendMessage(string.Format("ADMIN: {0}", string.Join(" ", commandParams)), "ff0000");
            });

            GM.Chat.ListenFor("plugins", "Show all the plugins loaded", (sender, commandParams) =>
            {
                if (!GM.Server.IsAdmin(sender))
                {
                    return;
                }

                GM.Server.SendMessage("-- Plugins --", "0f0f0f", sender);

                foreach (var plugin in PluginLoader.Plugins)
                {
                    GM.Server.SendMessage(string.Format("- {0}:{1} by {2}", plugin.Name, plugin.Version, plugin.Author), "0f0f0f", sender);
                }

                GM.Server.SendMessage("----", "0f0f0f", sender);
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
            GM.Chat.StopListening("plugins");
        }
    }
}
