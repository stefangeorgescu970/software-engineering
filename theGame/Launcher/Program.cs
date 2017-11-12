using Board;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Agent;

namespace Launcher
{
	class Program
	{
		static Application _app;
		static void Main()
		{
			Console.WriteLine("Number of players:");
			int numberOfPlayers = Convert.ToInt32(Console.ReadLine());

			Console.WriteLine("Goal area width:");
			int goalAreaWidth = Convert.ToInt32(Console.ReadLine());

			Console.WriteLine("Goal area height:");
			int goalAreaHeight = Convert.ToInt32(Console.ReadLine());

			Console.WriteLine("Board width:");
			int boardWidth = Convert.ToInt32(Console.ReadLine());

			Console.WriteLine("Board height:");
			int boardHeight = Convert.ToInt32(Console.ReadLine());

			Console.WriteLine("Press \"Enter\" to start client, \"Esc\" to close it" );
			var appthread = new Thread(new ThreadStart(() =>
			{
				_app = new Application()
				{
					ShutdownMode = ShutdownMode.OnExplicitShutdown
				};
				
				_app.Run();
			}));
			appthread.SetApartmentState(ApartmentState.STA);
			appthread.Start();

			while (true)
			{
				var key = Console.ReadKey().Key;
				// Press 1 to create a window
				if (key == ConsoleKey.Enter)
				{
					Console.WriteLine("Launching client");
					// Use of dispatcher necessary as this is a cross-thread operation
					DispatchToApp(() => new MainWindow(numberOfPlayers,goalAreaWidth, goalAreaHeight, boardWidth, boardHeight).Show());
				    GeneratePlayers(numberOfPlayers, goalAreaHeight);
				}
				// Press 2 to exit
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
	                    Arguments = String.Concat($"{numberOfPlayers}", $"{goalAreaHeight}"),
	                    FileName = @"..\..\..\Agent\bin\Debug\Agent.exe",
                        CreateNoWindow = true,
	                    UseShellExecute = true
	                }
	            };
	            p.Start();
	        }
	    }


	    static void DispatchToApp(Action action)
		{
			_app.Dispatcher.Invoke(action);
		}
	}
}
