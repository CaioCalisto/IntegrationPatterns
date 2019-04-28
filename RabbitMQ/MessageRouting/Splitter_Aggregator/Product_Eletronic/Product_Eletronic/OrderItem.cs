using System;

namespace Product_Eletronic
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string CustomerId { get; set; }
        public DateTime Date { get; set; }
        public int ItemSeq { get; set; }
        public int OrderLength { get; set; }

        public OrderItem(int orderId, int itemId, int quantity, string customerId, DateTime date, int itemSeq, int orderLength)
        {
            this.OrderId = orderId;
            this.ItemId = itemId;
            this.Quantity = quantity;
            this.CustomerId = customerId;
            this.Date = date;
            this.ItemSeq = itemSeq;
            this.OrderLength = orderLength;
        }
    }
}
