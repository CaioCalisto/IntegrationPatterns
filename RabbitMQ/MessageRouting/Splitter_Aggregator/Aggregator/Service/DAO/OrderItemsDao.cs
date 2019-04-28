using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.DAO
{
    public class OrderItemsDao : IOrderItemsDao
    {
        private List<Models.OrderItem> items;

        public OrderItemsDao()
        {
            this.items = new List<Models.OrderItem>();
        }

        public void AddOrderItem(Models.OrderItem orderItem)
        {
            if (!this.items.Contains(orderItem))
                this.items.Add(orderItem);
        }

        public IEnumerable<int> GetOrderIds()
        {
            return this.items.Select(a => a.OrderId);
        }

        public int GetOrderLength(int orderId)
        {
            Models.OrderItem order = this.items.Where(i => i.OrderId == orderId).FirstOrDefault();
            if (order == null)
                return 0;
            else
                return order.OrderLength;
        }

        public IEnumerable<Models.OrderItem> GetOrderItemsByOrderId(int orderId)
        {
            return this.items.Where(i => i.OrderId == orderId);
        }

        public void RemoveByOrderId(int orderId)
        {
            IEnumerable<Models.OrderItem> itemsToRemove = this.items.Where(i => i.OrderId == orderId);
            foreach(Models.OrderItem itemToRemove in itemsToRemove)
            {
                this.items.Remove(itemToRemove);
            }
        }

        public void Clear()
        {
            this.items.Clear();
            this.items = new List<Models.OrderItem>();
        }

    }
}
