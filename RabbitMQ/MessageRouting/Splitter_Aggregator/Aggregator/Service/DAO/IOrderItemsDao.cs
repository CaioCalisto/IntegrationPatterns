using System.Collections.Generic;
using Service.Models;

namespace Service.DAO
{
    public interface IOrderItemsDao
    {
        void AddOrderItem(OrderItem orderItem);
        void Clear();
        IEnumerable<int> GetOrderIds();
        IEnumerable<OrderItem> GetOrderItemsByOrderId(int orderId);
        int GetOrderLength(int orderId);
        void RemoveByOrderId(int orderId);
    }
}