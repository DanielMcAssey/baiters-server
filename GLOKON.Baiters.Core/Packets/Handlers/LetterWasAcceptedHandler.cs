using GLOKON.Baiters.Core.Models.Networking;
using Serilog;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal class LetterWasAcceptedHandler(BaitersServer server) : IPacketHandler
    {
        public void Handle(ulong sender, Packet packet)
        {
            Log.Information("Letter sent to {0} was accepted", sender);
        }
    }
}
