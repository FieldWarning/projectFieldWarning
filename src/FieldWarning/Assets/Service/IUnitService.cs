using System.Collections.Generic;
using Assets.Model.Armory;

namespace Assets.Service
{
    public interface IUnitService : IService<Unit>
    {
        ICollection<Unit> ByCategory(UnitCategory category);
        ICollection<Unit> ByCoalition(Coalition coalition);
        ICollection<Unit> ByFaction(Faction faction);
    }
}