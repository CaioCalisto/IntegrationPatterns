using System;
using System.Collections.Generic;

namespace Order.Message
{
    public class Order
    {
        private List<OrderItem> orderItems;
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Customer Customer { get; set; }        
        public IEnumerable<OrderItem> OrderItems
        {
            get { return this.orderItems.AsReadOnly(); }
        }
        
        private Order(int id, DateTime date, Customer customer)
        {
            Id = id;
            Date = date;
            Customer = customer;
            this.orderItems = new List<OrderItem>();
        }

        public static Order Create(int id, DateTime date, Customer customer)
        {
            return new Order(id, date, customer);
        }

        public void AddItem(int id, int quantity, Type itemType)
        {
            this.orderItems.Add(new OrderItem(id, quantity, itemType));
        }
    }
}
