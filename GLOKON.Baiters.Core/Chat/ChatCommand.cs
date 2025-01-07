namespace GLOKON.Baiters.Core.Chat
{
    internal class ChatCommand
    {
        internal required string HelpText { get; set; }

        internal required Action<ulong, string[]> OnCommand { get; set; }
    }
}
