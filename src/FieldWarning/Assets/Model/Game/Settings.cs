using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Game
{
    public class Settings
    {
        public int PointLimit { get; set; }
        public TimeSpan Duration { get; set; }
        public string Map { get; set; }

        public long Seed { get; set; }
    }
}
