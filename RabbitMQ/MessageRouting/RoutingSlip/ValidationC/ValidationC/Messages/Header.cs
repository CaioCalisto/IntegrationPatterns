using System;

namespace ValidationC.Messages
{
    public class Header
    {
        public RoutingSlip RoutingSlip { get; set; }
        public bool Success { get; set; }
        public DateTime Date { get; set; }
        
    }
}
