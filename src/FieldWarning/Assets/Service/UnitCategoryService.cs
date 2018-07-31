using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Armory;
using UnityEngine;

namespace Assets.Service
{
    public class UnitCategoryService : MonoBehaviour, IUnitCategoryService
    {
        private  ICollection<UnitCategory> _categories;

        public void Awake()
        {
            _categories = new List<UnitCategory>();

            _categories.Add(new UnitCategory() { Name = "LOG" });
            _categories.Add(new UnitCategory() { Name = "INF" });
            _categories.Add(new UnitCategory() { Name = "SUP" });
            _categories.Add(new UnitCategory() { Name = "TNK" });
            _categories.Add(new UnitCategory() { Name = "REC" });
            _categories.Add(new UnitCategory() { Name = "VHC" });
            _categories.Add(new UnitCategory() { Name = "HEL" });
        }

        public ICollection<UnitCategory> All()
        {
            return _categories;
        }
    }
}

