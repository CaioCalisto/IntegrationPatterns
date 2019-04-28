using System;

namespace Service.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public bool Processed { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

        public Order(int orderId, bool processed, string message, DateTime date)
        {
            OrderId = orderId;
            Processed = processed;
            Message = message;
            Date = date;
        }
    }
}
