using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.AusPost
{
    public class CreateDomesticShipmentRequest
    {
        [JsonProperty("shipments")]
        public List<ShipItem> ShipItems { get; set; }
    }
}
