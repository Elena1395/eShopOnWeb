using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderItemsReserver.Models
{
    public class Order
    {
        public string BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public Address ShipToAddress { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public double FinalPrice { get; set; }
    }
}
