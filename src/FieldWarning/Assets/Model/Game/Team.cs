using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Game
{
    public class Team
    {
        public List<Player> Players { get; }

        public Team()
        {
            Players = new List<Player>();
        }
    }
}
