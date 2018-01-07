using System;
using System.IO;
using System.Runtime.CompilerServices;
using Server;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Board;
using Newtonsoft.Json;
using System.Windows.Media;
using Client;
using Newtonsoft.Json.Linq;

namespace Agent
{
    public class Team
    {
        public Team(List<int> playersIds, int numberOfPlayers)
        {
            PlayersIds = playersIds;
            NumberOfPlayers = numberOfPlayers;
        }

        public List<int> PlayersIds { get; set; }
        public int NumberOfPlayers { get; set; }
    }
    public class Player : Client.Client
    {
        public Team MyTeam;
        private Tuple<int, int> location;
        private int teamLeaderId;
        private int gameMasterId;

        public Player(Tuple<int, int> location)
        {
            this.location = location;
            
            RegisterToServerAndGetId(ClientType.Agent);
            Console.WriteLine($"Player initialized");
            while (Id == -1 || gameMasterId == 0) ;
            GetBoardDim();
            while (Board == null) ;

            System.Threading.Thread.Sleep(5000);
            Move();
        }
        public void GetBoardDim()
        {
            Packet boardRequest = new Packet(Id, gameMasterId,RequestType.Send);
            boardRequest.AddArgument(ServerConstants.ArgumentNames.GameBoardSize, null);
            SendPacket(boardRequest);
        }

        /// <summary>
        /// return a pair of the new position, 
        /// the method should test every possible position whether it is ocupied by other player or not, by asking the server about it
        /// </summary>
        /// <returns></returns>

        public void Move()   
        {
            int []dx = {-1, 0, 1, 0};
            int []dy = {0, 1, 0, -1};
            Random r = new Random();
            
            int i = 1;
            while (i < 2)
            {
                int idx = r.Next(4);
                int newX = location.Item1 + dx[idx];
                int newY = location.Item2 + dy[idx];
                if (newX >= 0 && newX < 10 && newY >= 0 && newY < 10/* && (newX, newY) are not ocupied */ ) // 10 should subtituted by the board size
                {
                    Tuple<int, int> testLocation = new Tuple<int, int>(newX, newY);

                    Packet toSend = new Packet(Id, gameMasterId, RequestType.Send); //send to GM, plug in the id of GM

                    toSend.AddArgument(ServerConstants.ArgumentNames.CheckMove, testLocation);
                    SendPacket(toSend);
                    i++;
                }
            }
        }
        
        public override void HandleReceivePacket(Packet receivedPacket)
        {
            switch (receivedPacket.RequestType)
            {
                case RequestType.Register:
                    SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
                    Console.WriteLine($"Player ID set to {Id}");
                    var connetToGame = new Packet(Id, -1, RequestType.ConnectToGame);
                    connetToGame.AddArgument(ServerConstants.ArgumentNames.SenderType, ClientType.Agent);
                    SendPacket(connetToGame);
                    break;
                case RequestType.Send:
                    if (receivedPacket.Arguments.ContainsKey("TeamLeaderId") && receivedPacket.SenderId == gameMasterId)
                    {
                        teamLeaderId = (int)receivedPacket.Arguments["TeamLeaderId"];
                        Console.WriteLine($"Player: {Id} received Team Leader's id: {teamLeaderId} ");
                    }
                    else if (receivedPacket.Arguments.ContainsKey("GameBoardSize") &&
                             receivedPacket.SenderId == gameMasterId)
                    {
                        var k = receivedPacket.Arguments[ServerConstants.ArgumentNames.GameBoardSize];
                        Board = new GameBoard((int) k.Width, (int) k.Height, (int) k.GoalAreaHeight);
                        Console.WriteLine($"game board of size {k.Width}x{k.Height}x{k.GoalAreaHeight} added to {GetId()}");
                    }
                    else if (receivedPacket.Arguments.ContainsKey("CheckMove") && receivedPacket.SenderId == gameMasterId)
                    {
                        JObject firstCoordinate = receivedPacket.Arguments.Values.First();
                        Int32.TryParse(((JValue)firstCoordinate.First.Last).Value.ToString(), out int frist);
                        Int32.TryParse(((JValue)firstCoordinate.Last.Last).Value.ToString(), out int second);
                        location = new Tuple<int, int>(frist,second);
                    }
                    break;
                case RequestType.ConnectToGame:
                    gameMasterId = (int)receivedPacket.Arguments[ServerConstants.ArgumentNames.GameMasterId];
                    Console.WriteLine($"Player: {Id} received Game Master's id: {gameMasterId} ");
                    break;
                default:
                    Console.WriteLine("Player received packet of unknown type, do nothing");
                    break;
            }
        }
    }
}
