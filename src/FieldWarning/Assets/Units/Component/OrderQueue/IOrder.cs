using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public interface IOrder
    {
        bool OrderComplete();
        void Deactivate();
        void ProcessWaypoint();
        Vector3 Destination { get; }
    }
}