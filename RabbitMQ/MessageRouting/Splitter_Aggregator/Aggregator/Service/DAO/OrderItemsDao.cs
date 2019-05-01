﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.DAO
{
    public class OrderItemsDao : IOrderItemsDao
    {
        private BlockingCollection<Order> items;

        public OrderItemsDao()
        {
            this.items = new BlockingCollection<Order>();
        }

        public void AddOrderItem(Order orderItem)
        {
            if (!this.items.Any(o => o.OrderId == orderItem.OrderId && o.ItemSeq == orderItem.ItemSeq))
            {
                this.items.TryAdd(orderItem);
            }
        }

        public IEnumerable<int> GetOrderIds()
        {
            return this.items.Select(a => a.OrderId);
        }

        public int GetOrderLength(int orderId)
        {
            Order order = this.items.Where(i => i.OrderId == orderId).FirstOrDefault();
            if (order == null)
                return 0;
            else
                return order.OrderLength;
        }

        public IEnumerable<Order> GetOrderItemsByOrderId(int orderId)
        {
            return this.items.Where(i => i.OrderId == orderId);
        }

        public void RemoveByOrderId(int orderId)
        {
            IEnumerable<Order> itemsToRemove = this.items.Where(i => i.OrderId == orderId);
            foreach(Order itemToRemove in itemsToRemove)
            {
                Order removed = itemToRemove;
                this.items.TryTake(out removed);
            }
        }
    }
}
