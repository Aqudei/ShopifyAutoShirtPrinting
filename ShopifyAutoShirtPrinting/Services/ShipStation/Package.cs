namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Package
    {
        public string CarrierCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Domestic { get; set; }
        public bool International { get; set; }
    }
}
