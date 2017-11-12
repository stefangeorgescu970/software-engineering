using System;
using Server;

namespace Agent
{
    public class Player : global::Client.Client
    {
	    Tuple<int, int> position;
        public Team _myTeam;
        public Player(int i, int j)
        {
            TryConnect(5);
            RegisterToServerAndGetId(ClientType.Agent);
            position = new Tuple<int, int>(i, j);
            _myTeam = null;
            Console.WriteLine($"Player with id: {_id}, initialized on location x: {i} y: {j}");
        }
        public void SetTeam(Team myTeam)
        {
            this._myTeam = myTeam;
        }


        /* return a pair of the new position
         * the method should test every possible position whether it is ocupied by other player or not, by asking the server about it
        */
        public Tuple<int, int> Move()   
        {
            int []dx = {-1, 0, 1, 0};
            int []dy = {0, 1, 0, -1};
            Random r = new Random();
            while (true)
            {
                int idx = r.Next(4);
                int newX = position.Item1 + dx[idx];
                int newY = position.Item2 + dy[idx];
                if (newX >= 0 && newX < 10 && newY >= 0 && newY < 10/* && (newX, newY) are not ocupied */ ) // 10 should subtituted by the board size
                {
                    position = new Tuple<int, int>(newX, newY);

                    return new Tuple<int, int>(newX, newY);
                }

            }
        }


        public override void HandleReceivePacket(Packet receivedPacket)
        {
            SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
        }
    }
}
