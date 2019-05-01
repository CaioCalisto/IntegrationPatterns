using System.Collections.Generic;

namespace Service.DAO
{
    public interface IOrderItemsDao
    {
        void AddOrderItem(Order orderItem);
        IEnumerable<int> GetOrderIds();
        IEnumerable<Order> GetOrderItemsByOrderId(int orderId);
        int GetOrderLength(int orderId);
        void RemoveByOrderId(int orderId);
    }
}