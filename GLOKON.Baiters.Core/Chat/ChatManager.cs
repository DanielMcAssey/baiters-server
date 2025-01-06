using GLOKON.Baiters.Core.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace GLOKON.Baiters.Core.Chat
{
    public class ChatManager(IOptions<WebFishingOptions> _options)
    {
        private readonly WebFishingOptions options = _options.Value;
        private readonly ConcurrentDictionary<string, ChatCommand> _commands = new();

        public IEnumerable<KeyValuePair<string, ChatCommand>> Commands => _commands;

        public void ListenFor(string command, string helpText, Action<ulong, string[]> onCommand)
        {
            _commands.TryAdd(command, new() { HelpText = helpText, OnCommand = onCommand });
        }

        public void OnChatMessage(ulong sender,  string message)
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
