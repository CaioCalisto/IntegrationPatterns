using System.Collections.Generic;
using System.Linq;

namespace Service.DAO
{
    public class OrderItemsDao : IOrderItemsDao
    {
        private List<Services.Receiver.Response> items;

        public OrderItemsDao()
        {
            this.items = new List<Services.Receiver.Response>();
        }

        public void AddOrderItem(Services.Receiver.Response orderItem)
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
            Services.Receiver.Response order = this.items.Where(i => i.OrderId == orderId).FirstOrDefault();
            if (order == null)
                return 0;
            else
                return order.OrderLength;
        }

        public IEnumerable<Services.Receiver.Response> GetOrderItemsByOrderId(int orderId)
        {
            return this.items.Where(i => i.OrderId == orderId);
        }

        public void RemoveByOrderId(int orderId)
        {
            IEnumerable<Services.Receiver.Response> itemsToRemove = this.items.Where(i => i.OrderId == orderId);
            foreach(Services.Receiver.Response itemToRemove in itemsToRemove)
            {
                this.items.Remove(itemToRemove);
            }
        }

        public void Clear()
        {
            this.items.Clear();
            this.items = new List<Services.Receiver.Response>();
        }

    }
}
