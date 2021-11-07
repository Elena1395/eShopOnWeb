using System;
using System.Collections.Generic;
using System.Text;

namespace OrderItemsReserver.Models
{
    public class OrderItem
    {
        public double UnitPrice { get; set; }
        public int Units { get; set; }
        public int Id { get; set; }
        public ItemOrdered ItemOrdered { get; set; }
    }
}
