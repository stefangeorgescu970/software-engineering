using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Board;
using Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Launcher
{
    /// <summary>
    /// it has board fields
    /// every game master has its own board and players..
    /// game master spawn pieces
    /// player can ask game maseter about the board
    /// player get the correctness of placing pieces in goal area from game master
    /// </summary>
    public class GameMaster : Client.Client
    {
       
        private List<int> PlayersList = new List<int>();


        public GameMaster(int maxNoOfPlayers)
        {
            RegisterToServerAndGetId(ClientType.GameMaster, maxNoOfPlayers); 
        }
        public override void HandleReceivePacket(Packet receivedPacket)
        {
            switch (receivedPacket.RequestType)
            {
                case RequestType.Register:
                    SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
                    Console.WriteLine("id is set for game master : " + Id);
                    break;

                case RequestType.Send:
                    break;
                case RequestType.ConnectToGame:
                    var newPlayer = (int)receivedPacket.Arguments["NewPlayerId"];
                    PlayersList.Add(newPlayer);
                    var sendTeamLeaderId = new Packet(GetId(), newPlayer, RequestType.Send);
                    sendTeamLeaderId.AddArgument("TeamLeaderId", PlayersList[0]);
                    SendPacket(sendTeamLeaderId);
                    break;
                case RequestType.CheckMove:
                    JObject firstCoordinate = receivedPacket.Arguments.Values.First();
                    JObject secondCoordinate = receivedPacket.Arguments.Values.Last();
                    int frist = 0;
                    Int32.TryParse(((JValue)firstCoordinate.First.Last).Value.ToString(), out frist);
                    int destId = receivedPacket.Arguments[ServerConstants.ArgumentNames.Id];
                    Packet response = new Packet(Id, destId, RequestType.Send);
                    // return the status of the given cell
                    //response.AddArgument(ServerConstants.ArgumentNames.Move, new Tuple<int, int>(firstCoordinate.));
                    SendPacket(response);
                    break;

                default:
                    Console.WriteLine("Game Master received packet of unknown type, do nothing");
                    break;
            }

            //var value = receivedPacket.Arguments[ServerConstants.ArgumentNames.CheckMove];
            //if (value != null)
            //{
            //    Tuple<int, int> idx = value as Tuple<int, int>;
            //    int destId = receivedPacket.Arguments[ServerConstants.ArgumentNames.Id];
            //    Packet response = new Packet(Id, destId, RequestType.Send);
            //    // return the status of the given cell
            //    response.AddArgument(ServerConstants.ArgumentNames.Move, new Tuple<bool, bool>(board.IsOccupied(idx.Item1, idx.Item2), board.IsPiece(idx.Item1, idx.Item2)));
            //    SendPacket(response);
            //}
            //TODO - handle something received from another entit
        }

    }
    internal class Program
    {
        private static Application _app;
        [STAThread]
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

            // create the master
            GameMaster master = new GameMaster(numberOfPlayers);
            // wait for master id to be assigned
            while (master.Id == -1) ;
            Console.WriteLine("master id " + master.Id + " is ready");
            Packet toSend = new Packet(master.Id, master.Id, RequestType.Send);
            // toSend.AddArgument(ServerConstants.ArgumentNames.SenderType, ClientType.GameMaster);
            master.SendPacket(toSend);
            master.Board = master.CreateBoard(new GameBoard(boardWidth, boardHeight, goalAreaHeight));
            

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
                            {
                                _app.MainWindow = new MainWindow(numberOfPlayers, goalAreaHeight, boardWidth,
                                    boardHeight);
                                _app.MainWindow.Show();
                            }
                        );
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
                    
                }
                // Press Esc to exit
                if (key == ConsoleKey.Escape)
                {
                    DispatchToApp(() => _app.Shutdown());
                    Console.WriteLine("Client closed");
                    break;
                }
            }

            //_app.Dispatcher.Invoke(() =>
            //{
            //    Image myImage = new Image();
            //    BitmapImage bi = new BitmapImage();
            //    bi.BeginInit();
            //    bi.UriSource = new Uri("Cat-icon.PNG", UriKind.Relative);
            //    bi.EndInit();
            //    myImage.Stretch = Stretch.Fill;
            //    myImage.Source = bi;

            //    Grid.SetColumn(myImage,2);
            //    Grid.SetRow(myImage,2);
            //    ((Grid) _app.MainWindow.Content).Children.Add(myImage);
            //});

            Console.ReadKey();
        }


        private static void DispatchToApp(Action action)
        {
            _app.Dispatcher.Invoke(action);
        }
    }
}
                   