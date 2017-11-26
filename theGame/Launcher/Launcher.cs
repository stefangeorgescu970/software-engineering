using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Board;

namespace Launcher
{
    /// <summary>
    /// Launcher which starts players and generates the board. 
    /// </summary>
    internal class Launcher
    {
        /// <summary>
        /// MainWindow application. 
        /// </summary>
        private static Application _app;

        private static void Main()
        {
            GetIninitialParameters(out var numberOfPlayers, out var boardWidth, out var boardHeight, out var goalAreaHeight);

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
                    GeneratePlayers(numberOfPlayers, goalAreaHeight);
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

        /// <summary>
        /// Gets initial parameters from the user. 
        /// </summary>
        /// <param name="numberOfPlayers">Number of players per team.</param>
        /// <param name="boardWidth">Width of a board.</param>
        /// <param name="boardHeight">Height of a board.</param>
        /// <param name="goalAreaHeight">Height of a goal area.</param>
        private static void GetIninitialParameters(out int numberOfPlayers, out int boardWidth, out int boardHeight,
            out int goalAreaHeight)
        {
            Console.WriteLine("Number of players:");

            while (!int.TryParse(Console.ReadLine(), out numberOfPlayers))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Board width:");

            while (!int.TryParse(Console.ReadLine(), out boardWidth))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Board height:");

            while (!int.TryParse(Console.ReadLine(), out boardHeight))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Goal area height:");

            while (!int.TryParse(Console.ReadLine(), out goalAreaHeight))
                Console.WriteLine("\t Please enter an integer");

            if (goalAreaHeight > boardHeight / 2)
            {
                Console.WriteLine(
                    " \tThe height of goal area has to be smaller than half of the board height. \n \t Please enter valid height");
                while (!int.TryParse(Console.ReadLine(), out goalAreaHeight))
                    Console.WriteLine("\t Please enter an integer");
            }
        }

        /// <summary>
        /// Creates game players. 
        /// </summary>
        /// <param name="numberOfPlayers">Number of players.</param>
        /// <param name="goalAreaHeight">Height og a goal area.</param>
        private static void GeneratePlayers(int numberOfPlayers, int goalAreaHeight)
        {
            for (; numberOfPlayers > 0; numberOfPlayers--)
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        Arguments = string.Concat($"{numberOfPlayers}", $"{goalAreaHeight}"),
                        FileName = @"..\..\..\Agent\bin\Debug\Agent.exe",
                        CreateNoWindow = true,
                        UseShellExecute = true
                    }
                };
                p.Start();
            }
        }
        /// <summary>
        /// Dispatcher of a MainWindow.
        /// </summary>
        /// <param name="action"></param>
        private static void DispatchToApp(Action action)
        {
            _app.Dispatcher.Invoke(action);
        }
    }
}