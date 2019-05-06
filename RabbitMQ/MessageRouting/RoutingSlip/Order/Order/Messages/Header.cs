
using System;

namespace Order.Messages
{
    public class Header
    {
        public RoutingSlip RoutingSlip { get; set; }
        public bool Success { get; set; }
        public DateTime Date { get; set; }

        public Header(RoutingSlip routingSlip, bool success, DateTime date)
        {
            RoutingSlip = routingSlip;
            Success = success;
            Date = date;
        }
    }
}
