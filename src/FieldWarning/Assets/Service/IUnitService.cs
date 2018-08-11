using System.Collections.Generic;
using PFW.Model.Armory;

namespace PFW.Service
{
    public interface IUnitService : IService<Unit>
    {
        ICollection<Unit> ByCategory(UnitCategory category);
        ICollection<Unit> ByCoalition(Coalition coalition);
        ICollection<Unit> ByFaction(Faction faction);
    }
}