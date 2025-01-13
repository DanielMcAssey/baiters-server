using GLOKON.Baiters.Core.Enums.Networking;
using GLOKON.Baiters.Core.Models.Networking;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class LetterReceivedHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet packet)
        {
            // Auto decline letter as server cant receive them
            server.SendPacket(new("letter_was_denied"), DataChannel.GameState, sender);
        }
    }
}
