using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Shipment
    {
        [JsonPropertyName("manifested")]
        public bool Manifested { get; set; }
        [JsonPropertyName("has_label")]
        public bool HasLabel { get; set; }
        [JsonPropertyName("shipment_id")]
        public string ShipmentId { get; set; }
        [JsonPropertyName("order_number")]
        public string OrderNumber { get; set; }
        [JsonPropertyName("shipment_reference")]
        public string ShipmentReference { get; set; }
        [JsonPropertyName("sender_references")]
        public string SenderReferences { get; set; }
        [JsonPropertyName("shipment_creation_date")]
        public DateTime ShipmentCreationDate { get; set; }
        [JsonPropertyName("shipment_modified_date")]
        public DateTime ShipmentModifiedDate { get; set; }
        [JsonPropertyName("label")]
        public Uri Label { get; set; }

        [JsonPropertyName("items")]
        public IEnumerable<ShipmentItem> ShipmentItems { get; set; }

        public string ManifestFileName => $"shipment-manifest-{ShipmentOrder}.pdf";

        [JsonPropertyName("shipment_order")]

        public ShipmentOrder ShipmentOrder { get; set; }
        
        [JsonPropertyName("product_id")]
        public string PostageProductId { get; set; }

        [JsonPropertyName("total_weight")]
        public decimal TotalWeight { get; set; }
    }
}
