using ShopifyEasyShirtPrinting.Data;

namespace ShopifyEasyShirtPrinting.Models
{
    public class OrderInfo : EntityBase
    {
        public int BinNumber { get; set; }
        public long OrderId { get; set; }
        public bool Active { get; set; } = true;
        public bool LabelPrinted { get; set; }
        public string LabelData { get; set; }
        public string TrackingNumber { get; set; }
        public double InsuranceCost { get; set; }
        public double ShipmentCost { get; set; }
        public int ShipmentId { get; set; }
    }
}
