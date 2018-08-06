using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model.Game
{
    public class GameSession : MonoBehaviour
    {
        public Settings Settings { get; }
        public ICollection<Team> Teams { get; private set; }
        public ICollection<UnitBehaviour> AllUnitsIngame { get; private set; }

        public void Awake()
        {
            //Settings = settings;
            Teams = new List<Team>();

            AllUnitsIngame = new List<UnitBehaviour>();
        }
    }
}
