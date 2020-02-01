using System.Linq;
using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public sealed class FirePositionOrder : OrderBase
    {
        private readonly Vector3 _targetPosition;
        private readonly PlatoonBehaviour _platoon;

        public FirePositionOrder(Vector3 targetPosition, PlatoonBehaviour platoon)
        {
            _targetPosition = targetPosition;
            _platoon = platoon;
        }
        
        public override bool OrderComplete()
        {
            return _platoon.Units.All(u => !u.HasTarget);
        }

        public override void ProcessWaypoint()
        {
            _platoon.Units.ForEach(u => u.SendFirePosOrder(_targetPosition));
            _platoon.PlayAttackCommandVoiceline();
        }

        public override Vector3 Destination => _targetPosition;
    }
}