namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class CreateLabelResponse
    {
        public int ShipmentId { get; set; }
        public double ShipmentCost { get; set; }
        public double InsuranceCost { get; set; }
        public string TrackingNumber { get; set; }
        public string LabelData { get; set; }
        public string FormData { get; set; }
    }
}
