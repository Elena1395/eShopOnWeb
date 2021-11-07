using System;
using System.Collections.Generic;
using System.Text;

namespace OrderItemsReserver.Models
{
    public class ItemOrdered
    {
        public int CatalogItemId { get; set; }
        public string ProductName { get; set; }
        public string PictureUri { get; set; }
    }
}
