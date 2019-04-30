using System;
using System.Collections.Generic;

namespace Splitter.OrderMessage
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Customer Customer { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }


        public Order(int id, DateTime date, Customer customer, IEnumerable<OrderItem> orderItems)
        {
            this.Id = id;
            this.Date = date;
            this.Customer = customer;
            this.OrderItems = orderItems;
        }

    }
}
