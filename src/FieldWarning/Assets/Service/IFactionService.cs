using System.Collections.Generic;
using Assets.Model.Armory;

namespace Assets.Service
{
    public interface IFactionService : IService<Faction>
    {
        ICollection<Coalition> AllByFaction(Faction faction);
        ICollection<Coalition> AllCoalitions();
    }
}