using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.AusPost
{
    public class Address
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lines")]
        public List<string> Lines { get; set; }

        [JsonProperty("suburb")]
        public string Suburb { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postcode")]
        public string PostCode { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("business_name")]
        public string BusinessName { get; set; }
    }
}
