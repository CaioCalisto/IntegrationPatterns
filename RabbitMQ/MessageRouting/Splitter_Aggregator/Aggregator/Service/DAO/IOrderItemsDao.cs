using System.Collections.Generic;

namespace Service.DAO
{
    public interface IOrderItemsDao
    {
        void AddOrderItem(Services.Receiver.Response orderItem);
        void Clear();
        IEnumerable<int> GetOrderIds();
        IEnumerable<Services.Receiver.Response> GetOrderItemsByOrderId(int orderId);
        int GetOrderLength(int orderId);
        void RemoveByOrderId(int orderId);
    }
}