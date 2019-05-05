using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Messages.Out
{
    public class Order
    {
        public int CustomerId { get; set; }
        public double Amount { get; set; }
    }
}
