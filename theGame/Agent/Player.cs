using System;
using System.IO;
using System.Runtime.CompilerServices;
using Server;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

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
    public class Board
    {
        public Board(int width, int height, int goalAreaHeight)
        {
            Width = width;
            Height = height;
            GoalAreaHeight = goalAreaHeight;
        }

        private int Width { get; set; }
        private int Height { get; set; }
        private int GoalAreaHeight { get; set; }

    }
    public class TeamLeader : Client.Client
    {
        public Team MyTeam;
        private Tuple<int, int> location;
        private Board gameBoard;
        private int gameMasterId;

        public TeamLeader(int i, int j)
        {
            RegisterToServerAndGetId(ClientType.Agent);
            location = new Tuple<int, int>(i, j);
            MyTeam = null;
            Console.WriteLine($"TeamLeader initialized");
        }

        public TeamLeader(Tuple<int, int> locaton, int no_players, Board gameBoard)
        {
            MyTeam = new Team(new List<int>(), no_players);
            this.location = locaton;
            this.gameBoard = gameBoard;
            RegisterToServerAndGetId(ClientType.Agent);
            Console.WriteLine($"TeamLeader initialized");
        }


        /* return a pair of the new position
         * the method should test every possible position whether it is ocupied by other player or not, by asking the server about it
        */
        public Tuple<int, int> Move()
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            Random r = new Random();
            while (true)
            {
                int idx = r.Next(4);
                int newX = location.Item1 + dx[idx];
                int newY = location.Item2 + dy[idx];
                if (newX >= 0 && newX < 10 && newY >= 0 && newY < 10/* && (newX, newY) are not ocupied */ ) // 10 should subtituted by the board size
                {
                    location = new Tuple<int, int>(newX, newY);

                    return new Tuple<int, int>(newX, newY);
                }

            }
        }


        public override void HandleReceivePacket(Packet receivedPacket)
        {
            if (receivedPacket.RequestType == RequestType.Register)
            {
                SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
                Console.WriteLine($"TeamLeader id set to {Id}");

                for (var i = MyTeam.NumberOfPlayers; i > 0; i--)
                    CreateTeamMember(GetId());
                
                gameMasterId = int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.GameMasterId]);

                var askForBoard = new Packet(GetId(), gameMasterId, RequestType.Send);
                askForBoard.AddArgument(ServerConstants.ArgumentNames.GameBoardSize, "0");
                SendPacket(askForBoard);
            }
            else
            {
                if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.RegisterToTeamLeader))
                {
                    var k = receivedPacket.Arguments[ServerConstants.ArgumentNames.RegisterToTeamLeader];
                    int senderId = receivedPacket.SenderId;
                    MyTeam.PlayersIds.Add(senderId);
                    Console.WriteLine($"Player with id {senderId} added to team");
                }

                if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.GameBoardSize))
                {
                    var k = receivedPacket.Arguments[ServerConstants.ArgumentNames.GameBoardSize];
                    gameBoard = new Board((int)k.Width, (int)k.Height, (int)k.GoalAreaHeight);
                    Console.WriteLine($"game board of size {k.Width}x{k.Height}x{k.GoalAreaHeight} added to {GetId()}");
                }
            }


        }

        private static void CreateTeamMember(int teamLeaderId)
        {
            var p = new Process
            {
                StartInfo =
                    {
                        Arguments = $"{teamLeaderId}",
                        FileName = @".\Agent.exe",
                        CreateNoWindow = true,
                        UseShellExecute = true
                    }
            };
            p.Start();
            Console.WriteLine($"TeamLeader created team member");
        }
    }
    public class Player : Client.Client
    {
        Tuple<int, int> _position;
        public Team MyTeam;
        private Tuple<int, int> location;
        private Board gameBoard;
        private int teamLeaderId;
        private int gameMasterId;

        public Player(int i, int j)
        {
            RegisterToServerAndGetId(ClientType.Agent);
            _position = new Tuple<int, int>(i, j);
            MyTeam = null;
            Console.WriteLine($"Player initialized");
        }

        public Player(Tuple<int, int> location, Board gameBoard, int TeamLeaderId)
        {
            this.location = location;
            this.gameBoard = gameBoard;
            this.teamLeaderId = TeamLeaderId;
            TryConnect(5);
            RegisterToServerAndGetId(ClientType.Agent);
            Console.WriteLine($"Player initialized");
        }
        
        /// <summary>
        /// return a pair of the new position, 
        /// the method should test every possible position whether it is ocupied by other player or not, by asking the server about it
        /// </summary>
        /// <returns></returns>
        
        public Tuple<int, int> Move()   
        {
            int []dx = {-1, 0, 1, 0};
            int []dy = {0, 1, 0, -1};
            Random r = new Random();
            while (true)
            {
                int idx = r.Next(4);
                int newX = _position.Item1 + dx[idx];
                int newY = _position.Item2 + dy[idx];
                if (newX >= 0 && newX < 10 && newY >= 0 && newY < 10/* && (newX, newY) are not ocupied */ ) // 10 should subtituted by the board size
                {
                    _position = new Tuple<int, int>(newX, newY);

                    return new Tuple<int, int>(newX, newY);
                }
            }
        }

        public override void HandleReceivePacket(Packet receivedPacket)
        {
            if(receivedPacket.RequestType == RequestType.Register) {

                SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
                Console.WriteLine($"Player ID set to {Id}");
                gameMasterId = int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.GameMasterId]);

                var joinTeam = new Packet(GetId(), teamLeaderId, RequestType.Send);
                joinTeam.AddArgument(ServerConstants.ArgumentNames.RegisterToTeamLeader, GetId().ToString());
                SendPacket(joinTeam);

                var askForBoard = new Packet(GetId(), gameMasterId, RequestType.Send);
                askForBoard.AddArgument(ServerConstants.ArgumentNames.GameBoardSize, "0");
                SendPacket(askForBoard);

            } else {
                //TODO - handle something received from another entit
                if (receivedPacket.Arguments.ContainsKey(ServerConstants.ArgumentNames.GameBoardSize))
                {
                    var k = receivedPacket.Arguments[ServerConstants.ArgumentNames.GameBoardSize];
                    gameBoard = new Board((int)k.Width, (int)k.Height, (int)k.GoalAreaHeight);
                    Console.WriteLine($"game board of size {k.Width}x{k.Height}x{k.GoalAreaHeight} added to {GetId()}");
                } else
                {
                    Console.WriteLine($"Player recieved packet ");
                }
            }
        }
    }
}
