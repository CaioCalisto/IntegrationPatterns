using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Messages.Out
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }

        public Order(int orderId, int customerId, double amount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            Amount = amount;
        }
    }
}
