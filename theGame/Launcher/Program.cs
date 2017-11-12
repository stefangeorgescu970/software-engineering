#define DEFAULT_PARAMS

using Board;
using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
	class Program
	{
		static Application _app;
		static void Main()
		{

#if (DEFAULT_PARAMS)

            int numberOfPlayers = 2;
            int goalAreaWidth = 20;
            int goalAreaHeight = 10;
            int boardWidth = 40;
            int boardHeight = 40;

#else
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
#endif

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

            // Start GameMaster
            GameMaster gameMaster = new GameMaster();

			while (true)
			{
				var key = Console.ReadKey().Key;
				// Press 1 to create a window
				if (key == ConsoleKey.Enter)
				{
					Console.WriteLine("Launching client");
					// Use of dispatcher necessary as this is a cross-thread operation
					DispatchToApp(() => new MainWindow(numberOfPlayers,goalAreaWidth, goalAreaHeight, boardWidth, boardHeight).Show());
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

		static void DispatchToApp(Action action)
		{
			_app.Dispatcher.Invoke(action);
		}
	}
}
