using System;
using System.IO;
using System.Runtime.CompilerServices;
using Server;

namespace Agent
{
    public class Player : Client.Client
    {
        Tuple<int, int> _position;
        public Team MyTeam;
      
        private static int count = 1;   //assign Id to every player starting from 1.  
        public Player(int i, int j)
        {
            Id = count++;
            RegisterToServerAndGetId(ClientType.Agent);
            _position = new Tuple<int, int>(i, j);
            MyTeam = null;
            Console.WriteLine($"Player with id: {Id}, initialized on location x: {i} y: {j}");
        }
        public void SetTeam(Team myTeam)
        {
            MyTeam = myTeam;
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

        public int getId()
        {
            return Id;
        }
        public override void HandleReceivePacket(Packet receivedPacket)
        {

            if (receivedPacket.RequestType == RequestType.Register)
            {
                SetId(int.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]));
            }
            else
            {
                //TODO - handle something received from another entit
            }
        }
    }
}
