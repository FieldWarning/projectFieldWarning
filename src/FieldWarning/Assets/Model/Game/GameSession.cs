using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Game
{
    public class GameSession
    {
        public Settings Settings { get; }

        public ICollection<Team> Teams { get; }

        public GameSession(Settings settings)
        {
            Settings = settings;
            Teams = new List<Team>();
        }
    }
}
