using ShopifyEasyShirtPrinting.Data;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.Models
{
    public class OrderInfo : EntityBase
    {
        [JsonPropertyName("BinNumber")]
        public int BinNumber { get; set; }
        [JsonPropertyName("OrderId")]
        public long OrderId { get; set; }
        [JsonPropertyName("Active")]
        public bool Active { get; set; } = true;
        [JsonPropertyName("LabelPrinted")]
        public bool LabelPrinted { get; set; }
        [JsonPropertyName("LabelData")]
        public string LabelData { get; set; }
        [JsonPropertyName("TrackingNumber")]
        public string TrackingNumber { get; set; }
        [JsonPropertyName("InsuranceCost")]
        public double InsuranceCost { get; set; }
        [JsonPropertyName("ShipmentCost")]
        public double ShipmentCost { get; set; }
        [JsonPropertyName("ShipmentId")]
        public int ShipmentId { get; set; }
    }
}
