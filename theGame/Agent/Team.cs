using System;
using System.Collections.Generic;

namespace Agent
{
    public class Team
    {
        readonly List<Player> _players;
        Player _teamLeader;

        public Team(List<Player> players)
        {
            _players = players;
            foreach (var player in players)
            {
                player.SetTeam(this);
            }
        }

        public Team(List<Player> players, Player teamLeader)
        {
            foreach (Player t in players)
            {
                _players.Add(t);
                t.SetTeam(this);
            }
            _teamLeader = teamLeader;
        }

        public void SetTeamLeader(Player teamLeader)
        {
            _teamLeader = teamLeader;
        }

        public void MoveAllPlayers()
        {
            foreach (var player in _players)
            {
                Console.WriteLine($"Player moved to localization {player.Move()} ");
            }
        }
    }
}