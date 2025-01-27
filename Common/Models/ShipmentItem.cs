using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ShipmentItem
    {
        [JsonPropertyName("shipment_item_id")]
        public string ShipmentItemId { get; set; }
        [JsonPropertyName("article_id")]
        public string ArticleId { get; set; }
        [JsonPropertyName("consignment_id")]
        public string ConsignmentId { get; set; }
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }
        [JsonPropertyName("item_reference")]
        public string ItemReference { get; set; }
    }
}
