using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.AusPost
{
    public class Shipment
    {
        [JsonProperty("shipment_reference_1")]
        public string ShipmentReference { get; set; }

        [JsonProperty("customer_reference_2")]
        public string CustomerReference1 { get; set; }

        [JsonProperty("customer_reference_2")]
        public string CustomerReference2 { get; set; }

        [JsonProperty("email_tracking_enabled")]
        public bool EmailTrackingEnabled { get; set; }

        [JsonProperty("from")]
        public Address From { get; set; }

        [JsonProperty("to")]
        public Address To { get; set; }

        [JsonProperty("items")]
        public List<ShipItem> Items { get; set; }
    }
}
