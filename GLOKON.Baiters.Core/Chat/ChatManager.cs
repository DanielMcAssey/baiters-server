using GLOKON.Baiters.Core.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace GLOKON.Baiters.Core.Chat
{
    public class ChatManager
    {
        private readonly WebFishingOptions options;
        private readonly ConcurrentDictionary<string, ChatCommand> _commands = new();

        public ChatManager(IOptions<WebFishingOptions> _options, BaitersServer server)
        {
            options = _options.Value;
            server.OnChatMessage += Server_OnChatMessage;

            ListenFor("help", "Show all the commands and their help text", (sender, commandParams) =>
            {
                if (!server.IsAdmin(sender))
                {
                    return;
                }

                server.SendMessage("-- Help --", "0f0f0f", sender);

                foreach (var chatCommand in _commands)
                {
                    server.SendMessage(string.Format("- {0}: {1}", chatCommand.Key, chatCommand.Value.HelpText), "0f0f0f", sender);
                }

                server.SendMessage("----", "0f0f0f", sender);
            });
        }

        public void ListenFor(string command, string helpText, Action<ulong, string[]> onCommand)
        {
            _commands.TryAdd(command, new() { HelpText = helpText, OnCommand = onCommand });
        }

        public void StopListening(string command)
        {
            _commands.TryRemove(command, out _);
        }

        private void Server_OnChatMessage(ulong sender,  string message)
        {
            if (message.Length <= 0 || !message.StartsWith(options.CommandPrefix))
            {
                return;
            }

            string[] commandParts = message.TrimStart(options.CommandPrefix[0]).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (_commands.TryGetValue(commandParts[0].ToLowerInvariant(), out var command) && command != null)
            {
                if (commandParts.Length > 1)
                {
                    // Skip the command name
                    command.OnCommand(sender, commandParts.Skip(1).ToArray());
                }
                else
                {
                    command.OnCommand(sender, []);
                }
            }
        }
    }
}
