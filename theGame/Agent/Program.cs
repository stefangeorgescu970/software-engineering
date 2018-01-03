using Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {

        static void Main(string[] args)
        {
            int teamLeaderId = int.Parse(args[0]);

            Board gameBoard = new Board(10, 10, 2);
            Tuple<int, int> location = new Tuple<int, int>(1, 1);

            if (teamLeaderId == 0)
            {
                //assume 0 as teamleaderId argument indicates team Leader case

                int numberOfPlayers = int.Parse(args[1]);
                var player = new TeamLeader(location, numberOfPlayers, gameBoard);


            }
            else
            {
                var player = new Player(location, gameBoard, teamLeaderId);
            }
            //players[0].SendPacket(new Packet(players[0].Id, 0, RequestType.Send));
            
            Console.ReadKey();
        }
    }
}
