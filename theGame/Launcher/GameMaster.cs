using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using Server;

namespace Launcher
{
    public class GameMaster : Client.Client
    {
        //ClientType _clientType = ClientType.GameMaster;
        public GameMaster()
        {
            RegisterToServerAndGetId(ClientType.GameMaster);
        }

        public override void HandleReceivePacket(Packet receivedPacket)
        {
            //Console.WriteLine(receivedPacket.RequestType);
        }
    }
}
