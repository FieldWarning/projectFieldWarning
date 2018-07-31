using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Armory;
using UnityEngine;

namespace Assets.Service
{
    public class FactionService : MonoBehaviour, IFactionService
    {
        private ICollection<Faction> _factions;
        private ICollection<Coalition> _coalitions;

        public void Awake()
        {
            _factions = new List<Faction>();
            _coalitions = new List<Coalition>();

            var nato = new Faction() { Name = "NATO", Color = "Blue" };
            var wapa = new Faction() { Name = "Warsaw Pact", Color = "Red" };

            _factions.Add(nato);
            _factions.Add(wapa);

            _coalitions.Add(new Coalition() { Name = "USA", Faction = nato });
            _coalitions.Add(new Coalition() { Name = "USSR", Faction = wapa });
        }

        public ICollection<Faction> All()
        {
            return _factions;
        }

        public ICollection<Coalition> AllCoalitions()
        {
            return _coalitions;
        }

        public ICollection<Coalition> AllByFaction(Faction faction)
        {
            return _coalitions.Where(c => c.Faction == faction).ToList();
        }
    }
}

