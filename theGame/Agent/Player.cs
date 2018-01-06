﻿using System;
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

    public class Player : Client.Client
    {
        public Team MyTeam;
        private Tuple<int, int> location;
        private Board gameBoard;
        private int teamLeaderId;
        private int gameMasterId;

        public Player(Tuple<int, int> location, Board gameBoard)
        {
            this.location = location;
            this.gameBoard = gameBoard;
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
                    break;
                case RequestType.ConnectToGame:
                    gameMasterId = (int)receivedPacket.Arguments[ServerConstants.ArgumentNames.GameMasterId];
                    Console.WriteLine($"Player: {Id} received Game Master's id: {gameMasterId} ");
                    break;
                default:
                    Console.WriteLine("Received packet of unknown type, do nothing");
                    break;
            }
        }
    }
}
