using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    class Player
    {
        Tuple<int, int> position;
        Team myTeam;
        public Player(int i, int j)
        {
            position = new Tuple<int, int>(i, j);
            myTeam = null;
        }
        public void setTeam(Team myTeam)
        {
            this.myTeam = myTeam;
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


    }
}
