using System;
using System.Collections.Generic;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var players = new List<Player> {new Player(i: int.Parse(args[0]), j: int.Parse(args[0])) };
            var team = new Team(players);
            team.MoveAllPlayers();
            Console.ReadKey();
        }
    }
}
