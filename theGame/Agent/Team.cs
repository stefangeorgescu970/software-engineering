using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    class Team
    {
        List<Player> players;
        Player teamLeader;

        public Team(List<Player> players)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                this.players.Add(players[i]);
                players[i].setTeam(this);
            }
                
        }
        public Team(List<Player> players, Player teamLeader)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                this.players.Add(players[i]);
                players[i].setTeam(this);
            }
            this.teamLeader = teamLeader;
        }
        public void setTeamLeader(Player teamLeader){
            this.teamLeader = teamLeader;
        }
    }
}
