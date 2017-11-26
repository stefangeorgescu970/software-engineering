using System;
using Server;

namespace Agent
{
    /// <summary>
    /// Class representing a player. 
    /// </summary>
    public class Player : Client.Client
    {
        /// <summary>
        /// Position of a player
        /// </summary>
	    Tuple<int, int> _position;
        /// <summary>
        /// Team player belongs to.
        /// </summary>
        public Team MyTeam;

        /// <summary>
        /// Player constructor. Creates player with initial position <i,j>
        /// </summary>
        /// <param name="i">Inintial x coordinate</param>
        /// <param name="j">Ininitial y coordinate</param>
        public Player(int i, int j)
        {
            //TryConnect(5);
            _position = new Tuple<int, int>(i, j);
            MyTeam = null;
            RegisterToServerAndGetId(ClientType.Agent);
           
            Console.WriteLine($"Player with id: {Id}, initialized on location x: {i} y: {j}");
        }

        /// <summary>
        /// Function that assings a team to a player.
        /// </summary>
        /// <param name="myTeam">Team to assign</param>
        public void SetTeam(Team myTeam)
        {
            MyTeam = myTeam;
        }
        
        //The method should test every possible position whether it is ocupied by other player or not, by asking the server about it

        /// <summary>
        /// Returns a new position
        /// </summary>
        /// <returns>Tuple with new coordinates</returns>
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

        /// <summary>
        /// Sets ID of a player basing on the onformation received from the server.
        /// </summary>
        /// <param name="receivedPacket">Received packet</param>
        public override void HandleReceivePacket(Packet receivedPacket)
        {
            SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
        }
    }
}
