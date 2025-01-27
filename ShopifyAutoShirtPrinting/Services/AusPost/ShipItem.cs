using Newtonsoft.Json;

namespace ShopifyEasyShirtPrinting.Services.AusPost
{
    public class ShipItem
    {
        [JsonProperty("item_reference")]
        public string ItemReference { get; set; }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }
        [JsonProperty("length")]
        public string Length { get; set; }
        [JsonProperty("height")]
        public string Height { get; set; }
        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("authority_to_leave")]
        public bool AuthorityToLeave { get; set; }


        [JsonProperty("allow_partial_delivery")]
        public bool AllowPartialDelivery { get; set; }


    }
}
