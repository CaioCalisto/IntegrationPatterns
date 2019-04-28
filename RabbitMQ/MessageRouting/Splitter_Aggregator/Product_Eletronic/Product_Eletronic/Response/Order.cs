using System;
using System.Collections.Generic;
using System.Text;

namespace Product_Eletronic.Response
{
    public class Order
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public bool Processed { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public int ItemSeq { get; set; }
        public int OrderLength { get; set; }

        public Order(int orderId, int itemId, bool processed, string message, DateTime date, int itemSeq, int orderLength)
        {
            this.OrderId = orderId;
            this.ItemId = itemId;
            this.Processed = processed;
            this.Message = message;
            this.Date = date;
            this.ItemSeq = itemSeq;
            this.OrderLength = orderLength;
        }
    }
}
