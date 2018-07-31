using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Armory;

namespace Assets.Model.Profile
{
    public class Deck
    {
        public string Name { get; set; }
        public Coalition Coalition { get; set; }

        public List<Unit> Units { get; set; }
    }
}
