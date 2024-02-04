using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class ListOrdersResponse
    {
        [JsonPropertyName("orders")]
        public List<Order> Orders { get; set; } = new();
        public int Page { get; set; }
        public int Total { get; set; }
        public int Pages { get; set; }

        public bool HasNextPage => Page < Pages;
    }
}
