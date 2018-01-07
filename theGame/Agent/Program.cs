using Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using Board;

namespace Agent
{
    class Program
    {
       
        static void Main(string[] args)
        {
            Tuple<int, int> location = new Tuple<int, int>(1, 1);

            var player = new Player(location);

            Console.ReadLine();
        }
    }
}
