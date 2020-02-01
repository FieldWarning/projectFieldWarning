using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public abstract class OrderBase : IOrder
    {
        public abstract bool OrderComplete();

        public virtual void Deactivate()
        {
        }

        public abstract void ProcessWaypoint();

        public abstract Vector3 Destination { get; }
    }
}