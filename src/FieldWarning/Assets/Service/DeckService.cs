using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Profile;
using UnityEngine;

namespace Assets.Service
{
    public class DeckService : MonoBehaviour, IDeckService
    {
        private ICollection<Deck> _decks;

        //public DeckService(IFactionService factionService, IUnitService unitService)
        //{

        //}

        public void Start()
        {
            _decks = new List<Deck>();

            _decks.Add(new Deck() { Name = "US Battlegroup" });
        }

        public ICollection<Deck> All()
        {
            return _decks;
        }
    }
}
