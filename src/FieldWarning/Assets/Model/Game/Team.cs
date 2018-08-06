using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model.Game
{
    public class Team : MonoBehaviour
    {
        public string Name;
        public Color Color;

        public List<Player> Players { get; private set; }

        public void Awake()
        {
            Players = new List<Player>();
        }
    }
}
