using Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Board;

namespace Agent
{
    class Program
    {

        static void Main(string[] args)
        {

            GameBoard gameBoard = new GameBoard(10, 10, 2); //TODO change the Gameboard
            Tuple<int, int> location = new Tuple<int, int>(1, 1);

            var player = new Player(location, gameBoard);
            
            Console.ReadKey();
        }
    }
}
