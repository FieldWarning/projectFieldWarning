using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Armory
{
    public class Coalition
    {
        public string Name { get; set; }
        public Faction Faction { get; set; }
        //public List<Unit> Units { get; }

        public Coalition()
        {
            //Units = new List<Unit>();
        }
    }
}
