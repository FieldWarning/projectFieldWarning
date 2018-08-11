using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFW.Model.Armory
{
    public class Unit
    {
        public string Name { get; set; }

        public UnitCategory Category { get; set; }
        public Coalition Coalition { get; set; }
    }
}
