using System;
using System.Collections.Generic;
using Server;
namespace Agent
{
    
    class Program
    {

        static void Main(string[] args)
        {
            var players = new List<Player>
            {
                args != null && args.Length > 0
                ? new Player(i: int.Parse(args[0]), j: int.Parse(args[0]))
                : new Player(1, 1)
            };

            while (players[0].Id == -1) ;

            players[0].SendPacket(new Packet(players[0].Id, 0, RequestType.Send));
            //var team = new Team(players);
            //    team.MoveAllPlayers();

            Console.ReadKey();
        }
    }
}
