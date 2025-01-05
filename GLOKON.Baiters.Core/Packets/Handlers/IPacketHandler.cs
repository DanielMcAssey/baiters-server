using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLOKON.Baiters.Core.Packets.Handlers
{
    internal interface IPacketHandler
    {
        void Handle(SteamId sender, Dictionary<string, object> data);
    }
}
