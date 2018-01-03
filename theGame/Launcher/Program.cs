using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Board;

namespace Launcher
{
    internal class Program
    {
        private static Application _app;

        private static void Main()
        {
            int numberOfPlayers = 10, goalAreaHeight = 10, boardWidth = 10, boardHeight = 10;

            //Console.WriteLine("Number of players:");

            //while (!int.TryParse(Console.ReadLine(), out numberOfPlayers))
            //    Console.WriteLine("\t Please enter an integer");

            //Console.WriteLine("Board width:");

            //while (!int.TryParse(Console.ReadLine(), out boardWidth))
            //    Console.WriteLine("\t Please enter an integer");

            //Console.WriteLine("Board height:");

            //while (!int.TryParse(Console.ReadLine(), out boardHeight))
            //    Console.WriteLine("\t Please enter an integer");

            //Console.WriteLine("Goal area height:");

            //while (!int.TryParse(Console.ReadLine(), out goalAreaHeight))
            //    Console.WriteLine("\t Please enter an integer");

            //if (goalAreaHeight > boardHeight / 2)
            //{
            //    Console.WriteLine(
            //        " \tThe height of goal area has to be smaller than half of the board height. \n \t Please enter valid height");
            //    while (!int.TryParse(Console.ReadLine(), out goalAreaHeight))
            //        Console.WriteLine("\t Please enter an integer");
            //}


            Console.WriteLine("Press \"Enter\" to start client, \"Esc\" to close it");

            var appthread = new Thread(() =>
            {
                _app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };

                _app.Run();
            });
            appthread.SetApartmentState(ApartmentState.STA);
            appthread.Start();

            while (true)
            {
                var key = Console.ReadKey().Key;
                // Press Enter to create a window
                if (key == ConsoleKey.Enter)
                {
                    Console.WriteLine("Launching client");
                    // Use of dispatcher necessary as this is a cross-thread operation
                    DispatchToApp(() =>
                        new MainWindow(numberOfPlayers, goalAreaHeight, boardWidth, boardHeight).Show());
                    GenerateTeamLeader(numberOfPlayers, goalAreaHeight);
                }
                // Press Esc to exit
                if (key == ConsoleKey.Escape)
                {
                    DispatchToApp(() => _app.Shutdown());
                    Console.WriteLine("Client closed");
                    break;
                }
            }

            Console.ReadKey();
        }

        private static void GenerateTeamLeader(int numberOfPlayers, int goalAreaHeight)
        {
            var p = new Process
            {
                StartInfo =
                {
                    Arguments =  $"0" + " " + $"{numberOfPlayers - 1}" + " " + $"{goalAreaHeight}",
                    FileName = @"..\..\..\Agent\bin\Debug\Agent.exe",
                    CreateNoWindow = true,
                    UseShellExecute = true
                }
            };
            p.Start();
        }

        private static void DispatchToApp(Action action)
        {
            _app.Dispatcher.Invoke(action);
        }
    }
}