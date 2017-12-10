using System;
using System.Data;
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
            int numberOfPlayers, goalAreaHeight, boardWidth, boardHeight;

            Console.WriteLine("Number of players:");

            while (!int.TryParse(Console.ReadLine(), out numberOfPlayers) || !(numberOfPlayers > 0) )
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Board width:");

            while (!int.TryParse(Console.ReadLine(), out boardWidth) || !(boardWidth > 0))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Board height:");

            while (!int.TryParse(Console.ReadLine(), out boardHeight) || !(boardHeight > 0))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Goal area height:");

            while (!int.TryParse(Console.ReadLine(), out goalAreaHeight) || !(goalAreaHeight > 0))
                Console.WriteLine("\t Please enter an integer");

            if (goalAreaHeight > boardHeight / 2)
            {
                Console.WriteLine(
                    " \tThe height of goal area has to be smaller than half of the board height. \n \t Please enter valid height");
                while (!int.TryParse(Console.ReadLine(), out goalAreaHeight))
                    Console.WriteLine("\t Please enter an integer");
            }


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
                    try
                    {
                        DispatchToApp(() =>
                            new MainWindow(numberOfPlayers, goalAreaHeight, boardWidth, boardHeight).Show());
                    }
                    catch (ArgumentException ex) when (ex.ParamName == "numberOfPlayers")
                    {
                        Console.WriteLine("Something wrong with number of players");
                        return;
                    }
                    catch (ArgumentException ex) when (ex.ParamName == "goalAreaH")
                    {
                        Console.WriteLine("Something wrong with height of the goal area");
                        return;
                    }
                    catch (ArgumentException ex) when (ex.ParamName == "boardW")
                    {
                        Console.WriteLine("Something wrong with board width");
                        return;
                    }
                    catch (ArgumentException ex) when (ex.ParamName == "boardH")
                    {
                        Console.WriteLine("Something wrong with board height");
                        return;
                    }
                   
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

        private static void DispatchToApp(Action action)
        {
            _app.Dispatcher.Invoke(action);
        }
    }
}