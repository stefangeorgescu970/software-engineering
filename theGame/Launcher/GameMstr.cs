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
using System.Windows.Threading;
using Board;
using Client;
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
        private List<Tuple<int,Tuple<int,int>>> PlayerLocations = new List<Tuple<int, Tuple<int, int>>>();
        private List<Tuple<int, Tuple<int, int>>> itemsLocations = new List<Tuple<int, Tuple<int, int>>>();

        public List<Tuple<int, Tuple<int, int>>> ItemsLocations { get => itemsLocations; set => itemsLocations = value; }

        public GameMaster(int maxNoOfPlayers, int numberOfItems, int goalAreaHeight, int boardWidth, int boardHeight)
        {
            RegisterToServerAndGetId(ClientType.GameMaster, maxNoOfPlayers);
        }
        
        public override void HandleReceivePacket(Packet receivedPacket)
        {
            switch (receivedPacket.RequestType)
            {
                case RequestType.Register:
                    SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
                    Console.WriteLine("ID is set for game master : " + Id);
                    break;
                case RequestType.Send:
                    if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.GameBoardSize))
                    {
                        int destId = receivedPacket.SenderId;
                        Packet boardPacket = new Packet(Id, destId, RequestType.Send);
                        boardPacket.AddArgument(ServerConstants.ArgumentNames.GameBoardSize, Board.BoardDim);
                        SendPacket(boardPacket);
                    }
                    else if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.CheckMove))
                    {
                        JObject testLocation = receivedPacket.Arguments.Values.First();
                        Int32.TryParse(((JValue)testLocation.First.Last).Value.ToString(), out var testX);
                        Int32.TryParse(((JValue)testLocation.Last.Last).Value.ToString(), out var testY);

                        int destId = receivedPacket.SenderId;
                        Packet response = new Packet(Id, destId, RequestType.Send);
                        if (Board.IsOccupied(testX, testY))
                            response.AddArgument(ServerConstants.ArgumentNames.CheckMove, null);
                        else
                        {
                            //ugly, may be changed
                            if (Board.ContainsItem(testX, testY))
                            {
                                var item = ItemsLocations.Find(i => i.Item2.Item1 == testX && i.Item2.Item2 == testY);
                                response.AddArgument(ServerConstants.ArgumentNames.SteppedOnItem, item.Item1);
                            }


                            Tuple<int, int> currentLocation = PlayerLocations.First(i => i.Item1 == destId).Item2;
                            response.AddArgument(ServerConstants.ArgumentNames.CheckMove, new Tuple<int, int>(testX, testY));
                            Board.Board[testX, testY].Content = FieldContent.Player;
                            Board.Board[currentLocation.Item1, currentLocation.Item2].Content = FieldContent.Empty;
                            PlayerLocations.Remove(PlayerLocations.First(i => i.Item1 == destId));
                            PlayerLocations.Add(new Tuple<int, Tuple<int, int>>(destId, new Tuple<int, int>(testX, testY)));

                            GameMstr.GameWindowApplication.Dispatcher.Invoke(() =>
                            {
                                Grid.SetColumn(((MainWindow)GameMstr.GameWindowApplication.MainWindow).PlayerIcons[destId], testX);
                                Grid.SetRow(((MainWindow)GameMstr.GameWindowApplication.MainWindow).PlayerIcons[destId], testY);
                                
                                ((MainWindow)GameMstr.GameWindowApplication.MainWindow).UpdateLayout();
                            });
                        }
                        SendPacket(response);
                    }
                    else if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.ManhattanDistance))
                    {
                        //by default return distance between sender and teamLeader
                        int destId = receivedPacket.SenderId;
                        var plLoc = PlayerLocations.Find(player => player.Item1 == destId);
                        var distances = new List<Double>();

                        //if player is in goal area on field not neighbouring any tasks area fields
                        if(plLoc.Item2.Item2 < Board.GoalAreaHeight || plLoc.Item2.Item2 >Board.Height - Board.GoalAreaHeight) return;

                        foreach (var item in ItemsLocations)
                        {
                            //if item is in gola area: skip
                            if (item.Item2.Item2 < Board.GoalAreaHeight || item.Item2.Item2 > Board.Height - Board.GoalAreaHeight) continue;
                            distances.Add(Math.Sqrt(Math.Pow((plLoc.Item2.Item1 - item.Item2.Item1), 2) + Math.Pow((plLoc.Item2.Item2 - item.Item2.Item2), 2)));
                        }

                        Packet response = new Packet(Id, destId, RequestType.Send);
                        var min = distances.Count > 0 ? distances.Min() : -1.0;
                        response.AddArgument(ServerConstants.ArgumentNames.ManhattanDistance, min);
                        SendPacket(response);
                    }
                    else if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.SteppedOnItem))
                    {
                        var itemId = (int)receivedPacket.Arguments[ServerConstants.ArgumentNames.SteppedOnItem];
                        ItemsLocations.RemoveAll(item => item.Item1 == itemId);
                        Console.WriteLine($"player {receivedPacket.SenderId} picked an item {itemId}");
                    }

                    break;
                case RequestType.ConnectToGame:
                    var newPlayer = (int)receivedPacket.Arguments["NewPlayerId"];
                    PlayersList.Add(newPlayer);
                    var sendTeamLeaderId = new Packet(GetId(), newPlayer, RequestType.Send);
                    sendTeamLeaderId.AddArgument("TeamLeaderId", PlayersList[0]);
                    SendPacket(sendTeamLeaderId);
                    Board.Board[1, 1].Content = FieldContent.Player;
                    Board.Board[1, 1].PlayerID = newPlayer;
                    PlayerLocations.Add(new Tuple<int, Tuple<int, int>>(newPlayer,new Tuple<int, int>(1,1)));

                    GameMstr.GameWindowApplication.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                     {
                         Image myImage = new Image();
                         BitmapImage bi = new BitmapImage();
                         bi.BeginInit();
                         bi.UriSource = new Uri("Cat-icon.PNG", UriKind.Relative);
                         bi.EndInit();
                         myImage.Stretch = Stretch.Fill;
                         myImage.Source = bi;

                         Grid.SetColumn(myImage, 1);
                         Grid.SetRow(myImage, 1);
                         ((MainWindow)GameMstr.GameWindowApplication.MainWindow).PlayerIcons.Add(newPlayer, myImage);
                         ((Grid)GameMstr.GameWindowApplication.MainWindow.Content).Children.Add(myImage);
                         ((MainWindow)GameMstr.GameWindowApplication.MainWindow).UpdateLayout();
                     }));
                    Console.WriteLine("poop");
                    break;
                default:
                    Console.WriteLine("Game Master received packet of unknown type, do nothing");
                    break;
            }
            //TODO - handle something received from another entit
        }
    }
    public class GameMstr
    {
        public static Application GameWindowApplication;
        [STAThread]
        private static void Main()
        {
            int numberOfPlayers, goalAreaHeight, boardWidth, boardHeight, numberOfItems;

            Console.WriteLine("Number of players:");

            while (!int.TryParse(Console.ReadLine(), out numberOfPlayers) || !(numberOfPlayers > 0))
                Console.WriteLine("\t Please enter an integer");

            Console.WriteLine("Number of items:");

            while (!int.TryParse(Console.ReadLine(), out numberOfItems) || !(numberOfItems > 0))
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

            var appthread = new Thread(() =>
            {
                GameWindowApplication = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };

                GameWindowApplication.Run();
            });
            appthread.SetApartmentState(ApartmentState.STA);
            appthread.Start();

            // create the master
            GameMaster master = new GameMaster(numberOfPlayers, numberOfItems, goalAreaHeight, boardWidth, boardHeight);
            // wait for master id to be assigned
            while (master.Id == -1) ;
            Console.WriteLine("master id " + master.Id + " is ready");
            Packet toSend = new Packet(master.Id, master.Id, RequestType.Send);

            master.SendPacket(toSend);
            master.Board = new GameBoard(boardWidth, boardHeight, goalAreaHeight);

            Console.WriteLine("Press \"Enter\" to start client, \"Esc\" to close it");



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
                                new MainWindow(numberOfPlayers, goalAreaHeight, boardWidth, boardHeight).Show();
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

                    InitializeItems(numberOfItems, goalAreaHeight, boardWidth, boardHeight, master);
                    break;


                }
                // Press Esc to exit
                if (key == ConsoleKey.Escape)
                {
                    DispatchToApp(() => GameWindowApplication.Shutdown());
                    Console.WriteLine("Client closed");
                    break;
                }
            }
           
            Console.ReadKey();
        }

        public static void InitializeItems(int numberOfItems, int goalAreaHeight, int boardWidth, int boardHeight, GameMaster master)
        {
            Thread.Sleep(1000);
            int j = 0;
            Random r = new Random();
            Tuple<int, int> loc = null;
            for (int i = 0; i < numberOfItems; i++)
            {
                do
                {
                    loc = new Tuple<int, int>(r.Next(0, boardWidth), r.Next(goalAreaHeight, boardHeight));
                }
                while (master.ItemsLocations.Find(item => item.Item2 == loc) != null);

                master.ItemsLocations.Add(new Tuple<int, Tuple<int, int>>(-i, loc));

                master.Board.Board[loc.Item1, loc.Item2].Content = FieldContent.Item;
                master.Board.Board[loc.Item1, loc.Item2].PlayerID = -i;

                GameWindowApplication.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                {
                    var location = master.ItemsLocations[j++];
                    Image myImage = new Image();
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("GoldItem.png", UriKind.Relative);
                    bi.EndInit();
                    myImage.Stretch = Stretch.Fill;
                    myImage.Source = bi;

                    Grid.SetColumn(myImage, location.Item2.Item1);
                    Grid.SetRow(myImage, location.Item2.Item2);
                    ((MainWindow)GameMstr.GameWindowApplication.MainWindow).ItemIcons.Add(location.Item1, myImage);
                    ((Grid)GameMstr.GameWindowApplication.MainWindow.Content).Children.Add(myImage);
                    ((MainWindow)GameMstr.GameWindowApplication.MainWindow).UpdateLayout();
                }));
            }
        }

        private static void DispatchToApp(Action action)
        {
            GameWindowApplication.Dispatcher.Invoke(action);
        }
    }
}
