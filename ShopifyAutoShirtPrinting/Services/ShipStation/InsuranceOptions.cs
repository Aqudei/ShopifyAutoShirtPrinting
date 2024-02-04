namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class InsuranceOptions
    {
        //provider string Preferred Insurance provider.Available options: "shipsurance", "carrier", or "provider". The "provider" option is used to indicate that a shipment was insured by a third party other than ShipSurance or the carrier. The insurance is handled outside of ShipStation, and will not affect the cost of processing the label.
        //    insureShipment boolean Indicates whether shipment should be insured.
        //    insuredValue number  Value to insure.

        public string Provider { get; set; }
        public bool InsureShipment { get; set; }
        public int InsuredValue { get; set; }
    }
}
