using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFW.Model.Armory
{
    public class Faction
    {
        public string Name { get; set; }
        public string Color { get; set; }

        // Probably dont need this.
        //public List<Coalition> Coalitions { get; }

        public Faction()
        {
            //Coalitions = new List<Coalition>();
        }
    }
}
