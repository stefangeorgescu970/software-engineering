using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace Agent
{
    public class GameMaster : Client.Client
    {
        public GameMaster()
        {
            TryConnect(5);
            RegisterToServerAndGetId(ClientType.GameMaster);
        }

        public override void HandleReceivePacket(Packet receivedPacket)
        {
            // Handle packets
        }
    }
}
