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

            Board gameBoard = new Board(10, 10, 2);
            Tuple<int, int> location = new Tuple<int, int>(1, 1);

            var player = new Player(location, gameBoard);
            
            Console.ReadKey();
        }
    }
}
