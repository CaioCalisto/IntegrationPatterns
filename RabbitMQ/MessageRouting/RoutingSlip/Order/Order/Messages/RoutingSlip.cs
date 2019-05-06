using System.Collections.Generic;

namespace Order.Messages
{
    public class RoutingSlip
    {
        public List<string> Forward { get; set; }

        public RoutingSlip(List<string> forward)
        {
            Forward = forward;
        }
    }
}
