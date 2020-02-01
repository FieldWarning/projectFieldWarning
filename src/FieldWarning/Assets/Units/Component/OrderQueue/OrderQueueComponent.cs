using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PFW.Units.Component.OrderQueue
{
    public sealed class OrderQueue
    {
        private readonly Queue<IOrder> _orders = new Queue<IOrder>();
        private IOrder _activeOrder;

        public void SendOrder(IOrder order, bool enqueue)
        {
            if (!enqueue)
                Clear();
            _orders.Enqueue(order);
        }

        public IEnumerable<IOrder> Orders => _orders;

        public IOrder ActiveOrder
        {
            get => _activeOrder;
            set
            {
                if (_activeOrder == value)
                {
                    return;
                }

                var prevOrder = _activeOrder;
                prevOrder?.Deactivate();
                _activeOrder = value;
                _activeOrder?.ProcessWaypoint();
            }
        }

        public void HandleUpdate()
        {
            if (ActiveOrder != null && !ActiveOrder.OrderComplete())
                return;

            if (_orders.Any())
            {
                ActiveOrder = _orders.Dequeue();
                ActiveOrder.ProcessWaypoint();
            }
            else
            {
                ActiveOrder = null;
            }
        }

        public void Clear()
        {
            _orders.Clear();
            ActiveOrder = null;
        }
    }
}