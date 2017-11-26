using System;
using System.Collections.Generic;

namespace Agent
{
    class Agent
    {
        /// <summary>
        /// Creates numberof players required by the user.
        /// </summary>
        /// <param name="args">Input parameters</param>
        static void Main(string[] args)
        {
            var players = new List<Player>
            {
                args != null && args.Length > 0
                ? new Player(i: int.Parse(args[0]), j: int.Parse(args[0]))
                : new Player(1, 1)
            };


            var team = new Team(players);
                team.MoveAllPlayers();

            Console.ReadKey();
        }
    }
}
