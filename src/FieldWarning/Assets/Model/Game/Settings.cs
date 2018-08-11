using System;

namespace PFW.Model.Game
{
    public class Settings
    {
        public int PointLimit { get; private set; }
        public TimeSpan Duration { get; private set; }
        public string Map { get; private set; }

        public long Seed { get; private set; }
    }
}
