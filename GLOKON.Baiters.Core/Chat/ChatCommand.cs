namespace GLOKON.Baiters.Core.Chat
{
    public class ChatCommand
    {
        public required string HelpText { get; set; }

        public required Action<ulong, string[]> OnCommand { get; set; }
    }
}
