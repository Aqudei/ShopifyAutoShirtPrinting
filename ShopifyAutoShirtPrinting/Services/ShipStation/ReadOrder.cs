using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class ReadOrder
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderKey { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }


        public long CustomerId { get; set; }
        public string CustomerUsername { get; set; }
        public string CustomerEmail { get; set; }
        public Address BillTo { get; set; }
        public Address ShipTo { get; set; }
        public List<OrderLine> Items { get; set; } = new List<OrderLine>();

    }
}
