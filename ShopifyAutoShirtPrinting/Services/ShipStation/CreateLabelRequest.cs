namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class CreateLabelRequest
    {
        public long OrderId { get; set; }
        public string CarrierCode { get; set; }
        public string ServiceCode { get; set; }
        public string PackageCode { get; set; }
        public string Confirmation { get; set; }
        public string ShipDate { get; set; }
        public Weight Weight { get; set; }
        public Dimensions Dimensions { get; set; }
        public InsuranceOptions InsuranceOptions { get; set; }
        public InternationalOptions InternationalOptions { get; set; }
        public AdvancedOptions AdvancedOptions { get; set; }

    }
}
