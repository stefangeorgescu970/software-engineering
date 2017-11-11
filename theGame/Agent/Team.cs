using System.Collections.Generic;

namespace Agent
{
    class Team
    {
        List<Player> players;
        Player _teamLeader;

        public Team(List<Player> players)
        {
	        foreach (Player t in players)
	        {
		        this.players.Add(t);
		        t.SetTeam(this);
	        }
        }
        public Team(List<Player> players, Player teamLeader)
        {
	        foreach (Player t in players)
	        {
		        this.players.Add(t);
		        t.SetTeam(this);
	        }
	        this._teamLeader = teamLeader;
        }
        public void SetTeamLeader(Player teamLeader){
            this._teamLeader = teamLeader;
        }
    }
}
