using System;
using System.Collections.Generic;

namespace Agent
{
    public class Team
    {
        /// <summary>
        /// List of players in the team.
        /// </summary>
        readonly List<Player> _players;

        /// <summary>
        /// Team leader
        /// </summary>
        Player _teamLeader;

        /// <summary>
        /// Sets current this team for all players on the list.
        /// </summary>
        /// <param name="players">List of players in the team.</param>
        public Team(List<Player> players)
        {
            _players = players;
            foreach (var player in players)
            {
                player.SetTeam(this);
            }
        }

        /// <summary>
        /// Sets current this team for all players on the list with specified team leader.
        /// </summary>
        /// <param name="players">List of players in the team.</param>
        /// <param name="teamLeader">Team leader.</param>
        public Team(List<Player> players, Player teamLeader)
        {
            foreach (Player t in players)
            {
                _players.Add(t);
                t.SetTeam(this);
            }
            _teamLeader = teamLeader;
        }

        /// <summary>
        /// Sets a team leader for the currrent team.
        /// </summary>
        /// <param name="teamLeader">Team leader</param>
        public void SetTeamLeader(Player teamLeader)
        {
            _teamLeader = teamLeader;
        }

        /// <summary>
        /// Performs a move for the whole team.
        /// </summary>
        public void MoveAllPlayers()
        {
            foreach (var player in _players)
            {
                Console.WriteLine($"Player moved to localization {player.Move()} ");
            }
        }
    }
}