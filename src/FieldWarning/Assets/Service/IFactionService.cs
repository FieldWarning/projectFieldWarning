using System.Collections.Generic;
using PFW.Model.Armory;

namespace PFW.Service
{
    public interface IFactionService : IService<Faction>
    {
        ICollection<Coalition> AllByFaction(Faction faction);
        ICollection<Coalition> AllCoalitions();
    }
}