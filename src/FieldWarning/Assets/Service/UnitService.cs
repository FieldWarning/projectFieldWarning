using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PFW.Model.Armory;
using UnityEngine;

namespace PFW.Service
{
    public class UnitService : MonoBehaviour, IUnitService
    {
        public UnitCategoryService UnitCategoryService;
        public FactionService FactionService;

        private ICollection<Unit> _units;

        public void Awake()
        {
            _units = new List<Unit>();

            UnitCategoryService.Awake();
            FactionService.Awake();

            var allCats = UnitCategoryService.All();

            //var tank = UnitCategoryService.All().SingleOrDefault(x => x.Name == "TNK");
            var usa = FactionService.AllCoalitions().SingleOrDefault(c => c.Name == "USA");

            _units.Add(new Unit() { Name = "HEMIT", Category = allCats.ElementAt(0), Coalition = usa });
            _units.Add(new Unit() { Name = "Riflemen", Category = allCats.ElementAt(1), Coalition = usa });
            _units.Add(new Unit() { Name = "Marines", Category = allCats.ElementAt(1), Coalition = usa });
            _units.Add(new Unit() { Name = "PLZ-5", Category = allCats.ElementAt(2), Coalition = usa });

            _units.Add(new Unit() { Name = "M1A2 Abrams", Category = allCats.ElementAt(3), Coalition = usa });
            _units.Add(new Unit() { Name = "M1A1 Abrams", Category = allCats.ElementAt(3), Coalition = usa });

            _units.Add(new Unit() { Name = "Army Rangers", Category = allCats.ElementAt(4), Coalition = usa });

            _units.Add(new Unit() { Name = "ARTY", Category = allCats.ElementAt(5), Coalition = usa });

            _units.Add(new Unit() { Name = "AH-64D Apache", Category = allCats.ElementAt(6), Coalition = usa });
        }

        public ICollection<Unit> All()
        {
            return _units;
        }

        public ICollection<Unit> ByFaction(Faction faction)
        {
            return _units.Where(u => u.Coalition.Faction == faction).ToList();
        }

        public ICollection<Unit> ByCoalition(Coalition coalition)
        {
            return _units.Where(u => u.Coalition == coalition).ToList();
        }

        public ICollection<Unit> ByCategory(UnitCategory category)
        {
            return _units.Where(u => u.Category == category).ToList();
        }
    }
}
