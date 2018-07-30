using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Ingame.UI
{
    public class BuyTransaction
    {
        private readonly GhostPlatoonBehaviour _ghostPlatoonBehaviour;
     
        public UnitType UnitType { get; }
        public Player Owner { get;  }

        //public event Action<GhostPlatoonBehaviour> Finished;

        //public void OnFinished(GhostPlatoonBehaviour behaviour)
        //{
        //    Finished?.Invoke(behaviour);
        //}
        
        public BuyTransaction(UnitType type, Player owner)
        {
            UnitType = type;
            Owner = owner;

            _ghostPlatoonBehaviour = GhostPlatoonBehaviour.build(type, owner, 1);
        }

        public void AddUnit()
        {
            _ghostPlatoonBehaviour.AddSingleUnit();
            _ghostPlatoonBehaviour.buildRealPlatoon();
        }

        public GhostPlatoonBehaviour Finish()
        {
            return _ghostPlatoonBehaviour;
        }
    }
}
