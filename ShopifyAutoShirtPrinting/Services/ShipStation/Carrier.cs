namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Carrier
    {
        public string Name { get; set; }


        public string Code { get; set; }

        public string AccountNumber { get; set; }

        public string RequiresFundedAccount { get; set; }

        public double Balance { get; set; }

        public string Nickname { get; set; }

        public int ShippingProviderId { get; set; }
        public bool Primary { get; set; }
    }
}
